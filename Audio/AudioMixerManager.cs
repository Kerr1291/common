using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;

namespace nv
{
    public class AudioMixerManager : MonoBehaviour
    {
        /// <summary>
        /// publish a MixerParameter to set the value contained within
        /// </summary>
        public class MixerParameter
        {
            /// <summary>
            /// If empty, will apply to all audio mixers
            /// </summary>
            public string mixerName;            
            public string paramName;            
            public float value;
        }

        [Header("Required")]
        [SerializeField]
        AudioMixer audioMixer;

        [Header("Required")]
        [Tooltip("The mixer must have the volume exposed with this parameter name")]
        [SerializeField]
        string VolumeParameterName = "MasterVolume";

        [Header("Settings")]
        [SerializeField]
        [Range(0.0f, 1.0f)]
        float maxVolume = 0.0f;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        float minVolume = 1.0f;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        float defaultVolume = 0.5f;

        public UnityEngine.UI.Slider uiController;

        [SerializeField]
        [Tooltip("Enable to have this mixer added to all audio sources without a mixer in a scene on load")]
        bool autoAddMixerOnSceneLoad;

        [SerializeField] 
        CommunicationNode comms;

        //used to prevent recursive volume updates when updating the ui controller
        bool lockOnValueChange;
        
        public float Volume
        {
            get
            {
                float gameVolume = maxVolume + MixerVolume * (minVolume - maxVolume);
                return gameVolume;
            }
            set
            {
                lockOnValueChange = true;
                MixerVolume = Mathf.Clamp01(value);
                UpdateUIController(value);
                lockOnValueChange = false;
            }
        }

        public float MixerVolume
        {
            get
            {
                return this[VolumeParameterName];
            }
            set
            {
                this[VolumeParameterName] = value;
            }
        }

        public float this[string paramName]
        {
            get
            {
                float f = 0f;
                audioMixer.GetFloat(paramName, out f);
                return f;
            }
            set
            {
                audioMixer.SetFloat(paramName, value);
            }
        }

        void Start()
        {
            Volume = defaultVolume;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= AddMixerToAudioSourcesOnSceneLoad;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += AddMixerToAudioSourcesOnSceneLoad;
        }

        void OnEnable()
        {
            comms.EnableNode(this);

            if(!GameObject.FindObjectsOfType<AudioListener>().Any())
                gameObject.AddComponent<AudioListener>();

            if(uiController != null)
            {
                UpdateUIController(Volume);
                uiController.onValueChanged.RemoveListener(OnValueChange);
                uiController.onValueChanged.AddListener(OnValueChange);
            }
        }

        void OnDisable()
        {
            comms.DisableNode();
            
            if(uiController != null)
            {
                uiController.onValueChanged.RemoveListener(OnValueChange);
            }
        }

        void AddMixerToAudioSourcesOnSceneLoad(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if(!autoAddMixerOnSceneLoad)
                return;

            foreach(var rootGO in scene.GetRootGameObjects())
            {
                var emptyMixerSources = rootGO.GetComponentsInChildren<AudioSource>(true).Where(x => x != null && x.outputAudioMixerGroup == null).ToList();
                foreach(AudioSource source in emptyMixerSources)
                {
                    source.outputAudioMixerGroup = audioMixer.outputAudioMixerGroup;
                }
            }
        }

        [CommunicationCallback]
        public void SetParameter(object publisher, MixerParameter param)
        {
            if(string.IsNullOrEmpty(param.mixerName))
                this[param.paramName] = param.value;
            else if(param.mixerName == audioMixer.name)
                this[param.paramName] = param.value;
        }
        
        void UpdateUIController(float newVolume)
        {
            if(uiController == null)
                return;

            uiController.normalizedValue = newVolume;
        }
        
        public void OnValueChange(float newValue)
        {
            if(lockOnValueChange)
                return;

            if(uiController == null)
                return;

            Volume = uiController.normalizedValue;
        }
    }
}
