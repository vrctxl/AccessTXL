﻿
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
        public bool loadRemoteOnStart = true;
        public bool allowManualRefresh = false;
        public bool allowPeriodicRefresh = false;
        public float refreshPeriod = 1800;
        public DebugLog debugLog;
        public bool debugLogging = false;

        public string[] userList = new string[0];

        [UdonSynced]
        int syncRefreshCount = 0;

        protected override void _Init()
        {
            base._Init();

            if (loadRemoteOnStart)
                _LoadFromRemote();

            if (allowPeriodicRefresh && refreshPeriod > 0)
                SendCustomEventDelayedSeconds(nameof(_InternalPeriodicRefresh), refreshPeriod);
        }

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
            set
            {
                userList = value;
                if (userList == null)
                    userList = new string[0];
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

            if (remoteStringFormat == ACLListFormat.Newline)
                _LoadNewlineData(result.Result);
            else if (remoteStringFormat == ACLListFormat.JSONArray)
                _LoadJsonArrayData(result.Result);

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
        }

        public void _LoadJsonArrayData(string data)
        {
            userList = new string[0];
            if (data == null)
                return;

            bool hasObjectPath = jsonObjectPath == null || jsonObjectPath == "";
            bool hasEntryPath = jsonEntryPath == null || jsonEntryPath == "";

            int jsonStart = 0;
            if (hasObjectPath)
                jsonStart = data.IndexOf('[');
            else
                jsonStart = data.IndexOf('{');

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
                        _DebugLog("Expected object token");
                        return;
                    }

                    if (target.DataDictionary.TryGetValue(part, out DataToken nextToken))
                        target = nextToken;
                    else
                    {
                        _DebugLog($"Could not get object key: {part}");
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
                    string[] pathParts = jsonObjectPath.Split('/');
                    foreach (string part in pathParts)
                    {
                        if (leaf.TokenType != TokenType.DataDictionary)
                        {
                            _DebugLog("Expected object token");
                            return;
                        }

                        if (leaf.DataDictionary.TryGetValue(part, out DataToken nextToken))
                            leaf = nextToken;
                        else
                        {
                            _DebugLog($"Could not get object key: {part}");
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
