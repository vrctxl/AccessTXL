using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Texel
{
    [InitializeOnLoad]
    public class MenuAccessTXL
    {
        [MenuItem("Tools/TXL/AccessTXL/Add \"Access Control\" Prefab to Scene", false)]
        [MenuItem("GameObject/TXL/AccessTXL/Access Control", false, 100)]
        public static void AddAccessControlToScene()
        {
            Undo.SetCurrentGroupName("Add Access Control");
            int undoGroup = Undo.GetCurrentGroup();

            Selection.activeObject = MenuUtil.AddPrefabToActiveOrScene("Packages/com.texelsaur.common/Runtime/Prefabs/Access Control.prefab");

            Undo.CollapseUndoOperations(undoGroup);
        }

        [MenuItem("GameObject/TXL/AccessTXL/Access Prefabs/Access Grant Controller", false, 200)]
        public static void AddAccessGrantToScene()
        {
            Undo.SetCurrentGroupName("Add Access Grant");
            int undoGroup = Undo.GetCurrentGroup();

            AccessControl acl = MenuUtil.GetObjectOrParent<AccessControl>();
            if (!acl)
            {
                int result = EditorUtility.DisplayDialogComplex("Access Control Object Not Selected", "Adding this controller prefab with an Access Control object selected in the scene will automatically configure the two objects.\n\nYou can add the prefab by itself if you want to configure it manually.", "Place prefab anyway", "Cancel", "Create with new Access Control");
                if (result == 0 || result == 1)
                {
                    if (result == 0)
                        Selection.activeObject = MenuUtil.AddPrefabToActiveOrScene("Packages/com.texelsaur.access/Runtime/Prefabs/Access Grant Controller.prefab");

                    Undo.CollapseUndoOperations(undoGroup);
                    return;
                }

                GameObject aclPrefab = MenuUtil.AddPrefabToActiveOrScene("Packages/com.texelsaur.common/Runtime/Prefabs/Access Control.prefab");
                acl = aclPrefab.GetComponent<AccessControl>();
            }

            AccessControlDynamicUserList whitelist = MenuUtil.GetObjectOrParent<AccessControlDynamicUserList>();
            if (!whitelist)
            {
                GameObject whitelistPrefab = MenuUtil.AddPrefabToObject("Packages/com.texelsaur.access/Runtime/Prefabs/User Lists/Dynamic Whitelist.prefab", acl.transform);
                whitelist = whitelistPrefab.GetComponent<AccessControlDynamicUserList>();
            }

            GameObject grantPrefab = MenuUtil.AddPrefabToObject("Packages/com.texelsaur.access/Runtime/Prefabs/Access Grant Controller.prefab", acl.transform);
            DynamicWhitelistGrant grant = grantPrefab.GetComponent<DynamicWhitelistGrant>();

            Undo.RecordObject(grant, "Update Whitelist Grant");
            Undo.RecordObject(acl, "Update ACL");

            grant.grantACL = acl;
            grant.dynamicList = whitelist;

            acl.allowWhitelist = true;
            if (!acl.whitelistSources.Contains(whitelist))
                acl.whitelistSources = acl.whitelistSources.Append(whitelist).ToArray();

            EditorUtility.SetDirty(acl);
            EditorUtility.SetDirty(grant);

            Selection.activeObject = grantPrefab;

            Undo.CollapseUndoOperations(undoGroup);
        }

        [MenuItem("GameObject/TXL/AccessTXL/Access Prefabs/Access Keypad Controller", false, 201)]
        public static void AddAccessKeypadToScene()
        {
            Undo.SetCurrentGroupName("Add Access Keypad");
            int undoGroup = Undo.GetCurrentGroup();

            AccessControl acl = MenuUtil.GetObjectOrParent<AccessControl>();
            if (!acl)
            {
                Selection.activeObject = MenuUtil.AddPrefabToActiveOrScene("Packages/com.texelsaur.access/Runtime/Prefabs/Access Keypad Controller.prefab");
                Undo.CollapseUndoOperations(undoGroup);
                return;
            }

            AccessControlDynamicUserList whitelist = MenuUtil.GetObjectOrParent<AccessControlDynamicUserList>();
            if (!whitelist)
            {
                GameObject whitelistPrefab = MenuUtil.AddPrefabToObject("Packages/com.texelsaur.access/Runtime/Prefabs/User Lists/Dynamic Whitelist.prefab", acl.transform);
                whitelist = whitelistPrefab.GetComponent<AccessControlDynamicUserList>();
            }

            GameObject keypadPrefab = MenuUtil.AddPrefabToObject("Packages/com.texelsaur.access/Runtime/Prefabs/Access Keypad Controller.prefab", acl.transform);
            AccessKeypad keypad = keypadPrefab.GetComponent<AccessKeypad>();

            Undo.RecordObject(keypad, "Update Keypad");
            Undo.RecordObject(acl, "Update ACL");

            keypad.whitelistCodes = new string[] { "0000" };
            keypad.dynamicLists = new AccessControlDynamicUserList[] { whitelist };

            acl.allowWhitelist = true;
            if (!acl.whitelistSources.Contains(whitelist))
                acl.whitelistSources = acl.whitelistSources.Append(whitelist).ToArray();

            EditorUtility.SetDirty(acl);
            EditorUtility.SetDirty(keypad);

            Selection.activeObject = keypadPrefab;

            Undo.CollapseUndoOperations(undoGroup);
        }

        [MenuItem("GameObject/TXL/AccessTXL/Whitelist Sources/Static Whitelist", false, 210)]
        public static void AddStaticWhitelistToScene()
        {
            Undo.SetCurrentGroupName("Add Static Whitelist");
            int undoGroup = Undo.GetCurrentGroup();

            AccessControl acl = MenuUtil.GetObjectOrParent<AccessControl>();
            if (!acl)
            {
                if (!EditorUtility.DisplayDialog("Access Control Object Not Selected", "An Access Control object was not selected to add this whitelist to.", "Add whitelist anyway", "Cancel")) {
                    Undo.CollapseUndoOperations(undoGroup);
                    return;
                }
            }

            GameObject whitelistPrefab = MenuUtil.AddPrefabToActiveOrScene("Packages/com.texelsaur.access/Runtime/Prefabs/User Lists/Static Whitelist.prefab");
            AccessControlUserList whitelist = whitelistPrefab.GetComponent<AccessControlUserList>();

            Undo.RecordObject(acl, "Update ACL");

            acl.allowWhitelist = true;
            if (acl)
                acl.whitelistSources = acl.whitelistSources.Append(whitelist).ToArray();

            EditorUtility.SetDirty(acl);

            Selection.activeObject = whitelistPrefab;

            Undo.CollapseUndoOperations(undoGroup);
        }

        [MenuItem("GameObject/TXL/AccessTXL/Whitelist Sources/Remote Whitelist", false, 211)]
        public static void AddRemoteWhitelistToScene()
        {
            Undo.SetCurrentGroupName("Add Remote Whitelist");
            int undoGroup = Undo.GetCurrentGroup();

            AccessControl acl = MenuUtil.GetObjectOrParent<AccessControl>();
            if (!acl)
            {
                if (!EditorUtility.DisplayDialog("Access Control Object Not Selected", "An Access Control object was not selected to add this whitelist to.", "Add whitelist anyway", "Cancel"))
                {
                    Undo.CollapseUndoOperations(undoGroup);
                    return;
                }
            }

            GameObject whitelistPrefab = MenuUtil.AddPrefabToActiveOrScene("Packages/com.texelsaur.access/Runtime/Prefabs/User Lists/Remote Whitelist.prefab");
            AccessControlRemoteUserList whitelist = whitelistPrefab.GetComponent<AccessControlRemoteUserList>();

            Undo.RecordObject(acl, "Update ACL");

            acl.allowWhitelist = true;
            if (acl)
                acl.whitelistSources = acl.whitelistSources.Append(whitelist).ToArray();

            EditorUtility.SetDirty(acl);

            Selection.activeObject = whitelistPrefab;

            Undo.CollapseUndoOperations(undoGroup);
        }

        [MenuItem("GameObject/TXL/AccessTXL/Whitelist Sources/Dynamic Whitelist", false, 212)]
        public static void AddDynamicWhitelistToScene()
        {
            Undo.SetCurrentGroupName("Add Dynamic Whitelist");
            int undoGroup = Undo.GetCurrentGroup();

            AccessControl acl = MenuUtil.GetObjectOrParent<AccessControl>();
            if (!acl)
            {
                if (!EditorUtility.DisplayDialog("Access Control Object Not Selected", "An Access Control object was not selected to add this whitelist to.", "Add whitelist anyway", "Cancel"))
                {
                    Undo.CollapseUndoOperations(undoGroup);
                    return;
                }
            }

            GameObject whitelistPrefab = MenuUtil.AddPrefabToActiveOrScene("Packages/com.texelsaur.access/Runtime/Prefabs/User Lists/Dynamic Whitelist.prefab");
            AccessControlDynamicUserList whitelist = whitelistPrefab.GetComponent<AccessControlDynamicUserList>();

            Undo.RecordObject(acl, "Update ACL");

            acl.allowWhitelist = true;
            if (acl)
                acl.whitelistSources = acl.whitelistSources.Append(whitelist).ToArray();

            EditorUtility.SetDirty(acl);

            EditorUtility.DisplayDialog("Dynamic Whitelist", "The newly added dynamic whitelist won't do anything on its own.\n\nSelect the whitelist and try adding one of the Access Prefabs from the menu.", "OK");

            Selection.activeObject = whitelistPrefab;

            Undo.CollapseUndoOperations(undoGroup);
        }

        [MenuItem("GameObject/TXL/AccessTXL/Validation/Digest Validator", false, 220)]
        public static void AddDigestValidatorToScene()
        {
            Undo.SetCurrentGroupName("Add Digest Validator");
            int undoGroup = Undo.GetCurrentGroup();

            GameObject validatorPrefab = MenuUtil.AddPrefabToActiveOrScene("Packages/com.texelsaur.access/Runtime/Prefabs/Validation/Digest Validator.prefab");
            DataValidator validator = validatorPrefab.GetComponent<DataValidator>();

            AccessControlRemoteUserList remoteList = MenuUtil.GetObjectOrParent<AccessControlRemoteUserList>();
            if (remoteList)
            {
                Undo.RecordObject(remoteList, "Update Remote List");
                remoteList.dataValidator = validator;
                EditorUtility.SetDirty(remoteList);
            }

            EditorUtility.DisplayDialog("Digest Data Validator", "The newly added validator needs a key provider.\n\nYou can add a Serialized Key provider, but you should consider writing one specific to your needs.", "OK");

            Selection.activeObject = validatorPrefab;

            Undo.CollapseUndoOperations(undoGroup);
        }

        [MenuItem("GameObject/TXL/AccessTXL/Validation/Digest Validator - Serialized Key", false, 221)]
        public static void AddSerializedKeyDigestValidatorToScene()
        {
            Undo.SetCurrentGroupName("Add Digest Validator");
            int undoGroup = Undo.GetCurrentGroup();

            GameObject validatorPrefab = MenuUtil.AddPrefabToActiveOrScene("Packages/com.texelsaur.access/Runtime/Prefabs/Validation/Serialized Key Digest Validator.prefab");
            DataValidator validator = validatorPrefab.GetComponent<DataValidator>();

            AccessControlRemoteUserList remoteList = MenuUtil.GetObjectOrParent<AccessControlRemoteUserList>();
            if (remoteList)
            {
                Undo.RecordObject(remoteList, "Update Remote List");
                remoteList.dataValidator = validator;
                EditorUtility.SetDirty(remoteList);
            }

            Selection.activeObject = validatorPrefab;

            Undo.CollapseUndoOperations(undoGroup);
        }
    }
}
