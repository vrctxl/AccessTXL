﻿
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Texel
{
    public enum KeypadToggleAction
    {
        Enable,
        Disable,
        Toggle,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AccessKeypad : UdonSharpBehaviour
    {
        [SerializeField] private AccessKeypadControl[] keypads;

        [SerializeField] private string[] whitelistCodes;
        [SerializeField] private AccessControlDynamicUserList[] dynamicLists;

        [SerializeField] private string[] functionCodes;
        [SerializeField] private UdonBehaviour[] functionTargets;
        [SerializeField] private string[] functionNames;
        [SerializeField] private string[] functionArgs;

        [SerializeField] private string[] toggleCodes;
        [SerializeField] private GameObject[] toggleObjects;
        [SerializeField] private KeypadToggleAction[] toggleActions;

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
    }
}
