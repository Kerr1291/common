using UnityEngine;
using System.Collections;
using System;

namespace nv
{
    public class ListMouseScroller : ListViewScroller
    {
        public bool active = true;

        public float scrollDamping = 0.95f;
        public float maxSpeed = 5f;
        public float stopSpeed = 1f;
        public float dragRate = .1f;

        protected float currentScrollingRate = 0f;
        protected bool scrolling;
        protected Vector3 startPosition;
        protected Vector3 endPosition;
        protected Vector3 prevPosition;

        void Update()
        {
            if(!active)
                return;
            HandleInput();
        }

        void FixedUpdate()
        {
            if(!active)
                return;
            Scroll();
        }

        protected override void HandleInput()
        {
            if(Input.GetMouseButtonDown(0))
            {
                StartScrolling();
            }

            if(scrolling)
            {
                Vector3 delta = Input.mousePosition - prevPosition;
                listView.DataPosition += delta.y * dragRate;
                prevPosition = Input.mousePosition;
            }

            if(Input.GetMouseButtonUp(0))
            {
                StopScrolling();
            }
        }

        protected override void StartScrolling()
        {
            if(scrolling)
                return;
            scrolling = true;

            startPosition = Input.mousePosition;
        }

        protected override void Scroll()
        {
            if(Mathf.Approximately(currentScrollingRate, 0f))
                return;

            listView.DataPosition += currentScrollingRate * Time.fixedDeltaTime;
            currentScrollingRate *= Mathf.Abs(scrollDamping);
            if(Mathf.Abs(currentScrollingRate) <= stopSpeed)
                currentScrollingRate = 0f;
            Debug.Log(currentScrollingRate);
        }

        protected override void StopScrolling()
        {
            if(!scrolling)
                return;
            scrolling = false;

            endPosition = Input.mousePosition;

            Vector3 delta = endPosition - startPosition;
            currentScrollingRate += delta.y;
            currentScrollingRate = Mathf.Clamp(currentScrollingRate, -maxSpeed, maxSpeed);
        }
    }
}
