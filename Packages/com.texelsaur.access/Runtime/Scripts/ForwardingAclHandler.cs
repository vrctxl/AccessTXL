
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Texel
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ForwardingAclHandler : AccessControlHandler
    {
        public AccessControl[] forwardAcls;

        protected override void _Init()
        {
            base._Init();

            foreach (var acl in forwardAcls)
            {
                if (acl)
                    acl._Register(AccessControl.EVENT_VALIDATE, this, nameof(_OnValidate));
            }
        }

        public override AccessHandlerResult _CheckAccess(VRCPlayerApi player)
        {
            for (int i = 0; i < forwardAcls.Length; i++)
            {
                if (!Utilities.IsValid(forwardAcls[i]))
                    continue;

                if (forwardAcls[i]._HasAccess(player))
                {
                    return AccessHandlerResult.Allow;
                }
            }

            return AccessHandlerResult.Pass;
        }

        public void _OnValidate()
        {
            _UpdateHandlers(EVENT_REVALIDATE);
        }
    }
}
