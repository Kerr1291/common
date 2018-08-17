#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace nv.editor
{
    public class EditorData : ScriptableSingleton<EditorData>, ISerializationCallbackReceiver
    {
        Dictionary<string, StoredData> storedDataLookup;

        [SerializeField]
        List<string> storedDataKeys;
        [SerializeField]
        List<StoredData> storedDataValues;

        [ContextMenu("Clear Stored Data")]
        public void ClearSavedData()
        {
            storedDataLookup = new Dictionary<string, StoredData>();
            storedDataKeys = new List<string>();
            storedDataValues = new List<StoredData>();
        }

        /// <summary>
        /// Using up to two lookup keys, get the data stored there. Useful for saving things like previous working directories in editor scripts.
        /// </summary>
        public bool HasData<T>(string firstLookupKey, string secondLookupKey = null)
        {
            if(secondLookupKey == null)
                secondLookupKey = firstLookupKey;

            string lookupKey = typeof(T).Name;

            if(storedDataLookup.ContainsKey(lookupKey)
                && storedDataLookup[lookupKey].data.ContainsKey(firstLookupKey)
                && storedDataLookup[lookupKey].data[firstLookupKey].ContainsKey(secondLookupKey))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Using up to two lookup keys, get the data stored there. Useful for saving things like previous working directories in editor scripts.
        /// </summary>
        public bool TryGetData<T>(ref T data, string firstLookupKey, string secondLookupKey = null)
        {
            if(secondLookupKey == null)
                secondLookupKey = firstLookupKey;

            string lookupKey = typeof(T).Name;

            if(storedDataLookup.ContainsKey(lookupKey)
                && storedDataLookup[lookupKey].data.ContainsKey(firstLookupKey)
                && storedDataLookup[lookupKey].data[firstLookupKey].ContainsKey(secondLookupKey))
            {
                data = (T)storedDataLookup[lookupKey].data[firstLookupKey][secondLookupKey];
                return true;
            }
            return false;
        }

        /// <summary>
        /// Using up to two lookup keys, get the data stored there. Useful for saving things like previous working directories in editor scripts.
        /// </summary>
        public T GetData<T>(string firstLookupKey, string secondLookupKey = null)
        {
            if(secondLookupKey == null)
                secondLookupKey = firstLookupKey;

            string lookupKey = typeof(T).Name;

            if(storedDataLookup.ContainsKey(lookupKey)
                && storedDataLookup[lookupKey].data.ContainsKey(firstLookupKey)
                && storedDataLookup[lookupKey].data[firstLookupKey].ContainsKey(secondLookupKey))
            {
                return (T)storedDataLookup[lookupKey].data[firstLookupKey][secondLookupKey];
            }
            return default(T);
        }

        /// <summary>
        /// Using up to two lookup keys, save some data. Useful for saving things like previous working directories in editor scripts.
        /// </summary>
        public void SetData<T>(T data, string firstLookupKey, string secondLookupKey = null)
        {
            if(secondLookupKey == null)
                secondLookupKey = firstLookupKey;

            string lookupKey = typeof(T).Name;

            if(!storedDataLookup.ContainsKey(lookupKey))
                storedDataLookup.Add(lookupKey, new StoredData());

            if(!storedDataLookup[lookupKey].data.ContainsKey(firstLookupKey))
            {
                storedDataLookup[lookupKey].data.Add(firstLookupKey, new Dictionary<string, object>());
            }

            if(!storedDataLookup[lookupKey].data[firstLookupKey].ContainsKey(secondLookupKey))
            {
                storedDataLookup[lookupKey].data[firstLookupKey].Add(secondLookupKey, data);
            }
            else
            {
                storedDataLookup[lookupKey].data[firstLookupKey][secondLookupKey] = data;
            }

            UpdateStoredData();
        }

        public void OnBeforeSerialize()
        {
            UpdateStoredData();
        }

        public void OnAfterDeserialize()
        {
            storedDataLookup = storedDataKeys.Zip(storedDataValues, (k, v) => new { k, v }).ToDictionary(x => x.k, x=>x.v);
        }

        void UpdateStoredData()
        {
            storedDataKeys = storedDataLookup.Keys.ToList();
            storedDataValues = storedDataLookup.Values.ToList();
        }

        [Serializable]
        class StoredData : ISerializationCallbackReceiver
        {
            public Dictionary<string, Dictionary<string, object>> data = new Dictionary<string, Dictionary<string, object>>();

            [SerializeField]
            List<string> keys;
            [SerializeField]
            List<InternalData> values;

            public void OnAfterDeserialize()
            {
                Func<InternalData, Dictionary<string, object>> valueToDict = (v) => { return v.keys.Zip(v.values, (n, u) => new KeyValuePair<string, object>(n, FromByteArray(u.b))).ToDictionary(y => y.Key, y => y.Value); };

                data = keys.Zip(values, (k, v) => { return new KeyValuePair<string, Dictionary<string, object>>(k, valueToDict(v)); })
                           .ToDictionary(x => x.Key, x => x.Value);    
            }

            public void OnBeforeSerialize()
            {
                UpdateStoredData(); 
            }

            public void UpdateStoredData()
            {
                keys = data.Keys.ToList();
                values = new List<InternalData>();
                foreach(var pair in data.Values)
                {
                    InternalData v = new InternalData();
                    v.keys = pair.Keys.ToList();
                    v.values = pair.Values.Select(x=> new InternalData.ByteArray(ToByteArray(x))).ToList();
                    values.Add(v); 
                }
            }

            public byte[] ToByteArray(object obj)
            {
                if(obj == null)
                    return null;
                BinaryFormatter bf = new BinaryFormatter();
                using(MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, obj);
                    return ms.ToArray();
                }
            }

            public object FromByteArray(byte[] data)
            {
                if(data == null)
                    return null;
                BinaryFormatter bf = new BinaryFormatter();
                using(MemoryStream ms = new MemoryStream(data))
                {
                    object obj = bf.Deserialize(ms);
                    return obj;
                }
            }

            [Serializable]
            public class InternalData
            {
                public List<string> keys;

                [HideInInspector]
                public List<ByteArray> values;

                //needed to allow serializing of the values list
                [Serializable]
                public class ByteArray
                {
                    public ByteArray(byte[] b = null)
                    {
                        this.b = b;
                    }
                    public byte[] b;
                }
            }
        }
    }
}
#endif