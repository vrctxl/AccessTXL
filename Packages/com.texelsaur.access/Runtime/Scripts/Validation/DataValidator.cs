
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Texel
{
    public abstract class DataValidator : UdonSharpBehaviour
    {
        public virtual bool _PreValidate(string data)
        {
            return true;
        }

        public virtual string _Transform(string data)
        {
            return data;
        }

        public virtual bool _PostValidate(string data)
        {
            return true;
        }
    }
}
