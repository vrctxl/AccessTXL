using System.Collections;
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Texel
{
    [CustomEditor(typeof(AccessControlRemoteUserList))]
    public class AccessControlRemoteListInspector : Editor
    {
        SerializedProperty accessControlProperty;
        SerializedProperty remoteStringUrlProperty;
        SerializedProperty remoteStringFormatProperty;
        SerializedProperty jsonObjectPathProperty;
        SerializedProperty jsonEntryPathProperty;
        SerializedProperty dataValiadtorProperty;
        SerializedProperty loadRemoteOnStartProperty;
        SerializedProperty startDelayProperty;
        SerializedProperty allowManualRefreshProperty;
        SerializedProperty allowPeriodicRefreshProperty;
        SerializedProperty refreshPeriodProperty;
        SerializedProperty debugLogProperty;
        SerializedProperty debugLoggingProperty;
        SerializedProperty userListProperty;

        static bool expandDebug = false;

        private void OnEnable()
        {
            accessControlProperty = serializedObject.FindProperty(nameof(AccessControlRemoteUserList.accessControl));
            remoteStringUrlProperty = serializedObject.FindProperty(nameof(AccessControlRemoteUserList.remoteStringUrl));
            remoteStringFormatProperty = serializedObject.FindProperty(nameof(AccessControlRemoteUserList.remoteStringFormat));
            jsonObjectPathProperty = serializedObject.FindProperty(nameof(AccessControlRemoteUserList.jsonObjectPath));
            jsonEntryPathProperty = serializedObject.FindProperty(nameof(AccessControlRemoteUserList.jsonEntryPath));
            dataValiadtorProperty = serializedObject.FindProperty(nameof(AccessControlRemoteUserList.dataValidator));
            loadRemoteOnStartProperty = serializedObject.FindProperty(nameof(AccessControlRemoteUserList.loadRemoteOnStart));
            startDelayProperty = serializedObject.FindProperty(nameof(AccessControlRemoteUserList.startDelay));
            allowManualRefreshProperty = serializedObject.FindProperty(nameof(AccessControlRemoteUserList.allowManualRefresh));
            allowPeriodicRefreshProperty = serializedObject.FindProperty(nameof(AccessControlRemoteUserList.allowPeriodicRefresh));
            refreshPeriodProperty = serializedObject.FindProperty(nameof(AccessControlRemoteUserList.refreshPeriod));
            debugLogProperty = serializedObject.FindProperty(nameof(AccessControlRemoteUserList.debugLog));
            debugLoggingProperty = serializedObject.FindProperty(nameof(AccessControlRemoteUserList.debugLogging));
            userListProperty = serializedObject.FindProperty(nameof(AccessControlRemoteUserList.userList));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
                return;

            GUIStyle boldFoldoutStyle = new GUIStyle(EditorStyles.foldout);
            boldFoldoutStyle.fontStyle = FontStyle.Bold;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Remote Loading", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(remoteStringUrlProperty, new GUIContent("Remote String URL"));
            EditorGUILayout.PropertyField(remoteStringFormatProperty, new GUIContent("Remote String Format"));

            if (remoteStringFormatProperty.intValue == (int)ACLListFormat.JSONArray)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(jsonObjectPathProperty, new GUIContent("JSON Object Path", "The path to an array within the JSON response.  Each nested obejct is separated by a / character.  Leave empty if the JSON response is the desired array."));
                EditorGUILayout.PropertyField(jsonEntryPathProperty, new GUIContent("JSON Entry Path", "If the target array contains objects instead of strings, this is the path to the name string within each array entry.  Leave empty if the target array is an array of strings."));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(dataValiadtorProperty, new GUIContent("Data Validator", "Optional validation or transformation of downloaded data before loading names into the user list."));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Refresh Options", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(loadRemoteOnStartProperty, new GUIContent("Refresh on Start", "Whether or not remote data will be requested when the player joins."));
            if (loadRemoteOnStartProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(startDelayProperty, new GUIContent("Start Delay", "Time in seconds to delay calling the initial refresh on start"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(allowManualRefreshProperty, new GUIContent("Enable Sync Refresh", "Allows a local client to instruct all players in world to reload remote data via network sync"));
            if (allowManualRefreshProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(accessControlProperty, new GUIContent("Access Control", "Optional access control object that gates which clients can request a sync reload"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(allowPeriodicRefreshProperty, new GUIContent("Enable Periodic Refresh", "Automatically reloads remote data at a fixed time interval"));
            if (allowPeriodicRefreshProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(refreshPeriodProperty, new GUIContent("Refresh Period", "Time interval in seconds between remote data reloads"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Default Whitelist", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(userListProperty, new GUIContent("User Whitelist"));

            EditorGUILayout.Space();
            expandDebug = EditorGUILayout.Foldout(expandDebug, "Debug Options", true, boldFoldoutStyle);
            if (expandDebug)
            {
                EditorGUILayout.PropertyField(debugLogProperty, new GUIContent("Debug Log", "Log debug statements to a world object"));
                EditorGUILayout.PropertyField(debugLoggingProperty, new GUIContent("VRC Logging", "Write out debug statements to VRChat log"));
            }

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
}
