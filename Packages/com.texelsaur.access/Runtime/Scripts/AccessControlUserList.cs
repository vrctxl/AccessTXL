
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace Texel
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AccessControlUserList : AccessControlUserSource
    {
        public string[] userList;

        private DataDictionary userDict;

        protected override void _Init()
        {
            base._Init();

            userDict = new DataDictionary();
            _SetUserDict(userList);
        }

        void _SetUserDict(string[] names)
        {
            userDict.Clear();
            for (int i = 0; i < userList.Length; i++)
            {
                if (userList[i] != "" && !userDict.ContainsKey(userList[i]))
                    userDict.Add(userList[i], userList[i]);
            }
        }

        public override bool _ContainsName(string name)
        {
            return userDict.ContainsKey(name);
        }

        public string[] UserList
        {
            get { return userList; }
            set {
                userList = value;
                if (userList == null)
                    userList = new string[0];

                _SetUserDict(userList);
            }
        }
    }
}
