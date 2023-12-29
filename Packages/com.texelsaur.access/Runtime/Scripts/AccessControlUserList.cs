
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Texel
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AccessControlUserList : AccessControlUserSource
    {
        public string[] userList;

        public override bool _ContainsName(string name)
        {
            for (int i = 0; i < userList.Length; i++)
            {
                if (userList[i] == name)
                    return true;
            }

            return false;
        }

        public string[] UserList
        {
            get { return userList; }
            set {
                userList = value;
                if (userList == null)
                    userList = new string[0];
            }
        }
    }
}
