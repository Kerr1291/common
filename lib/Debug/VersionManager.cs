using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.Callbacks;
using UnityEditor;
#endif

namespace nv
{
    public class VersionManager : ScriptableSingleton<VersionManager>
    {
        [System.Serializable]
        public class ProjectVersionData
        {
            public int releaseBuild;
            public int debugBuild;
            public int minor;
            public string buildDate;
        }

        [SerializeField, HideInInspector]
        protected ProjectVersionData projectVersionData = new ProjectVersionData();

        [System.Serializable]
        public class SceneVersionData
        {
            public string sceneName;
            public int sceneVersion;
            public string sceneDate;
        }

        [SerializeField, HideInInspector]
        protected List<SceneVersionData> sceneVersionData = new List<SceneVersionData>();

#if UNITY_EDITOR
        [MenuItem("NV/Assets/Create VersionManager")]
        public static void Create()
        {
            CreateEditorInstance();
        }
#endif

        [SerializeField, Tooltip("Controls the displaying of a popup for notifying user that a saved scene is not in the project's build list (and so cannot be versioned).")]
        protected bool showBuildPopup = true;

        [SerializeField, Tooltip("Changing this does nothing. This is for debug display only.")]
        protected string versionDisplay;

        [SerializeField, Tooltip("Changing this does nothing. This is for debug display only.")]
        protected string versionDate;

        [SerializeField, Tooltip("Used to identify when a local build is created VS a \'release\' build through another system (like SCM)")]
        protected string perforceWorkspaceName;

        [SerializeField]
        protected string lastSavedBuildPath;

        /// <summary>
        /// The current version of the project - Format: ReleaseBuild.DebugBuild.MinorChange
        /// </summary>
        public virtual string ProjectVersion
        {
            get
            {
                return projectVersionData.releaseBuild.ToString().PadLeft(1, '0') + "."
                    + projectVersionData.debugBuild.ToString().PadLeft(2, '0') + "."
                    + projectVersionData.minor.ToString().PadLeft(4, '0');
            }
        }

        /// <summary>
        /// The current date of the project
        /// </summary>
        public virtual string ProjectVersionDate
        {
            get
            {
                return projectVersionData.buildDate;
            }
        }

        /// <summary>
        /// Returns the version of this scene. Will return "Unversioned" if the scene is not in the project's build list.
        /// </summary>
        public virtual string SceneVersion(GameObject go)
        {
            return SceneVersion(go.scene.name);
        }

        /// <summary>
        /// Returns the version of this scene. Will return "Unversioned" if the scene is not in the project's build list.
        /// </summary>
        public virtual string SceneVersion(Scene scene)
        {
            return SceneVersion(scene.name);
        }

        /// <summary>
        /// Returns the version of this scene. Will return "Unversioned" if the scene is not in the project's build list.
        /// </summary>
        public virtual string SceneVersion(string sceneName)
        {
            int sIndex = GetSceneIndex(sceneName);

            if(sIndex < 0)
                return "Unversioned";

            return sceneVersionData[sIndex].sceneVersion.ToString();
        }


        /// <summary>
        /// Returns the date of this scene. Will return "Unversioned" if the scene is not in the project's build list.
        /// </summary>
        public virtual string SceneVersionDate(GameObject go)
        {
            return SceneVersionDate(go.scene.name);
        }

        /// <summary>
        /// Returns the date of this scene. Will return "Unversioned" if the scene is not in the project's build list.
        /// </summary>
        public virtual string SceneVersionDate(Scene scene)
        {
            return SceneVersionDate(scene.name);
        }

        /// <summary>
        /// Returns the date of this scene. Will return "Unversioned" if the scene is not in the project's build list.
        /// </summary>
        public virtual string SceneVersionDate(string sceneName)
        {
            int sIndex = GetSceneIndex(sceneName);

            if(sIndex < 0)
                return "Unversioned";

            return sceneVersionData[sIndex].sceneDate;
        }

        protected virtual int GetSceneIndex(string sceneName)
        {
            return sceneVersionData.Select(x => x.sceneName).ToList().IndexOf(sceneName);
        }

#if UNITY_EDITOR
        protected virtual string BuildDateTime
        {
            get
            {
                return (System.DateTime.Now.ToLongDateString()) + " " + System.DateTime.Now.ToLongTimeString();
            }
        }

