using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    interface ISubscriptable<TKey, TValue> { TValue this[TKey key] { get; set; } }

    public class SerializableDictionary<TKey, TValue> : ISubscriptable<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys;
        [SerializeField]
        private List<TValue> values;
        private Dictionary<TKey, TValue> data = new Dictionary<TKey, TValue>();

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            Debug.Assert(keys.Count == values.Count, "SerializableDictionary should have same number of keys and values.");

            for (int i = 0; i < keys.Count; i++)
                data.Add(keys[i], values[i]);
        }

        public TValue this[TKey key] {
            get
            {
                return data[key];
            }

            set
            {
                data[key] = value;
            }
        }
    }
}