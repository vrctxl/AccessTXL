
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Texel
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DebugUserList : UdonSharpBehaviour
    {
        [SerializeField] internal AccessControl[] accessControl;

        [Header("UI")]
        [SerializeField] internal GameObject container;
        [SerializeField] internal GameObject textPrefab;

        private Text[] textFields;
        private VRCPlayerApi[] players;

        private bool updateQueued = false;

        void Start()
        {
            if (!container || !textPrefab)
                return;

            players = new VRCPlayerApi[100];
            textFields = new Text[accessControl.Length];

            for (int i = 0; i < accessControl.Length; i++)
            {
                if (!accessControl[i])
                    continue;

                accessControl[i]._Register(AccessControl.EVENT_VALIDATE, this, nameof(_InternalOnValidate));

                GameObject prefab = Instantiate(textPrefab, container.transform, false);
                textFields[i] = prefab.GetComponentInChildren<Text>();
            }

            _InternalOnValidate();
        }

        public void _InternalOnValidate()
        {
            if (!updateQueued)
            {
                updateQueued = true;
                SendCustomEventDelayedSeconds(nameof(_InternalRefresh), 1);
            }
        }

        public void _InternalRefresh()
        {
            updateQueued = false;
            _RefreshAll();
        }

        void _RefreshAll()
        {
            int playerCount = VRCPlayerApi.GetPlayerCount();
            players = VRCPlayerApi.GetPlayers(players);

            for (int i = 0; i < accessControl.Length; i++)
                _Refresh(accessControl[i], textFields[i], playerCount);
        }

        void _Refresh(AccessControl acl, Text field, int playerCount)
        {
            if (!acl || !field)
                return;

            field.text = $"[{acl.gameObject.name}]\n\n";

            for (int j = 0; j < playerCount; j++)
            {
                if (acl._HasAccess(players[j]))
                    field.text += $"{players[j].displayName}\n";
            }
        }
    }
}
