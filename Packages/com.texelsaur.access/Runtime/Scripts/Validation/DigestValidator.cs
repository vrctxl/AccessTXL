
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Texel
{
    enum HashStrategy
    {
        PrefixKey,
        PostfixKey,
        PrefixPostfixKey,
        HMAC,
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DigestValidator : DataValidator
    {
        [SerializeField] internal DataValidatorKey keyProvider;
        [SerializeField] internal Texel.UdonHashLib udonHashLib;
        [SerializeField] internal HashFunction hashFunction;
        [SerializeField] internal HashStrategy hashStrategy;

        private string cachedData = "";

        public override bool _PreValidate(string data)
        {
            cachedData = "";
            if (!keyProvider || !udonHashLib || data == null)
                return false;

            int dataLength = data.Length;
            int length = _KeyLength(hashFunction);
            if (length == 0)
                return false;

            string digest = data.Substring(0, length);
            while (length < dataLength)
            {
                char c = data[length];
                if (c == '\n' || c == '\r' || c == ' ')
                    length += 1;
                else
                    break;
            }

            cachedData = data.Substring(length);
            string refDigest = _HashData(cachedData);

            return digest == refDigest;
        }

        public override string _Transform(string data)
        {
            return cachedData;
        }

        int _KeyLength(HashFunction func)
        {
            if (func == HashFunction.MD5)
                return 32;
            if (func == HashFunction.SHA1)
                return 40;
            if (func == HashFunction.SHA224)
                return 56;
            if (func == HashFunction.SHA256)
                return 64;
            if (func == HashFunction.SHA384)
                return 96;
            if (func == HashFunction.SHA512)
                return 128;
            return 0;
        }

        string _HashData(string data)
        {
            string key = keyProvider._GetKey();
            if (key == null)
                key = "";

            if (hashStrategy == HashStrategy.HMAC)
                return _HashDataHMAC(data, key);

            string combined = data;
            if (hashStrategy == HashStrategy.PrefixKey)
                combined = key + data;
            else if (hashStrategy == HashStrategy.PostfixKey)
                combined = data + key;
            else if (hashStrategy == HashStrategy.PrefixPostfixKey)
                combined = key + data + key;

            return _HashDataRaw(combined);
        }

        string _HashDataRaw(string data)
        {
            if (hashFunction == HashFunction.MD5)
                return udonHashLib.MD5_UTF8(data);
            if (hashFunction == HashFunction.SHA1)
                return udonHashLib.SHA1_UTF8(data);
            if (hashFunction == HashFunction.SHA224)
                return udonHashLib.SHA224_UTF8(data);
            if (hashFunction == HashFunction.SHA256)
                return udonHashLib.SHA256_UTF8(data);
            if (hashFunction == HashFunction.SHA384)
                return udonHashLib.SHA384_UTF8(data);
            if (hashFunction == HashFunction.SHA512)
                return udonHashLib.SHA512_UTF8(data);

            return "";
        }

        string _HashDataHMAC(string data, string key)
        {
            if (hashFunction == HashFunction.SHA1)
                return udonHashLib.HMAC_SHA1_UTF8(data, key);
            if (hashFunction == HashFunction.SHA224)
                return udonHashLib.HMAC_SHA224_UTF8(data, key);
            if (hashFunction == HashFunction.SHA256)
                return udonHashLib.HMAC_SHA256_UTF8(data, key);
            if (hashFunction == HashFunction.SHA384)
                return udonHashLib.HMAC_SHA384_UTF8(data, key);
            if (hashFunction == HashFunction.SHA512)
                return udonHashLib.HMAC_SHA512_UTF8(data, key);

            return "";
        }
    }
}
