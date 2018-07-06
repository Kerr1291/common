using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Components
{
    #region View
    public class ShotList : ShotList<ShotView>
    {
    }

    /// <summary>
    /// This is essentially like a typedef for a simple list and an example of how you COULD
    /// implement a list view.
    /// in addition to to it being an example of how to implement a list.
    /// 
    /// For your own custom lists, mimic this but replace the generic parameters
    /// with your own custom data type and view type.
    /// </summary>
    public class ShotList<TDataView> : ListView<ShotData, TDataView>
            where TDataView : ListDataView<ShotData>
    {
        //Example of using a default element for when no view is provided for some data
        public GameObject defaultViewPrefab;

        [SerializeField]
        protected virtual List<ShotData> _listData { get; set; }

        [SerializeField]
        [HideInInspector]
        protected List<ShotData> _visibleData;

        public override int DataIndex {
            get; set;
        }

        public override int ViewCount {
            get; set;
        }

        public override int DataCount {
            get {
                if( ListData == null )
                    return 0;
                return ListData.Count;
            }
        }

        public override float DataPosition {
            get; set;
        }

        public virtual List<ShotData> ListData {
            get {
                return _listData;
            }
            set {
                if( _listData != null )
                {
                    Clear();
                }

                _listData = value;
            }
        }

        public override void Clear()
        {
            if( !Loaded )
                return;

            _visibleData.Clear();

            //clear all the pools
            foreach( var visibleSet in _pools )
                visibleSet.Value.ClearActiveViews();

            UnloadPooledData();

            //reset the data positions
            ViewCount = 0;
            DataPosition = 0f;

            //clear the list
            ListData.Clear();
        }

        public override bool Loaded {
            get {
                return base.Loaded;
            }
        }

        public override void Setup()
        {
            base.Setup();

            if( !HasPool( defaultViewPrefab.name ) )
                CreatePool( defaultViewPrefab.name, defaultViewPrefab );

            SetupDefaults();
        }

        protected virtual void SetupDefaults()
        {
            if( _visibleData == null )
                _visibleData = new List<ShotData>();

            //create the list if nothing else has yet
            if( ListData == null )
            {
                ListData = new List<ShotData>();
            }

            //initial list view
            ViewCount = 3;
        }
        public override ShotData GetDataAtViewIndex( int view_index )
        {
            return _visibleData[ view_index ];
        }

        public override int GetDataIndexAtViewIndex( int view_index )
        {
            for(int i = 0; i < ListData.Count; ++i )
            {
                if( ListData[ i ] == _visibleData[ view_index ] )
                    return i;
            }

            return -1;
        }
        public override bool Remove( ShotData data )
        {
            int index = ListData.IndexOf(data);
            EnPoolDataView( data, index );

            bool result = ListData.Remove(data);
            
            return result;
        }

        public override void RemoveAt( int index )
        {
            EnPoolDataView( ListData[ index ], index );
            ListData.RemoveAt( index );

        }

        //Return the first view index found with an object that matches this data, if it is in view
        public override int GetViewIndex( ShotData data )
        {
            for( int i = 0; i < ListData.Count; ++i )
            {
                if( data == _visibleData[ i ] )
                    return i;
            }

            return int.MinValue;
        }

        public override void Add( ShotData data )
        {
            if( !data.HasViewPrefab )
            {
                if( defaultViewPrefab != null )
                {
                    data.viewPrefab = defaultViewPrefab.name;
                    Debug.LogWarning( "Warning: Adding shot without prefab information. Applying default shot prefab to unknown shot data with name: " + data.shotData );
                }
                else
                {
                    Debug.LogWarning( "Warning: Adding shot without prefab information. This may cause nullref errors. Shot data name: " + data.shotData );
                }
            }

            int addedIndex = DataCount;
            ListData.Add( data );
            EnPoolDataView( data, addedIndex );
        }

        //is this data in the view?
        public override bool IsInView( ShotData data )
        {
            return _visibleData.Contains( data );
        }

        //is this data index in the view?
        public override bool IsInView( int data_index )
        {
            return _visibleData.Contains( ListData[ data_index ] );
        }

        public override List<ShotData> GetDataInView()
        {
            return _visibleData;
        }

        List<ShotData> _oldData = new List<ShotData>();

        public override void CalculateDataInView()
        {
            if( !Loaded )
                return;

            _oldData.Clear();

            for( int i = 0; i < DataCount; i++ )
            {
                //if the index is outside the active range, enpool it
                //else, create it (from a pool if needed) and position it on the screen
                if( !ListData[ i ].IsAlive )
                {
                    EnPoolDataView( ListData[ i ], i );
                    _oldData.Add( ListData[ i ] );
                }
                else
                {
                    //get our view out of the pool if needed
                    //ShotView view_element = DePoolDataView(ListData[i], i);
                    //TDataView view_element = 
                        DePoolDataView(ListData[i], i);
                }
            }

            for( int i = 0; i < _oldData.Count; i++ )
            {
                //Debug.Log( "removing " + _oldData[ i ].shotData );
                ListData.Remove( _oldData[ i ] );
            }

            _oldData.Clear();

            foreach( var visibleSet in _pools )
                (visibleSet.Value as ShotPool<TDataView>).ClearViewsWithNullData();

            //Dictionary<string, List<int>> visibleElementsByPool = new Dictionary<string, List<int>>();

            //for( int i = 0; i < _visibleData.Count; ++i )
            //{
            //    ShotData currentElement = _visibleData[i];

            //    string poolName = currentElement.ViewPrefabTypeName;
            //    if( visibleElementsByPool.ContainsKey( poolName ) == false )
            //        visibleElementsByPool[ poolName ] = new List<int>();

            //    visibleElementsByPool[ poolName ].Add( i );

            //    TDataView view_element = DePoolDataView(currentElement, i);
            //}

            //foreach( var visibleSet in _pools )
            //{
            //    if( !visibleElementsByPool.ContainsKey( visibleSet.Key ) )
            //        visibleSet.Value.ClearActiveViews();
            //    else
            //        visibleSet.Value.TrimActiveViews( visibleElementsByPool[ visibleSet.Key ] );
            //}
        }

        protected virtual void CalculateViewPosition( TDataView view_element, int view_index )
        {
            //do nothing
        }

        public override void CalculateDataPositionAndIndex()
        {
            //do nothing
            DataPosition = 0;
            DataIndex = 0;
        }

        protected override void CreatePool( string pool_name, GameObject pool_prefab )
        {
            _pools[ pool_name ] = new ShotPool<TDataView>( pool_prefab );
        }

        //if this data is bound to a view, place the view into a pool
        //and clear data's reference to the view
        protected override void EnPoolDataView( ShotData data, int virtual_index )
        {
            if( data == null || !data.HasViewPrefab )
                return;

            ShotPool<TDataView> dataViewPool = GetPool(data.ViewPrefabTypeName) as ShotPool<TDataView>;

            if( !dataViewPool.HasActiveView( data ) )
                return;

            //use the view's pooling strategy to store it in the pool
            dataViewPool.EnPoolDataView( data );
        }

        //if this check the pools for the view and see if this virtual index should be bound to this data
        //and clear data's reference to the view
        protected override TDataView DePoolDataView( ShotData data, int virtual_index )
        {
            if( data == null )
            {
                Debug.LogError( "Tried to get element view with null data" );
                return null;
            }

            ShotPool<TDataView> dataViewPool = GetPool(data.ViewPrefabTypeName) as ShotPool<TDataView>;

            if( dataViewPool == null )
            {
                Debug.LogError( "Cannot get pool for data with prefab " + data.ViewPrefabTypeName + "; This prefab doesn't exist in the list's pools" );
                return null;
            }

            //does it already have an active view? then return that view
            TDataView dataView = (dataViewPool as ShotPool<TDataView>).TryGetActiveView(data);
            if( dataView != null )
                return dataView;

            //are there views in the pool or do we need to create a new view?
            if( dataViewPool.Count > 0 )
            {
                //get element from pool and associate it with this data
                dataView = dataViewPool.DePoolDataView( data );
                if( dataView == null )
                    dataView = dataViewPool.CreateViewInstance( data, transform );
            }
            else
            {
                //create an element associate it with this data and parent it to this transform
                dataView = dataViewPool.CreateViewInstance( data, transform );
            }

            return dataView;
        }
    }
    #endregion



    public class ShotPool<TDataView> : ListDataViewPool<ShotData, TDataView>
            where TDataView : ListDataView<ShotData>
    {
        protected readonly List<TDataView> activeViewList = new List<TDataView>();
        //protected readonly Dictionary<int,ViewType> activeViews = new Dictionary<int,ViewType>();

        public ShotPool( GameObject prefab )
            :base( prefab )
        {
        }

        public TDataView TryGetActiveView( ShotData data )
        {
            for(int i = 0; i < activeViewList.Count; ++i )
            {
                if( activeViewList[ i ].data == data )
                    return activeViewList[ i ];
            }
            return null;
        }

        public bool HasActiveView( ShotData data )
        {
            for( int i = 0; i < activeViewList.Count; ++i )
            {
                if( activeViewList[ i ].data == data )
                    return true;
            }
            return false;
        }

        //public virtual void TrimActiveViews( List<ShotData> active_shots )
        //{
        //    if( activeViewList.Count <= 0 )
        //        return;

        //    for( int i = 0; i < activeViewList.Count; ++i )
        //    {
        //        if( !active_shots.Contains( activeViewList[ i ].data ) )
        //        {
        //            EnPoolDataView( activeViewList[ i ].data );
        //        }
        //    }
        //}

        public void ClearViewsWithNullData()
        {
            for( int i = 0; i < activeViewList.Count; ++i )
            {
                if( activeViewList[ i ] == null )
                    activeViewList.RemoveAt( i );
            }
        }

        public override void ClearActiveViews()
        {
            if( activeViewList.Count <= 0 )
                return;
            
            for( int i = 0; i < activeViewList.Count; ++i )
            {
                EnPoolDataView( activeViewList[ i ].data );
            }
        }

        public virtual void EnPoolDataView( ShotData data )
        {
            TDataView dataView = TryGetActiveView(data);

            //don't allow pooling of null data
            if( dataView == null )
            {
                activeViewList.Remove( dataView );
                return;
            }

            //the user's pooling strategy might request that the object be destroyed
            dataView = (TDataView)dataView.OnEnPool();
            if( dataView != null )
            {
                pool.Enqueue( dataView );
                dataView.ClearView();
            }

            activeViewList.Remove( dataView );
        }

        public virtual TDataView DePoolDataView( ShotData data )
        {
            //Error!!! Should not happen, but sometimes can; return a null to tell the list to get a new view
            if( pool.Peek() != null && pool.Peek().data != null )
            {
                TDataView badView = pool.Dequeue();
                GameObject.Destroy( badView.gameObject );
                return null;
            }

            TDataView dataView = pool.Dequeue();
            activeViewList.Add( dataView );
            dataView.BindDataToView( data );
            dataView.OnDePool();
            return dataView;
        }

        public virtual TDataView CreateViewInstance( ShotData data, Transform parent )
        {
            GameObject obj = (GameObject)GameObject.Instantiate(prefab, parent, false);
            TDataView dataView = obj.GetComponent<TDataView>();
            activeViewList.Add( dataView );
            dataView.BindDataToView( data );
            dataView.OnCreate();
            return dataView;
        }
    }
}

