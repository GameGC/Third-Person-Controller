using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameGC.Collections
{
    [Serializable]
    public class SKeyValueReadonlyArray<TKey,TValue> : IReadOnlyDictionary<TKey,TValue>
    {
        [SerializeField] private TKey[] _keys = Array.Empty<TKey>();
        [SerializeField] private TValue[] _values = Array.Empty<TValue>();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < _keys.Length; i++)
                yield return new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _keys.Length;
        public bool ContainsKey(TKey key) => Array.IndexOf(_keys, key) > -1;


        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = Array.IndexOf(_keys, key);
            value = index > -1 ?_values[index] : default;
            return index > -1;
        }

        public TValue this[TKey key]
        {
            get => _values[Array.IndexOf(_keys,key)];
            set => _values[Array.IndexOf(_keys, key)] = value;
        }

        public TValue GetValueAt(in int index) => _values[index];
        public void SetValueAt(in int index,in TValue value) => _values[index] = value;
        
        
        public IEnumerable<TKey> Keys => _keys;
        public IEnumerable<TValue> Values => _values;
        
        public TKey[] KeysArray => _keys;
        public TValue[] ValuesArray => _values;
    }
}