        [ContextMenu("Clear All Version Data")]
        public virtual void ClearAllVersionData()
        {
            bool result = EditorUtility.DisplayDialog("Confirm clear", "Clear and reset all version data?", "Clear");
            if(result)
            {
                projectVersionData.releaseBuild = 0;
                projectVersionData.debugBuild = 0;
                projectVersionData.minor = 0;
                versionDisplay = ProjectVersion;

                projectVersionData.buildDate = BuildDateTime;
                versionDate = ProjectVersionDate;

                sceneVersionData.Clear();
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

        [ContextMenu("Increment Build Version Manually")]
        public virtual void IncrementBuildVersionData()
        {
            projectVersionData.debugBuild++;
            projectVersionData.minor = 0;
            versionDisplay = ProjectVersion;

            projectVersionData.buildDate = BuildDateTime;
            versionDate = ProjectVersionDate;

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        protected virtual void UpdateReleaseVersion()
        {
            projectVersionData.releaseBuild++;
            projectVersionData.debugBuild = 0;
            projectVersionData.minor = 0;
            versionDisplay = ProjectVersion;

            projectVersionData.buildDate = BuildDateTime;
            versionDate = ProjectVersionDate;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        protected virtual void UpdateBuildVersion()
        {
            projectVersionData.debugBuild++;
            projectVersionData.minor = 0;
            versionDisplay = ProjectVersion;

            projectVersionData.buildDate = BuildDateTime;
            versionDate = ProjectVersionDate;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        protected virtual void UpdateSceneVersion(string sceneName)
        {
            int sIndex = GetSceneIndex(sceneName);
            if(sIndex < 0)
            {
                SyncVersionedScenes();
                sIndex = GetSceneIndex(sceneName);
            }

            if(sIndex < 0)
                return;

            sceneVersionData[sIndex].sceneVersion++;
            sceneVersionData[sIndex].sceneDate = BuildDateTime;

            projectVersionData.minor++;

            versionDisplay = ProjectVersion;
            versionDate = ProjectVersionDate;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        protected virtual void OnEnable()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaved -= OnSceneSaved;
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaved += OnSceneSaved;
        }

        protected virtual void OnDestroy()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneSaved -= OnSceneSaved;
        }

        protected virtual void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
        {
            CheckAndShowPopupToAddSceneToBuildSettings(scene);
            UpdateSceneVersion(GetNameFromPath(scene.path));

            versionDisplay = ProjectVersion;
            versionDate = ProjectVersionDate;
        }

        [PostProcessBuildAttribute(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            Instance.lastSavedBuildPath = pathToBuiltProject;
            if(UnityEditor.EditorUserBuildSettings.development || pathToBuiltProject.Contains(Instance.perforceWorkspaceName))
            {
                Debug.Log("Updating build version");
                Instance.UpdateBuildVersion();
            }
            else
            {
                Debug.Log("Updating release version");
                Instance.UpdateReleaseVersion();
            }
        }

        protected virtual void CheckAndShowPopupToAddSceneToBuildSettings(UnityEngine.SceneManagement.Scene scene)
        {
            if(!showBuildPopup)
                return;

            if(scene.buildIndex >= 0)
                return;

            bool result = EditorUtility.DisplayDialog("Add scene to build settings?", "The scene that was just saved is not in the project's build settings. Add it to the the build settings now?", "Add Scene: " + scene.name, "Cancel");
            if(result)
            {
                var list = UnityEditor.EditorBuildSettings.scenes.ToList();
                list.Add(new EditorBuildSettingsScene(scene.path, true));
                UnityEditor.EditorBuildSettings.scenes = list.ToArray();
            }
            else
            {
                RemoveScene(GetNameFromPath(scene.path));
                result = EditorUtility.DisplayDialog("Hide this notification?", "Stop showing notifications when saving scenes not in the build settings list? (You may re-enabled this by selecting the " + typeof(VersionManager).Name + " scriptable object and changing the option.", "Stop showing this popup", "Continue showing this popup");
                if(result)
                {
                    showBuildPopup = false;
                }
            }
        }

        protected virtual string GetNameFromPath(string path)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            return name;
        }

        protected virtual void SyncVersionedScenes()
        {
            List<string> orphans = new List<string>();

            //search project for orphans
            for(int i = 0; i < sceneVersionData.Count; ++i)
            {
                bool found = UnityEditor.EditorBuildSettings.scenes.Any(x => string.Compare(GetNameFromPath(x.path), sceneVersionData[i].sceneName) == 0);
                if(!found)
                {
                    orphans.Add(sceneVersionData[i].sceneName);
                }
            }

            foreach(string s in orphans)
            {
                RemoveScene(s);
            }

            foreach(var s in UnityEditor.EditorBuildSettings.scenes)
            {
                string sceneName = GetNameFromPath(s.path);
                if(!sceneVersionData.Any(x => string.Compare(x.sceneName, sceneName) == 0))
                {
                    AddScene(sceneName);
                }
            }
        }

        protected virtual void AddScene(string sceneName)
        {
            SceneVersionData newScene = new SceneVersionData()
            {
                sceneDate = System.DateTime.Now.ToLongDateString(),
                sceneName = sceneName,
                sceneVersion = 0
            };
            sceneVersionData.Add(newScene);
        }

        protected virtual void RemoveScene(string sceneName)
        {
            int index = GetSceneIndex(sceneName);
            if(index < 0)
                return;

            sceneVersionData.RemoveAt(index);
        }
#endif
    }
}
