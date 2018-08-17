using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace nv
{
    public class GameDataList : GameDataList<GameDataView>
    {
    }

    //Dummy classes to allow ScriptableObject to create a non-null instance
    public class JumpListOfGameData : JumpList<GameData>
    {
    }

    public class GameDataList<TDataView> : ListView<GameData, TDataView>
            where TDataView : ListDataView<GameData>
    {
        public static JumpListOfGameData CreateGameDataJumpList()
        {
            return ScriptableObject.CreateInstance<JumpListOfGameData>();
        }

        [Space(10)]
        //Editor prefab for previewing the layout 
        public GameObject editorPrefab;

        //Default element for when no view is provided 
        public GameObject defaultPrefab;
        
        [SerializeField][HideInInspector]
        protected JumpListOfGameData _listData;

        [SerializeField][HideInInspector]
        protected JumpListOfGameData.Iterator _listIter;

        [SerializeField][HideInInspector]
        protected List<GameData> _visibleData;

        //minor optimization to store this locally
        [SerializeField][HideInInspector]
        protected Transform listTransform;

        public virtual JumpListOfGameData ListData
        {
            get
            {
                return _listData;
            }
            set
            {
                if(_listData != null)
                {
                    Clear();
                    if(Application.isPlaying)
                        Destroy(_listData);
                    else
                        DestroyImmediate(_listData);
                }

                _listData = value;
            }
        }

        [SerializeField]
        [HideInInspector]
        protected bool _forwardIteration = true;

        public virtual bool ForwardIteration
        {
            get
            {
                return _forwardIteration;
            }
            set
            {
                if(value != _forwardIteration)
                    _listIter.ForwardIteration = value;

                _forwardIteration = value;
            }
        }

        public override int ViewCount
        {
            get; set;
        }

        public override int DataCount
        {
            get
            {
                if(ListData == null)
                    return 0;
                return ListData.Count;
            }
        }

        public override float DataPosition
        {
            get; set;
        }        

        public override int DataIndex
        {
            get
            {
                if(!Loaded)
                    return 0;
                return _listIter.CurrentIndex;
            }
            set
            {
                if(!Loaded)
                    return;
                _listIter.CurrentIndex = value;
            }
        }

        public virtual float ViewSpacing
        {
            get; set;
        }

        public virtual Vector3 LayoutDirection
        {
            get; set;
        }

        public virtual Vector3 LayoutOrigin
        {
            get
            {
                return listTransform.localPosition;
            }
            set
            {
                listTransform.localPosition = value;
            }
        }

        public override bool Loaded
        {
            get
            {
                return base.Loaded && DataCount > 0 && _listIter != null;
            }
        }

        protected virtual float IndexRoundingOffset
        {
            get
            {
                return .5f;
            }
        }

        protected virtual int DoIndexRounding(float value)
        {
            return (int)Mathf.Round(value);
        }

        public override void Setup()
        {
            listTransform = transform;

            base.Setup();

            if(Application.isEditor && !HasPool(editorPrefab.name))
                CreatePool(editorPrefab.name, editorPrefab);

            if(!HasPool(editorPrefab.name))
                CreatePool(defaultPrefab.name, defaultPrefab);

            SetupDefaults();
        }

        public override void Add(GameData data)
        {
            if(!data.HasViewPrefab)
            {
                if(defaultPrefab != null)
                {
                    data.viewPrefab = defaultPrefab.name;
                    Debug.LogWarning("Warning: Adding object without prefab information. Applying default object prefab to unknown object data with name: " + data.gameData);
                }
                else
                {
                    Debug.LogWarning("Warning: Adding object without prefab information. This may cause nullref errors. object data name: " + data.gameData);
                }
            }

            int addedIndex = DataCount;
            ListData.Add(data);
            EnPoolDataView(data, addedIndex);

            //did we just go from an empty list to a non empty list? then make sure our iterator is valid
            if(addedIndex == 0)
                RefreshListIter();
        }

        public override bool Remove(GameData data)
        {
            int index = ListData.IndexOf(data);
            EnPoolDataView(data, index);

            bool result = ListData.Remove(data);

            //did we just go from a non empty list to a empty list? then make sure our iterator is valid
            //note that this will cause the list to be marked as not loaded since it has nothing in it
            if(ListData.Count == 0)
                RefreshListIter();
            return result;
        }

        public override void RemoveAt(int index)
        {
            EnPoolDataView(ListData[index], index);
            ListData.RemoveAt(index);

            //did we just go from a non empty list to a empty list? then make sure our iterator is valid
            //note that this will cause the list to be marked as not loaded since it has nothing in it
            if(ListData.Count == 0)
                RefreshListIter();
        }

        public override List<GameData> GetDataInView()
        {
            List<GameData> visibleElements = _listIter.Peek(ViewCount);
            return visibleElements;
        }

        //is this data in the view?
        public override bool IsInView(GameData data)
        {
            List<GameData> visibleElements = _listIter.Peek(ViewCount);
            for(int i = 0; i < visibleElements.Count; ++i)
            {
                if(data.Equals(visibleElements[i]))
                    return true;
            }
            return false;
        }

        //Return the first view index found with an object that matches this data, if it is in view
        public override int GetViewIndex(GameData data)
        {
            List<GameData> visibleElements = _listIter.Peek(ViewCount);
            for(int i = 0; i < visibleElements.Count; ++i)
            {
                if(data.Equals(visibleElements[i]))
                    return i;
            }
            return int.MinValue;
        }

        //is this data index in the view?
        public override bool IsInView(int data_index)
        {
            List<int> visibleElements = _listIter.PeekIndices(ViewCount);
            for(int i = 0; i < visibleElements.Count; ++i)
            {
                if(data_index == visibleElements[i])
                    return true;
            }
            return false;
        }

        public override GameData GetDataAtViewIndex(int view_index)
        {
            List<JumpList<GameData>.Element> visibleElements = _listIter.PeekElements(ViewCount);
            return visibleElements[view_index].value;
        }

        public override int GetDataIndexAtViewIndex(int view_index)
        {
            List<int> visibleElements = _listIter.PeekIndices(ViewCount);
            return visibleElements[view_index];
        }

        //TODO: test me!
        public override void Clear()
        {
            if(!Loaded)
                return;

            _visibleData.Clear();

            //clear all the pools
            foreach(var visibleSet in _pools)
                visibleSet.Value.ClearActiveViews();

            UnloadPooledData();

            //reset the data positions
            ViewCount = 0;
            DataPosition = 0f;

            //clear the list
            ListData.Clear();

            //refresh the visible elements and iterator
            RefreshListIter();
        }

        //call to update/recalculate the visible elements
        public override void UpdateView()
        {
            if(!Loaded)
                return;

            CalculateDataPositionAndIndex();
            CalculateDataInView();
        }

        public override void CalculateDataPositionAndIndex()
        {
            if(!Loaded)
                return;

            float v = DataPosition;            

            //wrap our data position around if it goes off the end of the list
            //the .5 is the offset to match the rounding function
            if(v <= -IndexRoundingOffset)
            {
                while(v <= -IndexRoundingOffset)
                {
                    v += (1f);
                    ForwardIteration = false;
                    _listIter.MoveNext();
                }
            }
            else if(v >= IndexRoundingOffset)
            {
                while(v >= IndexRoundingOffset)
                {
                    v -= (1f);
                    ForwardIteration = true;
                    _listIter.MoveNext();
                }
            }

            //final sanity check to keep the position inside the list
            if(v <= -IndexRoundingOffset)
            {
                v = -.499f;
            }
            else if(v >= IndexRoundingOffset)
            {
                v = .499f;
            }

            DataPosition = v;
        }

        public override void CalculateDataInView()
        {
            if(!Loaded)
                return;

            _visibleData = _listIter.Peek(ViewCount);

            Dictionary<string, List<int>> visibleElementsByPool = new Dictionary<string, List<int>>();

            if(!ForwardIteration)
                _visibleData.Reverse();

            for(int i = 0; i < _visibleData.Count; ++i)
            {
                int virtual_index = i;

                GameData currentElement = _visibleData[i];

                string poolName = currentElement.ViewPrefabTypeName;
                if(visibleElementsByPool.ContainsKey(poolName) == false)
                    visibleElementsByPool[poolName] = new List<int>();

                visibleElementsByPool[poolName].Add(virtual_index);

                TDataView view_element = DePoolDataView(currentElement, virtual_index);

                CalculateViewPosition(view_element, virtual_index);
            }

            foreach(var visibleSet in _pools)
            {
                if(!visibleElementsByPool.ContainsKey(visibleSet.Key))
                    visibleSet.Value.ClearActiveViews();
                else
                    visibleSet.Value.TrimActiveViews(visibleElementsByPool[visibleSet.Key]);
            }
        }
        
        public virtual void ScrollView(float delta)
        {
            DataPosition += delta;
        }

        #region NonPublicMethods

        protected virtual void CalculateViewPosition(TDataView view_element, int view_index)
        {
            view_element.LocalTransform.localPosition = LayoutOrigin + (view_index * (ViewSpacing) - DataPosition * (ViewSpacing)) * LayoutDirection;
        }

        protected virtual void RefreshListIter()
        {
            _listIter = new JumpList<GameData>.Iterator(ListData, DataIndex, ForwardIteration);
        }

        protected virtual void SetupDefaults()
        {
            if(_visibleData == null)
                _visibleData = new List<GameData>();

            //create the list if nothing else has yet
            if(ListData == null)
            {
                ListData = CreateGameDataJumpList();
                RefreshListIter();
            }

            //simple example settings
            DataPosition = 0f;
            ViewSpacing = 2.5f;
            LayoutDirection = Vector3.up;

            //initial list view
            ViewCount = 5;
        }

        #endregion
    }
}