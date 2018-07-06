using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace nv
{
    public class SceneLoader : MonoBehaviour
    {
        public bool loadOnAwake = true;

        [SceneAssetPathField]
        public string[] mainScenes;

        public void LoadScenes()
        {
            for(int i = 0; i < mainScenes.Length; ++i)
            {
#if UNITY_EDITOR
                //avoid double-loading scenes that are already loaded
                if(Application.isPlaying)
                {
                    if(SceneManager.GetSceneByName(mainScenes[i]).isLoaded)
                        continue;
                    SceneManager.LoadSceneAsync(mainScenes[i], LoadSceneMode.Additive);
                }
                else
                {
                    if(EditorSceneManager.GetSceneByName(mainScenes[i]).isLoaded)
                        continue;
                    EditorSceneManager.OpenScene(mainScenes[i], OpenSceneMode.Additive);
                }
#else
                if(SceneManager.GetSceneByName(mainScenes[i]).isLoaded)
                    continue;
                
                 SceneManager.LoadSceneAsync(mainScenes[i], LoadSceneMode.Additive);  
#endif              
            }
        }

        public void Awake()
        {
            if(loadOnAwake)
                LoadScenes();
        }
    }
}
