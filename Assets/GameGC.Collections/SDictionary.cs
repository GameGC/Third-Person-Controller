using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
using Random = System.Random;
#endif

namespace GameGC.Collections
{
    [Serializable]
    public class SDictionary<TKey,TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver 
    {
        [SerializeField] private SKeyValuePair<TKey, TValue>[] _keyValuePairs;

        public SDictionary()
        {
        }

        public SDictionary([NotNull] IDictionary<TKey, TValue> dictionary) : base(dictionary)
        {
            _keyValuePairs = new SKeyValuePair<TKey, TValue>[dictionary.Count];
            int i = 0;
            foreach (var keyValuePair in dictionary)
            {
                _keyValuePairs[i] = keyValuePair;
                i++;
            }
        }


        /// <summary>
        /// OnBeforeSerialize implementation.
        /// </summary>
        public void OnBeforeSerialize()
        {
            if(_keyValuePairs == null || _keyValuePairs.Length != Count)
                _keyValuePairs = new SKeyValuePair<TKey, TValue>[Count];

            int i = 0;
            foreach (var kvp in this)
            {
                _keyValuePairs[i] = new SKeyValuePair<TKey, TValue>(kvp.Key,kvp.Value);
                i++;
            }
        }

        /// <summary>
        /// OnAfterDeserialize implementation.
        /// </summary>
        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            ValidateUnique();
#endif
            
            Clear();

            for (int i = 0; i < _keyValuePairs.Length; i++)
                Add(_keyValuePairs[i].Key, _keyValuePairs[i].Value);

            _keyValuePairs = null;
        }

#if UNITY_EDITOR
        private void ValidateUnique()
        {
            if(_keyValuePairs.Length<2) return;
            var allkeys = _keyValuePairs.Select(k => k.Key).ToList();

            for (var i = 0; i < allkeys.Count; i++)
            {
                int first = allkeys.FindIndex( k => k.Equals(allkeys[i]));
                int last  = allkeys.FindLastIndex(k => k.Equals(allkeys[i]));
                while (first != last)
                {
                    var type = typeof(TKey);
                    var newKey = type == typeof(string)? (TKey)(object)"" :Activator.CreateInstance<TKey>();

                    var randomGen = new Random();
                        
                    int attempts = 0;
                    while (allkeys.Contains(newKey) && attempts < 11)
                    {
                        switch (newKey)
                        {
                            case string str:
                                const string chars = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&";
                                str += "" +chars[randomGen.Next(0, chars.Length)];
                                newKey = (TKey) Convert.ChangeType(str, typeof(TKey));
                                break;
                            case int int_:
                                int_ = randomGen.Next(int.MinValue, int.MaxValue);
                                newKey = (TKey) Convert.ChangeType(int_, typeof(TKey));
                                break;
                            case float float_:
                                float_ = randomGen.Next(int.MinValue, int.MaxValue) - (float)randomGen.NextDouble();
                                newKey = (TKey) Convert.ChangeType(float_, typeof(TKey));
                                break;
                            case short short_:
                                short_ = (short) randomGen.Next(short.MinValue, short.MaxValue);
                                newKey = (TKey) Convert.ChangeType(short_, typeof(TKey));
                                break;
                            case byte byte_:
                                byte_ = (byte) randomGen.Next(byte.MinValue, byte.MaxValue);
                                newKey = (TKey) Convert.ChangeType(byte_, typeof(TKey));
                                break;
                            default:
                            {
                                if (type.IsEnum)
                                {
                                    var names = type.GetEnumNames();
                                    newKey = (TKey) Enum.Parse(type,names[Mathf.Clamp(attempts, 0, names.Length - 1)]);
                                }
                                break;
                            }
                        }

                        attempts++;
                    }

                    if(attempts == 11) throw new Exception("Out of attempts to generate new value");

                    int index = Mathf.Max(first, last);
                        
                    _keyValuePairs[index].Key = newKey;
                    allkeys[index] = newKey;
                    
                    
                    first = allkeys.FindIndex( k => k.Equals(allkeys[i]));
                    last = allkeys.FindLastIndex(k => k.Equals(allkeys[i]));
                }
            }
        }
#endif
    }
}
