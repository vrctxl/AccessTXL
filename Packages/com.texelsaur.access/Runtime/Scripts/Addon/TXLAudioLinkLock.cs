using UdonSharp;
using UnityEngine;

namespace DrakenStark
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TXLAudioLinkLock : UdonSharpBehaviour
    {
        [SerializeField] private Texel.AccessControl _accessControl = null;
        [SerializeField] private AudioLink.AudioLinkController[] _audioLinkControllers = new AudioLink.AudioLinkController[0];

        [SerializeField, HideInInspector] private VRC.SDK3.Components.VRCPickup[] _vRCPickups = new VRC.SDK3.Components.VRCPickup[0];
        [SerializeField, HideInInspector] private bool _hasPickups = false;
        [SerializeField, HideInInspector] private UnityEngine.UI.Button[] _buttons = new UnityEngine.UI.Button[0];
        [SerializeField, HideInInspector] private bool _hasButtons = false;
        [SerializeField, HideInInspector] private UnityEngine.UI.Slider[] _sliders = new UnityEngine.UI.Slider[0];
        [SerializeField, HideInInspector] private bool _hasSliders = false;
        [SerializeField, HideInInspector] private UnityEngine.UI.Toggle[] _toggles = new UnityEngine.UI.Toggle[0];
        [SerializeField, HideInInspector] private bool _hasToggles = false;

        private void Start()
        {
            SendCustomEventDelayedFrames(nameof(_lateStart), 1);
        }

        public void _lateStart()
        {
            _accessControl._Register(Texel.AccessControl.EVENT_VALIDATE, this, nameof(checkAccess));
            checkAccess();
        }

        public void checkAccess()
        {
            if (_accessControl._LocalHasAccess())
            {
                if (_hasPickups)
                {
                    for (int i = 0; i < _vRCPickups.Length; i++)
                    {
                        _vRCPickups[i].pickupable = true;
                    }
                }
                if (_hasButtons)
                {
                    for (int i = 0; i < _buttons.Length; i++)
                    {
                        _buttons[i].interactable = true;
                    }
                }
                if (_hasSliders)
                {
                    for (int i = 0; i < _sliders.Length; i++)
                    {
                        _sliders[i].interactable = true;
                    }
                }
                if (!_hasToggles)
                {
                    for (int i = 0; i < _toggles.Length; i++)
                    {
                        _toggles[i].interactable = true;
                    }
                }
            }
        }
    }
}
