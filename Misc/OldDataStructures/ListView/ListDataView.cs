using UnityEngine;
using System.Collections.Generic;

namespace Components
{
    //This is essentially like a typedef for a simple element
    public class ListDataView : ListDataView<ListDataInspectorView>
    {
    }

    //--Model Type--
    //Custom data types will derive from this
    public class ListData
    {
        public string viewPrefab;

        public virtual bool HasViewPrefab
        {
            get
            {
                return !string.IsNullOrEmpty(viewPrefab);
            }
        }

        public string ViewPrefabTypeName
        {
            get
            {
                return viewPrefab;
            }
        }
    }

    //allows elements to be represented (later on) in the inspector
    public class ListDataViewBase : MonoBehaviour
    {
        public Transform LocalTransform { get; protected set; }
    }

    //--View Type--
    //Custom element views will derive from this
    public class ListDataView<DataType> : ListDataViewBase 
        where DataType : ListData
    {
        [Header("Data linked to this view")]
        public DataType data;

        protected virtual void DefaultSetup(DataType data)
        {
            this.data = data;
        }

        protected virtual void DefaultPoolStrategy(bool entering_pool)
        {
            gameObject.SetActive(!entering_pool);
        }

        //clear the view from using this data
        public virtual void ClearView()
        {
            data = null;
        }

        //assign this view to use this data and map the data to this view
        public virtual void BindDataToView(DataType data)
        {
            DefaultSetup(data);
        }

        //defines how the view should be managed when its placed in a pool
        //returns a reference to the object that will go into the pool
        //Example: if you want to destroy the object, instead of pooling it, return null
        public virtual ListDataView<DataType> OnEnPool()
        {
            DefaultPoolStrategy(true);
            return this;
        }

        //defines how the view should be managed when its removed from a pool
        public virtual void OnDePool()
        {
            DefaultPoolStrategy(false);
        }

        //called the first time a view is created
        public virtual void OnCreate()
        {
            //minor optimization
            LocalTransform = transform;
            DefaultPoolStrategy(false);
        }
    }

    //Enables the simple base "typedef" type
    [System.Serializable]
    public class ListDataInspectorView : ListData
    {
    }

    public class ListDataViewPool<DataType, ViewType>
            where DataType : ListData
            where ViewType : ListDataView<DataType>
    {
        public readonly GameObject prefab;
        protected readonly Queue<ViewType> pool = new Queue<ViewType>();
        protected readonly Dictionary<int,ViewType> activeViews = new Dictionary<int,ViewType>();

        public ListDataViewPool(GameObject prefab)
        {
            if(prefab == null)
                Debug.LogError("Template prefab cannot be null");
            this.prefab = prefab;
        }

        public virtual int Count
        {
            get
            {
                return pool.Count;
            }
        }

        public virtual void UnloadPooledViews()
        {
            foreach(ViewType view in pool)
            {
                if(view == null || view.gameObject == null)
                    continue;

                if(!Application.isPlaying)
                    GameObject.DestroyImmediate(view.gameObject);
                else
                    GameObject.Destroy(view.gameObject);
            }
            pool.Clear();
        }

        public virtual ViewType TryGetActiveView(int virtual_index)
        {
            ViewType dataView;
            if(!activeViews.TryGetValue(virtual_index, out dataView))
                return null;
            return dataView;
        }

        public virtual bool HasActiveView(int virtual_index)
        {
            return activeViews.ContainsKey(virtual_index);
        }

        public virtual void TrimActiveViews(List<int> active_indices)
        {
            if(activeViews.Count <= 0)
                return;

            List<int> keys = new List<int>(activeViews.Keys);
            for(int i = 0; i < keys.Count; ++i)
            {
                if(!active_indices.Contains(keys[i]))
                {
                    EnPoolDataView(keys[i]);
                }
            }
        }

        public virtual void ClearActiveViews()
        {
            if(activeViews.Count <= 0)
                return;

            List<int> keys = new List<int>(activeViews.Keys);
            for(int i = 0; i < keys.Count; ++i)
            {
                EnPoolDataView(keys[i]);
            }
        }

        public virtual void EnPoolDataView(int virtual_index)
        {
            ViewType dataView;
            if(!activeViews.TryGetValue(virtual_index, out dataView))
                return;

            //don't allow pooling of null data
            if(dataView == null)
            {
                activeViews.Remove(virtual_index);
                return;
            }

            //the user's pooling strategy might request that the object be destroyed
            dataView = (ViewType)dataView.OnEnPool();
            if(dataView != null)
            {
                pool.Enqueue(dataView);
                dataView.ClearView();
            }

            activeViews.Remove(virtual_index);
        }

        public virtual ViewType DePoolDataView(DataType data, int virtual_index)
        {
            ViewType dataView = pool.Dequeue();
            activeViews.Add(virtual_index, dataView);
            dataView.BindDataToView(data);
            dataView.OnDePool();
            return dataView;
        }

        public virtual ViewType CreateViewInstance(DataType data, Transform parent, int virtual_index)
        {
            GameObject obj = (GameObject)GameObject.Instantiate(prefab, parent, false);
            ViewType dataView = obj.GetComponent<ViewType>();
            activeViews.Add(virtual_index, dataView);
            dataView.BindDataToView(data);
            dataView.OnCreate();
            return dataView;
        }
    }

    //Used when you want to have elements that nest themselves in the list; won't be needed by most
    //example: public class AdvancedListData : ListDataNestedData<AdvancedElementData>
    //         public class AdvancedListDataView : ListDataView<AdvancedElementData>
    //
    public class ListDataNestedData<ChildType> : ListData
    {
        public bool expanded;
        public List<ChildType> children;
    }
}
/*
//
// EXAMPLE
//

//Setting up a "normal" element

    //This is the data type
[System.Serializable]
public class TData : ListElementData
{
    public float t;
}

    //This is the view for the data type, created when the object is brought into view
public class DTextItem : ListElement<TData>
{
    public override void SetupView(TData data)
    {
        base.SetupView(data);

        Transform temp = data.viewInstance.GetComponent<Transform>();
        if(temp != null)
            temp.Translate(Vector3.one * data.t);
    }
}


//
//
//
*/
