using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UnityEditor;
using UnityEngine;

namespace Texel
{
    [InitializeOnLoad]
    public class MenuAccessTXL
    {
        [MenuItem("Tools/TXL/AccessTXL/Add \"Access Control\" Prefab to Scene", false)]
        [MenuItem("GameObject/TXL/AccessTXL/Access Control", false, 100)]
        public static void AddSyncPlayerToScene()
        {
            Selection.activeObject = AddPrefabToActiveOrScene("Packages/com.texelsaur.common/Runtime/Prefabs/Access Control.prefab");
        }

        [MenuItem("GameObject/TXL/AccessTXL/Access Prefabs/Access Grant Controller", false, 200)]
        public static void AddAccessGrantToScene()
        {
            AccessControl acl = GetObjectOrParent<AccessControl>();
            if (!acl)
            {
                int result = EditorUtility.DisplayDialogComplex("Access Control Object Not Selected", "Adding this controller prefab with an Access Control object selected in the scene will automatically configure the two objects.\n\nYou can add the prefab by itself if you want to configure it manually.", "Place prefab anyway", "Cancel", "Create with new Access Control");
                if (result == 1)
                    return;
                else if (result == 0)
                {
                    Selection.activeObject =  AddPrefabToActiveOrScene("Packages/com.texelsaur.access/Runtime/Prefabs/Access Grant Controller.prefab");
                    return;
                }

                GameObject aclPrefab = AddPrefabToActiveOrScene("Packages/com.texelsaur.common/Runtime/Prefabs/Access Control.prefab");
                acl = aclPrefab.GetComponent<AccessControl>();
            }

            AccessControlDynamicUserList whitelist = GetObjectOrParent<AccessControlDynamicUserList>();
            if (!whitelist)
            {
                GameObject whitelistPrefab = AddPrefabToObject("Packages/com.texelsaur.access/Runtime/Prefabs/User Lists/Dynamic Whitelist.prefab", acl.transform);
                whitelist = whitelistPrefab.GetComponent<AccessControlDynamicUserList>();
            }

            GameObject grantPrefab = AddPrefabToObject("Packages/com.texelsaur.access/Runtime/Prefabs/Access Grant Controller.prefab", acl.transform);
            DynamicWhitelistGrant grant = grantPrefab.GetComponent<DynamicWhitelistGrant>();

            grant.grantACL = acl;
            grant.dynamicList = whitelist;

            acl.allowWhitelist = true;
            if (!acl.whitelistSources.Contains(whitelist))
                acl.whitelistSources = acl.whitelistSources.Append(whitelist).ToArray();

            Selection.activeObject = grantPrefab;
        }

        [MenuItem("GameObject/TXL/AccessTXL/Access Prefabs/Access Keypad Controller", false, 201)]
        public static void AddAccessKeypadToScene()
        {
            AccessControl acl = GetObjectOrParent<AccessControl>();
            if (!acl)
            {
                Selection.activeObject = AddPrefabToActiveOrScene("Packages/com.texelsaur.access/Runtime/Prefabs/Access Keypad Controller.prefab");
                return;
            }

            AccessControlDynamicUserList whitelist = GetObjectOrParent<AccessControlDynamicUserList>();
            if (!whitelist)
            {
                GameObject whitelistPrefab = AddPrefabToObject("Packages/com.texelsaur.access/Runtime/Prefabs/User Lists/Dynamic Whitelist.prefab", acl.transform);
                whitelist = whitelistPrefab.GetComponent<AccessControlDynamicUserList>();
            }

            GameObject keypadPrefab = AddPrefabToObject("Packages/com.texelsaur.access/Runtime/Prefabs/Access Keypad Controller.prefab", acl.transform);
            AccessKeypad keypad = keypadPrefab.GetComponent<AccessKeypad>();

            keypad.whitelistCodes = new string[] { "0000" };
            keypad.dynamicLists = new AccessControlDynamicUserList[] { whitelist };

            acl.allowWhitelist = true;
            if (!acl.whitelistSources.Contains(whitelist))
                acl.whitelistSources = acl.whitelistSources.Append(whitelist).ToArray();

            Selection.activeObject = keypadPrefab;
        }

