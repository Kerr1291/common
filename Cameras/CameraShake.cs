﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace nv
{
    public class CameraShake : MonoBehaviour
    {
        public class StartShakeMessage
        {
            public float shakeTime;
        }

        public class StopShakeMessage { }

        [SerializeField]
        nv.CommunicationNode node = new nv.CommunicationNode();

        [Header("Shake extents")]
        public Vector3 axisShakeMin;
        public Vector3 axisShakeMax;

        [Tooltip("Time (x axis) is normalized from 0 to 1")]
        public AnimationCurve shakeAmountOverTime;

        Vector3 startPos;
        IEnumerator shakeCamera;

        void OnEnable()
        {
            node.EnableNode(this);
        }

        void OnDisable()
        {
            node.DisableNode();
        }

        public void StartShake(float shakeTime = 0f)
        {
            if(shakeTime <= 0f)
                return;

            if(shakeCamera != null)
            {
                StopCoroutine(shakeCamera);
            }
            else
            {
                startPos = transform.position;
            }

            shakeCamera = ShakeCamera(shakeTime);
            StartCoroutine(ShakeCamera(shakeTime));
        }

        public void StopShake()
        {
            shakeCamera = null;
            transform.position = startPos;
        }

        IEnumerator ShakeCamera(float shakeTime)
        {
            YieldInstruction fixedYield = new WaitForFixedUpdate();

            float currentTime = 0f;
            while(currentTime < shakeTime)
            {
                currentTime += Time.fixedDeltaTime;

                float normalizedTime = currentTime / shakeTime;

                float shakeAmount = shakeAmountOverTime.Evaluate(normalizedTime);

                //TODO: replace with Dev.Random instad of unity's
                transform.position = startPos + new Vector3(Random.Range(axisShakeMin.x, axisShakeMax.x), Random.Range(axisShakeMin.y, axisShakeMax.y), Random.Range(axisShakeMin.z, axisShakeMax.z)) * shakeAmount;
                yield return fixedYield;
            }

            shakeCamera = null;
            transform.position = startPos;
        }

#if UNITY_EDITOR
        [ContextMenu("Start Demo Shake")]
        void DemoStartShake()
        {
            StartShake(4f);
        }

        [ContextMenu("Stop Demo Shake")]
        void DemoStopShake()
        {
            StopShake();
        }
#endif

        [CommunicationCallback]
        void HandleStartCameraShake(StartShakeMessage msg)
        {
            StartShake(msg.shakeTime);
        }

        [CommunicationCallback]
        void HandleStopCameraShake(StopShakeMessage msg)
        {
            StopShake();
        }
    }
}