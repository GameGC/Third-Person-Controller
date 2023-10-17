using System;
using System.Collections.Generic;

namespace GameGC.Collections
{
    [Serializable]
    public struct SKeyValuePair<TKey, TValue> 
    {
        public TKey Key;
        public TValue Value;
        
        public SKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
        
        public static implicit operator SKeyValuePair<TKey, TValue> (KeyValuePair<TKey, TValue> pair)
        {
            return new SKeyValuePair<TKey, TValue>(pair.Key, pair.Value);
        }

        public static explicit operator KeyValuePair<TKey, TValue>( SKeyValuePair<TKey, TValue> pair)
        {
            return new KeyValuePair<TKey, TValue>(pair.Key, pair.Value);
        }
    }
}