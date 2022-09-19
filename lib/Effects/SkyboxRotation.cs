using UnityEngine;
using System.Collections;

namespace nv
{
    [RequireComponent(typeof(Skybox)), ExecuteInEditMode]
    public class SkyboxRotation : MonoBehaviour
    {
        protected Skybox sykbox;
        public Skybox Skybox
        {
            get
            {
                if(sykbox == null)
                    sykbox = GetComponent<Skybox>();
                return sykbox;
            }
        }

        //public Material volumetricSpace;

        public bool rotateInEditMode = false;
        public float rotationRate = 1.0f;
        //store the current rotation locally in case the material is changed at runtime
        public float currentRotation = 0f;

        float fixedDT;
        float currentTimeScale;


        const string RotationShaderField = "_Rotation";
        public float Rotation
        {
            get
            {
                return currentRotation;
            }
            set
            {
                float fractional = value - ((int)value);
                int modRotation = Mathnv.Modulus(Mathf.FloorToInt(value), 360);
                currentRotation = modRotation + fractional;
                Skybox.material.SetFloat(RotationShaderField, currentRotation);
            }
        }

        public float CurrentRotationRate
        {
            get
            {
                return rotationRate * fixedDT * currentTimeScale;
            }
        }

        private void OnEnable()
        {
            currentRotation = Rotation;
            fixedDT = Time.fixedDeltaTime;
        }

        private void OnValidate()
        {
            Rotation = currentRotation;
        }

        //cache timescale each frame so time is only accessed once per frame
        private void Update()
        {
            if(!Application.isPlaying && !rotateInEditMode)
                return;

            currentTimeScale = Time.timeScale;

            if(!Application.isPlaying)
            {
                Rotation += CurrentRotationRate;
            }
        }
        
        void FixedUpdate()
        {
            if(!Application.isPlaying)
                return;

            //TODO: consider summing the rotation and moving the apply into late update?
            Rotation += CurrentRotationRate;
        }

        //private void OnRenderImage(RenderTexture source, RenderTexture destination)
        //{
        //    if(destination == null && volumetricSpace != null)
        //    {
        //        volumetricSpace.SetTexture("_MainTex", source);
        //        Graphics.Blit(source, destination, volumetricSpace);
        //    }
        //}
    }

}