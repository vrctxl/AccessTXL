
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/**
 * This is a sample implementation of a data validator key provider.  This provider simply returns a string
 * that's a serialized field on the object.  While there are no foolproof ways to embed a key securely within
 * your scene, this provider may be particularly vulnerable to any players with client tools that can
 * examine unity objects.
 * 
 * You could consider writing a different provider class that hardcodes a return string or performs other
 * obfuscation steps to make a key more difficult to discover.
 */

namespace Texel
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SerializedKey : DataValidatorKey
    {
        [SerializeField] internal string key;

        public override string _GetKey()
        {
            return key;
        }
    }
}
