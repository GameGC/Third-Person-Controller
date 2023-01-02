using System;

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
    }
}