
using System;
using System.Runtime.CompilerServices;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[assembly: InternalsVisibleTo("com.texelsaur.access.Editor")]

namespace Texel
{
    public enum KeypadCodeType
    {
        Whitelist,
        Function,
        Toggle,
    }

    public enum KeypadToggleAction
    {
        Enable,
        Disable,
        Toggle,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AccessKeypad : UdonSharpBehaviour
    {
        [SerializeField] internal AccessKeypadControl[] keypads;

        [SerializeField] internal string[] whitelistCodes;
        [SerializeField] internal AccessControlDynamicUserList[] dynamicLists;

        [SerializeField] internal string[] functionCodes;
        [SerializeField] internal UdonBehaviour[] functionTargets;
        [SerializeField] internal string[] functionNames;
        [SerializeField] internal string[] functionArgs;

        [SerializeField] internal string[] toggleCodes;
        [SerializeField] internal GameObject[] toggleObjects;
        [SerializeField] internal KeypadToggleAction[] toggleActions;

        [NonSerialized]
        public string keypadArg = "";

        void Start()
        {
            foreach (AccessKeypadControl keypad in keypads)
            {
                if (keypad)
                    keypad._Register(AccessKeypadControl.EVENT_SUBMIT, this, nameof(_OnSubmit), nameof(keypadArg));
            }
        }

        public void _OnSubmit()
        {
            _SubmitCode(keypadArg);
        }

        public void _SubmitCode(string code)
        {
            for (int i = 0; i < whitelistCodes.Length; i++)
            {
                if (code != whitelistCodes[i])
                    continue;
                if (!dynamicLists[i])
                    continue;

                dynamicLists[i]._AddPlayer(Networking.LocalPlayer);
            }

            for (int i = 0; i < functionCodes.Length; i++)
            {
                if (code != functionCodes[i])
                    continue;
                if (!functionTargets[i] || functionNames[i] == null || functionNames[i] == string.Empty)
                    continue;

                if (functionArgs[i] != null && functionArgs[i].Length > 0)
                    functionTargets[i].SetProgramVariable(functionArgs[i], Networking.LocalPlayer);
                functionTargets[i].SendCustomEvent(functionNames[i]);
            }

            for (int i = 0; i < toggleCodes.Length; i++)
            {
                if (code != toggleCodes[i])
                    continue;
                if (!toggleObjects[i])
                    continue;

                if (toggleActions[i] == KeypadToggleAction.Enable)
                    toggleObjects[i].SetActive(true);
                else if (toggleActions[i] == KeypadToggleAction.Disable)
                    toggleObjects[i].SetActive(false);
                else
                    toggleObjects[i].SetActive(!toggleObjects[i].activeSelf);
            }
        }

        public string _GetCode(KeypadCodeType type, int index)
        {
            string[] list = _GetCodeList(type);
            if (list == null || index < 0 || index >= list.Length)
                return "";

            return list[index];
        }

        public void _SetCode(KeypadCodeType type, int index, string code)
        {
            string[] list = _GetCodeList(type);
            if (list == null || index < 0 || index >= list.Length)
                return;

            list[index] = code;
        }

        string[] _GetCodeList(KeypadCodeType type)
        {
            if (type == KeypadCodeType.Whitelist)
                return whitelistCodes;
            if (type == KeypadCodeType.Function)
                return functionCodes;
            if (type == KeypadCodeType.Toggle)
                return toggleCodes;

            return null;
        }
    }
}
