using UnityEngine;
using System.Collections;

namespace nv
{

    public class GarbageManager : MonoBehaviour
    {
        public static GarbageManager Instance
        {
            get;
            private set;
        }

        [SerializeField]
        bool useStartupGarbageCollection = false;

        [SerializeField]
        [Header("Chunks of heap allocated on startup")]
        int defaultHeapSize = 1024;

        [SerializeField]
        [Header("Size of each chunk")]
        int defaultChunkSize = 1024;

        [SerializeField]
        [Header("Force GC Collect at intervals?")]
        bool useCustomGarbageCollection = false;

        [SerializeField]
        [Header("Rate to force GC.Collect in frames")]
        int garbageCollectionRate = 120;

        void Awake()
        {
            if(Instance != null)
                return;

            Instance = this;

            if(useStartupGarbageCollection)
            {
                var temp = new System.Object[defaultHeapSize];

                // make allocations in smaller blocks to avoid them to be treated in a special way, which is designed for large blocks
                for(int i = 0; i < defaultHeapSize; i++)
                    temp[i] = new byte[defaultChunkSize];

                // release reference
                temp = null;
            }
        }

        void Update()
        {
            if(useCustomGarbageCollection == false)
                return;

            if(Time.frameCount % garbageCollectionRate == 0)
            {
                RunGarbageCollection();
            }
        }

        /// <summary>
        /// Warning: Can cause significant frame spikes. Call this during "safe" preiods in your game when there is no action.
        /// </summary>
        public void RunGarbageCollection()
        {
            System.GC.Collect();
        }
    }

}