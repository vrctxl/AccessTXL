
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Texel
{
    public abstract class DataValidatorKey : UdonSharpBehaviour
    {
        public abstract string _GetKey();
    }
}
