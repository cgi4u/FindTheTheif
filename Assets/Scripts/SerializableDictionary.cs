using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    interface ISubscriptable<TKey, TValue> { TValue this[TKey key] { get; set; } }

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : MonoBehaviour, ISubscriptable<TKey, TValue> 
    {
        [SerializeField]
        private List<TKey> keys;
        [SerializeField]
        private List<TValue> values;

        private void Awake()
        {
            Debug.Assert(keys.Count == values.Count, "SerializableDictionary should have same number of keys and values.");

            for (int i = 0; i < keys.Count; i++)
                dictionaryData[keys[i]] = values[i];
        }

        private Dictionary<TKey, TValue> dictionaryData;

        public TValue this[TKey key] {
            get
            {
                return dictionaryData[key];
            }

            set
            {
                dictionaryData[key] = value;
            }
        }
    }
}