using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using System.Collections.Generic;
using VRC;

namespace DrakenStark
{
    [InitializeOnLoad]
    public class TXLAudioLinkLockBuildDetect : Object, IProcessSceneWithReport
    {
        //Regardless of multiple copies in the scene, On Build and Mode change, this should in theory only be run once.
        //Go through everything that needs to be done in either in this class without worrying about doubled effort.

        //Detect Build before entering Play Mode
        static TXLAudioLinkLockBuildDetect()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            //Changes will persist after exiting Play mode.
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                //Debug.LogWarning("Play Build Process Detected!");
                _finalizeForBuild();
            }

            //Changes will revert after exiting Play mode.
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
            }
        }

        //Detect Build when not entering Play Mode
        public int callbackOrder => 0; //Lets Unity know that, during a build, this script will care where it falls in the order of scripts to run.
        //public int callbackOrder { get; } //Lets Unity know that, during a build, this script will not care where it falls in the order of scripts to run.
        public void OnProcessScene(UnityEngine.SceneManagement.Scene scene, BuildReport report)
        {
            if (!Application.isPlaying)
            {
                //Debug.LogWarning("Non-Play Build Process Detected!");
                _finalizeForBuild();
            }
        }

        //Code to run in either Build case
        public static void _finalizeForBuild()
        {
            //Debug.LogWarning("Build Prep Started!");

            TXLAudioLinkLock[] allTXLAudioLinkLocks = FindObjectsOfType<TXLAudioLinkLock>();
            AudioLink.AudioLinkController[] aLControllerList = null;
            List<VRC.SDK3.Components.VRCPickup> allPickups = new List<VRC.SDK3.Components.VRCPickup>();
            List<UnityEngine.UI.Button> allButtons = new List<UnityEngine.UI.Button>();
            List<UnityEngine.UI.Slider> allSliders = new List<UnityEngine.UI.Slider>();
            List<UnityEngine.UI.Toggle> allToggles = new List<UnityEngine.UI.Toggle>();

            //Go through each of the TXLAudioLinkLock scripts in the scene.
            for (int tXLAudioLinkLock = 0; tXLAudioLinkLock < allTXLAudioLinkLocks.Length; tXLAudioLinkLock++)
            {
                try
                {
                    //For each Audio Link Controller, find all the appropriate objects and populate each of the arrays in the Editor so Players will not have to.
                    aLControllerList = (AudioLink.AudioLinkController[])allTXLAudioLinkLocks[tXLAudioLinkLock].GetProgramVariable("_audioLinkControllers");
                    Debug.LogWarning("There are " + aLControllerList.Length + " Audio Link Controllers detected on " + allTXLAudioLinkLocks[tXLAudioLinkLock].name, allTXLAudioLinkLocks[tXLAudioLinkLock].gameObject);
                    if (VRC.SDKBase.Utilities.IsValid(aLControllerList) && aLControllerList.Length > 0)
                    {
                        //Set all of the list variables to an empty default.
                        allPickups = new List<VRC.SDK3.Components.VRCPickup>();
                        allButtons = new List<UnityEngine.UI.Button>();
                        allSliders = new List<UnityEngine.UI.Slider>();
                        allToggles = new List<UnityEngine.UI.Toggle>();

                        //Use the list variables to collect all the components.
                        for (int aLController = 0; aLController < aLControllerList.Length; aLController++)
                        {
                            allPickups.AddRange(aLControllerList[aLController].gameObject.GetComponentsInChildren<VRC.SDK3.Components.VRCPickup>(true));
                            allButtons.AddRange(aLControllerList[aLController].gameObject.GetComponentsInChildren<UnityEngine.UI.Button>(true));
                            allSliders.AddRange(aLControllerList[aLController].gameObject.GetComponentsInChildren<UnityEngine.UI.Slider>(true));
                            allToggles.AddRange(aLControllerList[aLController].gameObject.GetComponentsInChildren<UnityEngine.UI.Toggle>(true));
                        }

                        //Lock down all the found components and Mark them to have the change saved.
                        for (int pickup = 0; pickup < allPickups.Count; pickup++)
                        {
                            allPickups[pickup].pickupable = false;
                            allPickups[pickup].MarkDirty();
                        }
                        for (int uIElement = 0; uIElement < allButtons.Count; uIElement++)
                        {
                            allButtons[uIElement].interactable = false;
                            allButtons[uIElement].MarkDirty();
                        }
                        for (int uIElement = 0; uIElement < allSliders.Count; uIElement++)
                        {
                            allSliders[uIElement].interactable = false;
                            allSliders[uIElement].MarkDirty();
                        }
                        for (int uIElement = 0; uIElement < allToggles.Count; uIElement++)
                        {
                            allToggles[uIElement].interactable = false;
                            allToggles[uIElement].MarkDirty();
                        }

                        //Update the script variables with the lists of collected components.
                        allTXLAudioLinkLocks[tXLAudioLinkLock].SetProgramVariable("_vRCPickups", allPickups.ToArray());
                        allTXLAudioLinkLocks[tXLAudioLinkLock].SetProgramVariable("_buttons", allButtons.ToArray());
                        allTXLAudioLinkLocks[tXLAudioLinkLock].SetProgramVariable("_sliders", allSliders.ToArray());
                        allTXLAudioLinkLocks[tXLAudioLinkLock].SetProgramVariable("_toggles", allToggles.ToArray());

                        //Checks if the array is a valid variable, then checks if it has a length larger than zero. If not, the variable is set to false thanks to the check returning a bool value.
                        //If AudioLink ever updates in a way that it doesn't have one of these kinds of UI elements, the rest can still be found and allow the runtime script to still work.
                        allTXLAudioLinkLocks[tXLAudioLinkLock].SetProgramVariable("_hasPickups", VRC.SDKBase.Utilities.IsValid((VRC.SDK3.Components.VRCPickup[])allTXLAudioLinkLocks[tXLAudioLinkLock].GetProgramVariable("_vRCPickups")) && ((VRC.SDK3.Components.VRCPickup[])allTXLAudioLinkLocks[tXLAudioLinkLock].GetProgramVariable("_vRCPickups")).Length > 0);
                        allTXLAudioLinkLocks[tXLAudioLinkLock].SetProgramVariable("_hasButtons", VRC.SDKBase.Utilities.IsValid((UnityEngine.UI.Button[])allTXLAudioLinkLocks[tXLAudioLinkLock].GetProgramVariable("_buttons")) && ((UnityEngine.UI.Button[])allTXLAudioLinkLocks[tXLAudioLinkLock].GetProgramVariable("_buttons")).Length > 0);
                        allTXLAudioLinkLocks[tXLAudioLinkLock].SetProgramVariable("_hasSliders", VRC.SDKBase.Utilities.IsValid((UnityEngine.UI.Slider[])allTXLAudioLinkLocks[tXLAudioLinkLock].GetProgramVariable("_sliders")) && ((UnityEngine.UI.Slider[])allTXLAudioLinkLocks[tXLAudioLinkLock].GetProgramVariable("_sliders")).Length > 0);
                        allTXLAudioLinkLocks[tXLAudioLinkLock].SetProgramVariable("_hasToggles", VRC.SDKBase.Utilities.IsValid((UnityEngine.UI.Toggle[])allTXLAudioLinkLocks[tXLAudioLinkLock].GetProgramVariable("_toggles")) && ((UnityEngine.UI.Toggle[])allTXLAudioLinkLocks[tXLAudioLinkLock].GetProgramVariable("_toggles")).Length > 0);

                        //Mark the script so that changes are saved.
                        allTXLAudioLinkLocks[tXLAudioLinkLock].MarkDirty();
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex, allTXLAudioLinkLocks[tXLAudioLinkLock].gameObject);
                }
            }

            //Update the AssetDatabase so the new screenshot file(s) can be recognized.
            //Resources.UnloadUnusedAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //Debug.LogWarning("Build Prep Finished!");
        }
    }
}