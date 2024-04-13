
using System;
using Texel;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace Texel
{
    public enum ACLListFormat {
        Newline,
        JSONArray,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AccessControlRemoteUserList : AccessControlUserSource
    {
        public AccessControl accessControl;
        public VRCUrl remoteStringUrl;
        public ACLListFormat remoteStringFormat;
        public string jsonObjectPath;
        public string jsonEntryPath;
        public DataValidator dataValidator;
        public bool loadRemoteOnStart = true;
        public float startDelay = 0;
        public bool allowManualRefresh = false;
        public bool allowPeriodicRefresh = false;
        public float refreshPeriod = 1800;
        public DebugLog debugLog;
        public bool debugLogging = false;

        public string[] userList = new string[0];

        [UdonSynced]
        int syncRefreshCount = 0;

        DataDictionary userDict;

        protected override void _Init()
        {
            base._Init();

            userDict = new DataDictionary();
            _SetUserDict(userList);

            if (loadRemoteOnStart)
            {
                if (startDelay > 0)
                    SendCustomEventDelayedSeconds(nameof(_LocalRefresh), startDelay);
                else
                    _LocalRefresh();
            }

            if (allowPeriodicRefresh && refreshPeriod > 0)
                SendCustomEventDelayedSeconds(nameof(_InternalPeriodicRefresh), refreshPeriod + startDelay);
        }

        void _SetUserDict(string[] names)
        {
            _EnsureInit();

            userDict.Clear();
            for (int i = 0; i < userList.Length; i++)
            {
                if (userList[i] != "" && !userDict.ContainsKey(userList[i]))
                    userDict.Add(userList[i], userList[i]);
            }
        }

        public override bool _ContainsName(string name)
        {
            _EnsureInit();

            return userDict.ContainsKey(name);
        }

        public string[] UserList
        {
            get { return userList; }
            set
            {
                userList = value;
                if (userList == null)
                    userList = new string[0];

                _SetUserDict(userList);
            }
        }

        public int RefreshCount
        {
            get { return syncRefreshCount; }
            set
            {
                if (allowManualRefresh && (!loadRemoteOnStart || syncRefreshCount > 0))
                    _LoadFromRemote();

                syncRefreshCount = value;
            }
        }

        public void _LocalRefresh()
        {
            _LoadFromRemote();
        }

        public void _SyncRefresh()
        {
            if (!_AccessCheck())
                return;

            RefreshCount += 1;
            RequestSerialization();
        }

        public void _InternalPeriodicRefresh()
        {
            if (Networking.IsOwner(gameObject))
            {
                RefreshCount += 1;
                RequestSerialization();
            }

            SendCustomEventDelayedSeconds(nameof(_InternalPeriodicRefresh), refreshPeriod);
        }

        void _LoadFromRemote()
        {
            VRCStringDownloader.LoadUrl(remoteStringUrl, (UdonBehaviour)(Component)this);
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            _DebugLog(result.Error);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            _DebugLog($"Received data {result.Result.Length} characters");

            string data = result.Result;

            if (dataValidator)
            {
                if (!dataValidator._PreValidate(data))
                {
                    _DebugLog("Pre-validation failed");
                    return;
                }

                data = dataValidator._Transform(data);
                if (!dataValidator._PostValidate(data))
                {
                    _DebugLog("Post-validation failed");
                    return;
                }
            }

            if (remoteStringFormat == ACLListFormat.Newline)
                _LoadNewlineData(data);
            else if (remoteStringFormat == ACLListFormat.JSONArray)
                _LoadJsonArrayData(data);

            _UpdateHandlers(EVENT_REVALIDATE);
        }

        public void _LoadNewlineData(string data)
        {
            if (data == null)
            {
                userList = new string[0];
                return;
            }

            userList = data.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            _SetUserDict(userList);
        }

        public void _LoadJsonArrayData(string data)
        {
            userList = new string[0];
            _SetUserDict(userList);
 
            if (data == null)
                return;

            bool hasObjectPath = jsonObjectPath != null && jsonObjectPath != "";
            bool hasEntryPath = jsonEntryPath != null && jsonEntryPath != "";

            int jsonStart = 0;
            if (hasObjectPath)
                jsonStart = data.IndexOf('{');
            else
                jsonStart = data.IndexOf('[');

            if (jsonStart > 0)
                data = data.Substring(jsonStart);

            DataToken root;
            if (!VRCJson.TryDeserializeFromJson(data, out root))
            {
                _DebugLog("Unexpected JSON data");
                return;
            }

            DataToken target = root;
            if (hasObjectPath)
            {
                string[] pathParts = jsonObjectPath.Split('/');
                foreach (string part in pathParts)
                {
                    if (target.TokenType != TokenType.DataDictionary)
                    {
                        _DebugLog($"Expected object token on object path, got {target.TokenType}");
                        return;
                    }

                    if (target.DataDictionary.TryGetValue(part, out DataToken nextToken))
                        target = nextToken;
                    else
                    {
                        _DebugLog($"Could not get object key on object path: {part}");
                        return;
                    }
                }
            }

            if (target.TokenType != TokenType.DataList)
            {
                _DebugLog($"Expected list token at end of JSON path: {jsonObjectPath}");
                return;
            }

            DataList list = target.DataList;
            userList = new string[list.Count];
            int nextIndex = 0;

            for (int i = 0; i < list.Count; i++)
            {
                DataToken entry;
                if (!list.TryGetValue(i, out entry))
                {
                    _DebugLog("Couldn't get list entry");
                    continue;
                }

                DataToken leaf = entry;
                if (hasEntryPath)
                {
                    string[] pathParts = jsonEntryPath.Split('/');
                    foreach (string part in pathParts)
                    {
                        if (leaf.TokenType != TokenType.DataDictionary)
                        {
                            _DebugLog($"Expected object token on entry path, got {leaf.TokenType}");
                            return;
                        }

                        if (leaf.DataDictionary.TryGetValue(part, out DataToken nextToken))
                            leaf = nextToken;
                        else
                        {
                            _DebugLog($"Could not get object key on entry path: {part}");
                            return;
                        }
                    }
                }

                if (leaf.TokenType != TokenType.String)
                {
                    _DebugLog("Skipping non-string element");
                    continue;
                }

                userList[nextIndex] = leaf.String;
                nextIndex += 1;
            }

            if (userList.Length < nextIndex)
            {
                string[] compactList = new string[nextIndex];
                Array.Copy(userList, compactList, nextIndex);
                userList = compactList;
            }

            _SetUserDict(userList);
        }

        void _DebugLog(string message)
        {
            if (debugLogging)
                Debug.Log("[AccessTXL:RemoteWhitelist] " + message);
            if (debugLog)
                debugLog._Write("RemoteWhitelist", message);
        }

        public override void OnPostSerialization(SerializationResult result)
        {
            //DebugLowLevel($"PostSerialize: {result.success}, {result.byteCount} bytes");
        }

        public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
            if (!accessControl)
                return true;

            bool requesterCheck = accessControl._HasAccess(requestingPlayer) || Networking.IsOwner(requestingPlayer, gameObject);
            bool requesteeCheck = accessControl._HasAccess(requestedOwner);

            //DebugLowLevel($"Ownership check: requester={requesterCheck}, requestee={requesteeCheck}");

            return requesterCheck && requesteeCheck;
        }

        bool _AccessCheck()
        {
            if (accessControl && !accessControl._LocalHasAccess())
                return false;

            if (!Networking.IsOwner(gameObject))
            {
                //if (!allowOwnershipTransfer)
                //    return false;

                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }

            return true;
        }
    }
}
