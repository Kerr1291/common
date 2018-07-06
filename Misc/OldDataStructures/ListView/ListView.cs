using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Components
{
    #region SimpleListView
    /// <summary>
    /// This is essentially like a typedef for a simple list and an example of how you COULD
    /// implement a list view.
    /// in addition to to it being an example of how to implement a list.
    /// 
    /// For your own custom lists, mimic this but replace the generic parameters
    /// with your own custom data type and view type.
    /// </summary>
    public class ListView : ListView<ListDataInspectorView, ListDataView>
    {
        //Example of using a default element for when no view is provided for some data
        public GameObject defaultViewPrefab;

        public virtual List<ListDataInspectorView> listData { get; set; }

        public override int DataCount
        {
            get
            {
                if(listData == null)
                    return 0;
                return listData.Count;
            }
        }
        
        public override float DataPosition
        {
            get; set;
        }

        public virtual float MaxScrollOffset
        {
            get
            {
                return (DataCount - ViewCount);
            }
        }

        public virtual float Spacing
        {
            get; set;
        }

        public virtual float ElementSize
        {
            get; set;
        }

        public override int DataIndex
        {
            get; set;
        }

        public virtual int EndVisibleIndex
        {
            get
            {
                return DataIndex + ViewCount;
            }
            set
            {
                if(value > DataIndex)
                {
                    ViewCount = Mathf.Min(value - DataIndex, DataCount);
                    ViewCount = Mathf.Max(ViewCount, 0);
                }
                else
                    ViewCount = 0;
            }
        }

        public override int ViewCount
        {
            get; set;
        }

        public virtual Vector3 ScrollDirection
        {
            get; set;
        }

        public virtual Vector3 ListPosition
        {
            get
            {
                return transform.localPosition;
            }
            set
            {
                transform.localPosition = value;
            }
        }

        protected virtual void SetupDefaults()
        {
            //create the list if nothing else has yet
            if(listData == null)
                listData = new List<ListDataInspectorView>();

            //simple example settings
            DataPosition = 0f;
            Spacing = 1f;
            ScrollDirection = Vector3.down;
            ElementSize = 1f;

            //initial list view
            DataIndex = 0;
            ViewCount = 5;
        }

        public override void Setup()
        {
            base.Setup();
            SetupDefaults();
        }

        public override void Add(ListDataInspectorView data)
        {
            if(!data.HasViewPrefab)
            {
                if(defaultViewPrefab != null)
                {
                    data.viewPrefab = defaultViewPrefab.name;
                    Debug.LogWarning("Warning: Adding element without view information. Applying default list prefab to unknown data.");
                }
                else
                {
                    Debug.LogWarning("Warning: Adding element without view information. This may cause nullref errors.");
                }
            }

            int addedIndex = listData.Count;
            listData.Add(data);
            EnPoolDataView(data, addedIndex);
        }

        public override bool Remove(ListDataInspectorView data)
        {
            int index = listData.IndexOf(data);
            EnPoolDataView(data, index);
            return listData.Remove(data);
        }

        public override void RemoveAt(int index)
        {
            EnPoolDataView(listData[index], index);
            listData.RemoveAt(index);
        }

        public override List<ListDataInspectorView> GetDataInView()
        {
            List<ListDataInspectorView> visibleElements = new List<ListDataInspectorView>();
            for(int i = DataIndex; i < EndVisibleIndex; ++i)
                visibleElements.Add(listData[i]);
            return visibleElements;
        }

        public override bool IsInView(ListDataInspectorView data)
        {
            for(int i = DataIndex; i < EndVisibleIndex; ++i)
            {
                if(listData[i] == data)
                    return true;
            }
            return false;
        }

        //Is this index one that is visible?
        public override bool IsInView(int data_index)
        {
            return (data_index >= DataIndex && data_index < EndVisibleIndex);
        }

        //Get an element by the indexing into the list of currently visible objects
        public override ListDataInspectorView GetDataAtViewIndex(int view_index)
        {
            if(view_index < 0 || view_index >= ViewCount)
                return null;

            int v_index = DataIndex + view_index;
            return listData[v_index];
        }
        
        public override int GetDataIndexAtViewIndex(int view_index)
        {
            if(view_index < 0 || view_index >= ViewCount)
                return -1;
            int v_index = DataIndex + view_index;
            return v_index;
        }

        //Return the first index found with an object that matches this element
        public override int GetViewIndex(ListDataInspectorView visibleObject)
        {
            for(int i = DataIndex; i < EndVisibleIndex; ++i)
            {
                int current = i - DataIndex;
                if(listData[i] == visibleObject)
                    return current;
            }
            return -1;
        }

        public override void Clear()
        {
            for(int i = 0; i < DataCount; ++i)
                EnPoolDataView(listData[i],i);

            listData.Clear();
        }

        public override void CalculateDataPositionAndIndex()
        {
            //clamp the scroll offset
            DataPosition = Mathf.Min(DataPosition, MaxScrollOffset);
            DataPosition = Mathf.Max(DataPosition, 0f);

            DataIndex = (int)Mathf.Round(DataPosition);
        }

        public override void CalculateDataInView()
        {
            for(int i = 0; i < DataCount; i++)
            {
                //if the index is outside the active range, enpool it
                //else, create it (from a pool if needed) and position it on the screen
                if(i < DataIndex || i >= EndVisibleIndex)
                {
                    //this element is not visible, place its view into a pool
                    if(!IsInView(i))
                        EnPoolDataView(listData[i], i);
                }
                else
                {
                    //get our view out of the pool if needed
                    ListDataView view_element = DePoolDataView(listData[i], i);
                    CalculateElementPosition(view_element, i);
                }
            }
        }

        protected virtual void CalculateElementPosition(ListDataView view_element, int offset)
        {
            view_element.transform.localPosition = ListPosition + (offset * (ElementSize + Spacing) - DataPosition * (Spacing + ElementSize)) * ScrollDirection;
        }
    }
    #endregion

    /// <summary>
    /// Base type that allows the generic list to be a unity component
    /// </summary>
    #region ListViewBase
    public abstract class ListViewBase : MonoBehaviour
    {
        [Tooltip("Item template prefabs (usually, at least one is required)")]
        public List<GameObject> viewPrefabs;

        public virtual GameObject GetViewPrefab(string data_typename)
        {
            for(int i = 0; i < viewPrefabs.Count; ++i)
            {
                if(viewPrefabs[i].name == data_typename)
                    return viewPrefabs[i];
            }
            return null;
        }

        public abstract float DataPosition { get; set; }

        public abstract int DataIndex { get; set; }

        public virtual int ViewTypeCount { get { return viewPrefabs.Count; } }

        public abstract bool Loaded { get; }

        public abstract int DataCount { get; }

        public abstract int ViewCount { get; set; }

        public abstract void Setup();
        
        public abstract void UpdateView();

        public abstract void CalculateDataPositionAndIndex();

        public abstract void CalculateDataInView();
    }
    #endregion

    /// <summary>
    /// Implement this type for your custom lists
    /// </summary>
    #region TListView
    public abstract class ListView<DataType, ViewType> : ListViewBase
            where DataType : ListData
            where ViewType : ListDataView<DataType>
    {
        //used to generate the "view" of each list element
        protected readonly Dictionary<string, ListDataViewPool<DataType, ViewType>> _pools = new Dictionary<string, ListDataViewPool<DataType, ViewType>>();
        
        public override bool Loaded
        {
            get
            {
                return _pools.Count > 0;
            }
        }

        //call to initialize the list before use
        public override void Setup()
        {
            if(Loaded)
                return;

            if(ViewTypeCount < 1)
            {
                Debug.LogError("No view prefabs found in this list view! A List view requires at least one view prefab.");
            }

            for(int i = 0; i < ViewTypeCount; ++i)
            {
                if(HasPool(viewPrefabs[i].name))
                {
                    Debug.LogError("Multiple view prefabs with the same name detected: "+viewPrefabs[i].name+". Ignoring the extras.");
                }

                CreatePool(viewPrefabs[i].name, viewPrefabs[i]);
            }
        }

        //call to update/recalculate the visible elements
        public override void UpdateView()
        {
            if(!Loaded)
                return;

            CalculateDataPositionAndIndex();
            CalculateDataInView();
        }

        public abstract void Add(DataType data);

        public abstract bool Remove(DataType data);

        public abstract void RemoveAt(int index);

        public abstract List<DataType> GetDataInView();

        public abstract bool IsInView(DataType data);

        public abstract bool IsInView(int data_index);

        public abstract DataType GetDataAtViewIndex(int view_index);

        public abstract int GetDataIndexAtViewIndex(int view_index);

        public abstract int GetViewIndex(DataType visibleObject);

        public abstract void Clear();

        #region PoolMethods
        public virtual void UnloadPooledData()
        {
            foreach(var pool in _pools)
            {
                pool.Value.UnloadPooledViews();
            }
            _pools.Clear();
        }

        protected virtual bool HasPool(string pool_name)
        {
            return _pools.ContainsKey(pool_name);
        }

        protected virtual ListDataViewPool<DataType, ViewType> GetPool(string pool_name)
        {
            ListDataViewPool<DataType, ViewType> pool;
            if(!_pools.TryGetValue(pool_name, out pool))
                return null;
            return pool;
        }

        protected virtual void CreatePool(string pool_name, GameObject pool_prefab)
        {
            _pools[pool_name] = new ListDataViewPool<DataType, ViewType>(pool_prefab);
        }

        //if this data is bound to a view, place the view into a pool
        //and clear data's reference to the view
        protected virtual void EnPoolDataView(DataType data, int virtual_index)
        {
            if(data == null || !data.HasViewPrefab)
                return;

            ListDataViewPool<DataType, ViewType> dataViewPool = GetPool(data.ViewPrefabTypeName);

            if(!dataViewPool.HasActiveView(virtual_index))
                return;

            //use the view's pooling strategy to store it in the pool
            dataViewPool.EnPoolDataView(virtual_index);
        }

        //if this check the pools for the view and see if this virtual index should be bound to this data
        //and clear data's reference to the view
        protected virtual ViewType DePoolDataView(DataType data, int virtual_index)
        {
            if(data == null)
            {
                Debug.LogError("Tried to get element view with null data");
                return null;
            }

            ListDataViewPool<DataType, ViewType> dataViewPool = GetPool(data.ViewPrefabTypeName);

            if(dataViewPool == null)
            {
                Debug.LogError("Cannot get pool for data with prefab " + data.ViewPrefabTypeName + "; This prefab doesn't exist in the list's pools");
                return null;
            }

            //does it already have an active view? then return that view
            ViewType dataView = dataViewPool.TryGetActiveView(virtual_index);
            if(dataView != null)
                return dataView;

            //are there views in the pool or do we need to create a new view?
            if(dataViewPool.Count > 0)
            {
                //get element from pool and associate it with this data
                dataView = dataViewPool.DePoolDataView(data, virtual_index);
            }
            else
            {
                //create an element associate it with this data and parent it to this transform
                dataView = dataViewPool.CreateViewInstance(data, transform, virtual_index);
            }

            return dataView;
        }
        #endregion
    }
    #endregion
}

