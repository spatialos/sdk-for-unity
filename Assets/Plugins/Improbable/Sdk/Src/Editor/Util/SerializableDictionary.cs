// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Improbable.Unity.EditorTools.Util
{
    /// <summary>
    ///     Unity's serialization system can't handle Dictionary by default.
    ///     Users must inherit their from this type to serialize it.
    /// </summary>
    [Serializable]
    internal class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        // These fields must not be readonly, or serialization will not work.

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        [SerializeField] private List<TKey> keys = new List<TKey>();

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        [SerializeField] private List<TValue> values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (var pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();

            if (keys.Count != values.Count)
            {
                throw new Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are [Serializable].", keys.Count, values.Count));
            }

            for (int i = 0; i < keys.Count; i++)
            {
                Add(keys[i], values[i]);
            }
        }
    }
}