        [MenuItem("GameObject/TXL/AccessTXL/Whitelist Sources/Static Whitelist", false, 210)]
        public static void AddStaticWhitelistToScene()
        {
            AccessControl acl = GetObjectOrParent<AccessControl>();
            if (!acl)
            {
                if (!EditorUtility.DisplayDialog("Access Control Object Not Selected", "An Access Control object was not selected to add this whitelist to.", "Add whitelist anyway", "Cancel"))
                    return;
            }

            GameObject whitelistPrefab = AddPrefabToActiveOrScene("Packages/com.texelsaur.access/Runtime/Prefabs/User Lists/Static Whitelist.prefab");
            AccessControlUserList whitelist = whitelistPrefab.GetComponent<AccessControlUserList>();

            acl.allowWhitelist = true;
            if (acl)
                acl.whitelistSources = acl.whitelistSources.Append(whitelist).ToArray();

            Selection.activeObject = whitelistPrefab;
        }

        [MenuItem("GameObject/TXL/AccessTXL/Whitelist Sources/Remote Whitelist", false, 210)]
        public static void AddRemoteWhitelistToScene()
        {
            AccessControl acl = GetObjectOrParent<AccessControl>();
            if (!acl)
            {
                if (!EditorUtility.DisplayDialog("Access Control Object Not Selected", "An Access Control object was not selected to add this whitelist to.", "Add whitelist anyway", "Cancel"))
                    return;
            }

            GameObject whitelistPrefab = AddPrefabToActiveOrScene("Packages/com.texelsaur.access/Runtime/Prefabs/User Lists/Remote Whitelist.prefab");
            AccessControlRemoteUserList whitelist = whitelistPrefab.GetComponent<AccessControlRemoteUserList>();

            acl.allowWhitelist = true;
            if (acl)
                acl.whitelistSources = acl.whitelistSources.Append(whitelist).ToArray();

            Selection.activeObject = whitelistPrefab;
        }

        [MenuItem("GameObject/TXL/AccessTXL/Whitelist Sources/Dynamic Whitelist", false, 210)]
        public static void AddDynamicWhitelistToScene()
        {
            AccessControl acl = GetObjectOrParent<AccessControl>();
            if (!acl)
            {
                if (!EditorUtility.DisplayDialog("Access Control Object Not Selected", "An Access Control object was not selected to add this whitelist to.", "Add whitelist anyway", "Cancel"))
                    return;
            }

            GameObject whitelistPrefab = AddPrefabToActiveOrScene("Packages/com.texelsaur.access/Runtime/Prefabs/User Lists/Dynamic Whitelist.prefab");
            AccessControlDynamicUserList whitelist = whitelistPrefab.GetComponent<AccessControlDynamicUserList>();

            acl.allowWhitelist = true;
            if (acl)
                acl.whitelistSources = acl.whitelistSources.Append(whitelist).ToArray();

            EditorUtility.DisplayDialog("Dynamic Whitelist", "The newly added dynamic whitelist won't do anything on its own.\n\nSelect the whitelist and try adding one of the Access Prefabs from the menu.", "OK");

            Selection.activeObject = whitelistPrefab;
        }

        public static GameObject AddPrefabToScene(string path)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);

                EditorGUIUtility.PingObject(instance);
                return instance;
            }

            return null;
        }

        public static GameObject AddPrefabToActiveOrScene(string path)
        {
            GameObject active = GetActiveObject();
            if (active)
                return AddPrefabToObject(path, active.transform);
            else
                return AddPrefabToScene(path);
        }

        public static GameObject AddPrefabToObject(string path, Transform parent)
        {
            if (!parent)
                return null;

            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!asset)
                return null;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            instance.transform.parent = parent;

            EditorGUIUtility.PingObject(instance);
            return instance;
        }

        public static T GetObjectOrParent<T>() where T : UdonSharpBehaviour
        {
            if (!Selection.activeTransform)
                return null;
            T com = Selection.activeTransform.GetComponent<T>();
            if (!com)
                com = Selection.activeTransform.GetComponentInParent<T>();

            return com;
        }

        public static GameObject GetActiveObject()
        {
            if (!Selection.activeTransform)
                return null;

            return Selection.activeTransform.gameObject;
        }
    }
}
