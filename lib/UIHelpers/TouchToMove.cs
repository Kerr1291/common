using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace mods.Internal.MediaServiceAPI
{
    public class TouchToMove : MonoBehaviour
    {
        public Transform target;
        bool tracking = false;

        Vector3 startPoint;

        public void OnClick()
        {
            if(tracking)
                return;
            startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            tracking = true;
        }

        private void Update()
        {
            if(tracking && Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                target.position = startPoint + (endPoint - startPoint);
                tracking = false;
            }
        }
    }
}