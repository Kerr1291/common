using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace nv
{
    [System.Serializable]
    public class SerializableSystemType
    {
        public SerializableSystemType() { }
        public SerializableSystemType(System.Type type)
        {
            Data = type;
        }

        public SerializableSystemType(string assemblyName, string fullName)
        {
            typeAssemblyName = assemblyName;
            typeName = fullName;
        }

        public System.Type Data
        {
            get
            {
                //if the data is lost (which it will be after every time unity rebuilds), rebuild it
                if(data == null)
                {
                    if(string.IsNullOrEmpty(typeAssemblyName))
                        return null;

                    BuildType();
                }

                return data;
            }
            set
            {
                data = value;
                if(data != null)
                    SaveTypeData();
            }
        }

        private void SaveTypeData()
        {
            typeName = data.FullName;
            typeAssemblyName = Assembly.GetAssembly(data).FullName;
        }

        void BuildType()
        {
            if(string.IsNullOrEmpty(typeAssemblyName))
                return;

            Assembly typeAssembly = Assembly.Load(typeAssemblyName);
            System.Type targetType = typeAssembly.GetType(typeName);

            if(targetType == null)
            {
                UnityEngine.Debug.LogWarning("Did not find type " + typeName + " in assembly " + typeAssemblyName);
                return;
            }

            data = targetType;
        }

        System.Type data;

        [SerializeField]
        string typeAssemblyName;

        [SerializeField]
        string typeName;
    }
}