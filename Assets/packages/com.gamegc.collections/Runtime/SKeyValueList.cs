using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameGC.Collections
{
    [Serializable]
    public class SKeyValueList<TKey,TValue> : IDictionary<TKey,TValue>
    {
        [SerializeField] private List<TKey> _keys = new List<TKey>();
        [SerializeField] private List<TValue> _values =  new List<TValue>();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < _keys.Count; i++)
                yield return new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key,item.Value);

        public void Clear()
        {
            _keys.Clear();
            _values.Clear();
            _keys = null;
            _values = null;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => ContainsKey(item.Key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public int Count => _keys.Count;
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly { get; }

        public void Add(TKey key, TValue value)
        {
            _keys.Add(key);
            _values.Add(value);
        }

        public bool ContainsKey(TKey key) =>_keys.IndexOf(key) > -1;
        public bool Remove(TKey key)
        {
            int index = _keys.IndexOf(key);
            if (index > -1)
            {
                _keys.RemoveAt(index);
                _values.RemoveAt(index);
                return true;
            }
            return false;
        }


        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = _keys.IndexOf(key);
            value = index > -1 ?_values[index] : default;
            return index > -1;
        }

        public TValue this[TKey key]
        {
            get => _values[_keys.IndexOf(key)];
            set => _values[_keys.IndexOf(key)] = value;
        }


        ICollection<TKey> IDictionary<TKey,TValue>.Keys => _keys;
        ICollection<TValue> IDictionary<TKey,TValue>.Values => _values;

        public TValue GetValueAt(in int index) => _values[index];
        public void SetValueAt(in int index,in TValue value) => _values[index] = value;

        public List<TKey> KeysArray => _keys;
        public List<TValue> ValuesArray => _values;
    }
}