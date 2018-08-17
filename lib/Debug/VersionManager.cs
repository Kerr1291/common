using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.Callbacks;
using UnityEditor;
#endif

namespace nv
{
    public class VersionManager : ScriptableSingleton<VersionManager>
    {
        [Tooltip("Controls the displaying of a popup for notifying user that a saved scene is not in the project's build list (and so cannot be versioned).")]
        public bool showBuildPopup = true;

        [Tooltip("Changing this does nothing. This is for debug display only.")]
        public string versionDisplay;

        /// <summary>
        /// The current version of the project - ReleaseBuild.DebugBuild.MinorChange
        /// </summary>
        public string ProjectVersion
        {
            get
            {
                return releaseBuild.ToString().PadLeft(1, '0') + "."
                    + debugBuild.ToString().PadLeft(2, '0') + "." 
                    + minor.ToString().PadLeft(4, '0');
            }
        }

        /// <summary>
        /// Returns the version of this scene. Will return "Unversioned" if the scene is not in the project's build list.
        /// </summary>
        public string SceneVersion(string sceneName)
        {
            int sIndex = GetSceneIndex(sceneName);

            if(sIndex < 0)
                return "Unversioned";

            return sceneVersions[sIndex].ToString();
        }

        [SerializeField, HideInInspector]
        int releaseBuild;
        [SerializeField, HideInInspector]
        int debugBuild;
        [SerializeField, HideInInspector]
        int minor;

        [SerializeField,HideInInspector]
        List<string> sceneNames;
        [SerializeField, HideInInspector]
        List<int> sceneVersions;

#if UNITY_EDITOR
        [ContextMenu("Clear All Version Data")]
        void ClearAllVersionData()
        {
            bool result = EditorUtility.DisplayDialog("Confirm clear", "Clear and reset all version data?", "Clear");
            if(result)
            {
                releaseBuild = 0;
                debugBuild = 0;
                minor = 0;
                sceneNames.Clear();
                sceneVersions.Clear();
                versionDisplay = ProjectVersion;
            }
        }

        void UpdateReleaseVersion()
        {
            releaseBuild++;
            debugBuild = 0;
            minor = 0;
            versionDisplay = ProjectVersion;
        }

        void UpdateBuildVersion()
        {
            debugBuild++;
            minor = 0;
            versionDisplay = ProjectVersion;
        }

        void UpdateSceneVersion(string sceneName)
        {
            int sIndex = GetSceneIndex(sceneName);
            if(sIndex < 0)
            {
                SyncVersionedScenes();
                sIndex = GetSceneIndex(sceneName);
            }

            if(sIndex < 0)
                return;

            sceneVersions[sIndex]++;
            minor++;
            versionDisplay = ProjectVersion;
        }

        void OnEnable()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaved -= OnSceneSaved;
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaved += OnSceneSaved;
            versionDisplay = ProjectVersion;
        }

        void OnDestroy()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaved -= OnSceneSaved;
        }

        public static void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
        {
            Instance.CheckAndShowPopupToAddSceneToBuildSettings(scene);
            Instance.UpdateSceneVersion(scene.path);
        }        

        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if(UnityEditor.EditorUserBuildSettings.development)
            {
                Instance.UpdateReleaseVersion();
            }
            else
            {
                Instance.UpdateBuildVersion();
            }
        }

        void CheckAndShowPopupToAddSceneToBuildSettings(UnityEngine.SceneManagement.Scene scene)
        {
            if(!showBuildPopup)
                return;

            if(scene.buildIndex >= 0)
                return;

            bool result = EditorUtility.DisplayDialog("Add scene to build settings?", "The scene that was just saved is not in the project's build settings. Add it to the the build settings now?", "Add Scene: "+scene.name, "Cancel");
            if(result)
            {
                var list = UnityEditor.EditorBuildSettings.scenes.ToList();
                list.Add(new EditorBuildSettingsScene(scene.path, true));
                UnityEditor.EditorBuildSettings.scenes = list.ToArray();
            }
            else
            {
                RemoveScene(scene.path);
                result = EditorUtility.DisplayDialog("Hide this notification?", "Stop showing notifications when saving scenes not in the build settings list? (You may re-enabled this by selecting the "+typeof(VersionManager).Name+" scriptable object and changing the option.", "Stop showing this popup", "Continue showing this popup");
                if(result)
                {
                    showBuildPopup = false;
                }
            }
        }

        void SyncVersionedScenes()
        {
            List<string> orphans = new List<string>();

            //search project for orphans
            for(int i = 0; i < sceneNames.Count; ++i)
            {
                bool found = UnityEditor.EditorBuildSettings.scenes.Where(x => x.path.Contains(sceneNames[i])).Count() > 0;
                if(!found)
                    orphans.Add(sceneNames[i]);
            }

            foreach(string s in orphans)
            {
                RemoveScene(s);
            }

            foreach(var s in UnityEditor.EditorBuildSettings.scenes)
            {
                if(!sceneNames.Contains(s.path))
                {
                    AddScene(s.path);
                }
            }
        }

        int GetSceneIndex(string sceneName)
        {
            return sceneNames.IndexOf(sceneName);
        }

        void AddScene(string sceneName)
        {
            sceneNames.Add(sceneName);
            sceneVersions.Add(0);
        }

        void RemoveScene(string sceneName)
        {
            int index = GetSceneIndex(sceneName);
            if(index < 0)
                return;
            sceneNames.RemoveAt(index);
            sceneVersions.RemoveAt(index);
        }
#endif
    }
}
