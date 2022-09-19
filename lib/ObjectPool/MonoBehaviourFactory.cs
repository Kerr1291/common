using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nv
{
    public class MonoBehaviourFactory : ScriptableObject, IMonoBehaviourFactory
    {
#if UNITY_EDITOR
        [ContextMenu("Add folder of prefabs")] 
        public virtual void SelectPrefabs()
        {
            string startingFolder = nv.editor.EditorData.Instance.GetData<string>( typeof( MonoBehaviourFactory ).Name + name, "SelectPrefabs" + "startingFolder" );
            string path = EditorUtility.SaveFolderPanel("Select folder to import", startingFolder, name);
            if( string.IsNullOrEmpty( path ) )
                return;
            nv.editor.EditorData.Instance.SetData<string>( startingFolder, typeof( MonoBehaviourFactory ).Name + name, "SelectPrefabs" + "startingFolder" );

            string[] fileEntries = Directory.GetFiles( path, "*.prefab", SearchOption.TopDirectoryOnly );

            List<PoolableMonoBehaviour> prefabs = new List<PoolableMonoBehaviour>();
            foreach( string fileName in fileEntries )
            {
                string localPath = "Assets" + fileName.Remove( 0, Application.dataPath.Length ); 
                PoolableMonoBehaviour asset = AssetDatabase.LoadAssetAtPath<PoolableMonoBehaviour>( localPath );                
                if( asset != null )
                {
                    objectMappingKeys.Add( asset.name );
                    objectMappingValues.Add( asset );
                }
            }

            EditorUtility.SetDirty( this );
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() );
            AssetDatabase.SaveAssets();
        }
#endif
        [SerializeField, Tooltip("This string is appended to the front of the name of an object after it's created or taken from a pool.")]
        protected string ojbectNamePrefix = "Symbol: ";

        [SerializeField]
        protected List<string> objectMappingKeys;

        [SerializeField]
        protected List<PoolableMonoBehaviour> objectMappingValues;

        protected List<string> EditorKeys
        {
            get
            {
                return objectMappingKeys != null ? objectMappingKeys : (objectMappingKeys = new List<string>());
            }
        }

        protected List<PoolableMonoBehaviour> EditorValues
        {
            get
            {
                return objectMappingValues != null ? objectMappingValues : (objectMappingValues = new List<PoolableMonoBehaviour>());
            }
        }

        protected Dictionary<string, MonoBehaviourPool<object, PoolableMonoBehaviour>> map;

        public virtual IDictionary<string, MonoBehaviourPool<object, PoolableMonoBehaviour>> Map
        {
            get
            {
                return map ?? (map = new Dictionary<string, MonoBehaviourPool<object, PoolableMonoBehaviour>>());
            }
            set
            {
                UnbindCallbacks();
                UnloadPools();
                Map.Clear();

                foreach(var kvp in value)
                    Map.Add(kvp.Key, kvp.Value);

                BindCallbacks();
            }
        }

        public virtual MonoBehaviourPool<object, PoolableMonoBehaviour> this[string key]
        {
            get
            {
                try
                {
                    return Map[key];
                }
                catch(KeyNotFoundException e)
                {
                    Debug.LogError(e.Message + " " + key);
                    throw e;
                }
            }
            set
            {
                if(Map.ContainsKey(key))
                {
                    Map[key].OnCreateObject -= SetupSymbolName;
                    Map[key].UnloadAll();
                }
                Map[key] = value;
                Map[key].OnCreateObject += SetupSymbolName;
            }
        }

        public virtual TPoolableMonoBehaviour Get<TPoolableMonoBehaviour>(string key, object setupData, params object[] initParams)
            where TPoolableMonoBehaviour : PoolableMonoBehaviour
        {
            return this[key].Get<TPoolableMonoBehaviour>(setupData, initParams);
        }

        public virtual PoolableMonoBehaviour Get(string key, object setupData, params object[] initParams)
        {
            return Get<PoolableMonoBehaviour>(key, setupData, initParams);
        }

        public virtual PoolableMonoBehaviour CheckDelayedAndGet(string key, object setupData, params object[] initParams)
        {
            if(delayedEnPool.Count > 0 && delayedEnPool.ContainsKey(key) && delayedEnPool[key].Count > 0)
            {
                PoolableMonoBehaviour delayedItem = delayedEnPool[key].FirstOrDefault();
                delayedEnPool[key].RemoveAt(0);
                return delayedItem;
            }

            return Get<PoolableMonoBehaviour>(key, setupData, initParams);
        }

        Dictionary<string, List<PoolableMonoBehaviour>> delayedEnPool = new Dictionary<string, List<PoolableMonoBehaviour>>();

        public virtual void DelayedEnPool(string key, PoolableMonoBehaviour objectToEnPool)
        {
            if(!delayedEnPool.ContainsKey(key))
                delayedEnPool.Add(key, new List<PoolableMonoBehaviour>());
            delayedEnPool[key].Add(objectToEnPool);
        } 

        public virtual void FlushDelayedEnPools()
        {
            if(delayedEnPool.Count <= 0)
                return;

            foreach(var pool in delayedEnPool)
            {
                if(pool.Value.Count > 0)
                {
                    pool.Value.ForEach(x => this[pool.Key].EnPool(x));
                    pool.Value.Clear();
                }
            }

            delayedEnPool.Clear();
        }

        protected virtual void LateUpdate()
        {
            FlushDelayedEnPools();
        }

        public virtual void EnPool(string key, PoolableMonoBehaviour objectToEnPool)
        {
            this[key].EnPool(objectToEnPool);
        }

        public virtual MonoBehaviourPool<object, PoolableMonoBehaviour> Create(string key, PoolableMonoBehaviour prefab)
        {
            this[key] = new MonoBehaviourPool<object, PoolableMonoBehaviour>() { Prefab = prefab };
            return this[key];
        }

        protected virtual void SetupSymbolName(IObjectPool<object, PoolableMonoBehaviour> pool, object setupData, PoolableMonoBehaviour obj, params object[] initParams)
        {
            obj.name = ojbectNamePrefix + setupData.ToString() + " " + obj.name;
        }

        protected virtual void SetupPools()
        {
            if(objectMappingValues == null || objectMappingValues.Count <= 0)
                return;

            for(int i = 0; i < objectMappingValues.Count; ++i)
            {
                if(Application.isEditor && !Application.isPlaying)
                {
                    if(objectMappingValues[i] == null)
                        continue;
                    //if(objectMappingKeys[i] is string && string.IsNullOrEmpty(objectMappingKeys[i] as string))
                    //    continue;
                }

                if(objectMappingValues[i] == null)
                    throw new System.NullReferenceException("Symbol mapping value may not be null.");

                //if(objectMappingKeys[i] is string && string.IsNullOrEmpty(objectMappingKeys[i] as string))
                //    throw new System.NullReferenceException("Symbol mapping key may not be null or empty.");

                Map[objectMappingKeys[i]] = new MonoBehaviourPool<object, PoolableMonoBehaviour>() { Prefab = objectMappingValues[i] };
            }
        }

        protected virtual void BindCallbacks()
        {
            foreach(var s in Map)
            {
                s.Value.OnCreateObject -= SetupSymbolName;
                s.Value.OnCreateObject += SetupSymbolName;
            }
        }

        protected virtual void UnbindCallbacks()
        {
            foreach(var s in Map)
            {
                s.Value.OnCreateObject -= SetupSymbolName;
            }
        }

        public virtual void UnloadPools()
        {
            foreach(var s in Map)
            {
                if(s.Value != null)
                    s.Value.UnloadAll();
            }
        }

        protected virtual void OnApplicationQuit()
        {
            UnbindCallbacks();
            UnloadPools();
        }

        protected virtual void Awake()
        {
            SetupPools();
            BindCallbacks();
        }

        protected virtual void OnEnable()
        {
            SetupPools();
            BindCallbacks();
        }

        protected virtual void OnDisable()
        {
            UnbindCallbacks();
        }

        protected virtual void OnDestroy()
        {
            UnbindCallbacks();
            UnloadPools();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem(editor.Consts.Menu.ROOT + editor.Consts.Menu.ASSETS + "/Create MonoBehaviourFactory")]
        public static MonoBehaviourFactory Create()
        {
            return editor.ScriptableObjectEditor.CreateScriptableObject<MonoBehaviourFactory>(Application.dataPath, allowCreateAssetInPlayMode: false);
        }
        
        public static MonoBehaviourFactory Create(string assetFolderPath, bool useFileExplorer)
        {
            if(!assetFolderPath.StartsWith(editor.Consts.AssetDatabase.REQUIRED_ROOT_PATH))
                throw new System.Exception("path name must start with "+ editor.Consts.AssetDatabase.REQUIRED_ROOT_PATH);

            return editor.ScriptableObjectEditor.CreateScriptableObject<MonoBehaviourFactory>(assetFolderPath, useFileExplorer, allowCreateAssetInPlayMode: false);
        }
#endif
    }

    public interface IMonoBehaviourFactory
    {
        TPoolableMonoBehaviour Get<TPoolableMonoBehaviour>(string key, object setupData, params object[] initParams)
            where TPoolableMonoBehaviour : PoolableMonoBehaviour;

        PoolableMonoBehaviour Get(string key, object setupData, params object[] initParams);

        void EnPool(string key, PoolableMonoBehaviour objectToEnPool);
    }
}
