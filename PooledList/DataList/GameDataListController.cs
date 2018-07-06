using UnityEngine;
using System.Collections;

namespace nv
{
    public class GameDataListController : GameDataListController<GameData, GameDataView>
    {
        [SerializeField]
        protected GameDataList _gameDataList;

        public GameDataList ContainedList
        {
            get
            {
                return _gameDataList;
            }
        }

        public override void UpdateController()
        {
            if(_gameDataList.DataIndex == StopVelocityAtIndex)
            {
                Velocity = 0f;
            }

            _gameDataList.ScrollView( Velocity * TimeScale );
        }

        public override void UpdateList()
        {
            if(!_gameDataList.Loaded)
                return;

            PreviousListIndex = _gameDataList.DataIndex;

            _gameDataList.UpdateView();

            CurrentListIndex = _gameDataList.DataIndex;
        }
    }

    public abstract class GameDataListControllerBase : MonoBehaviour
    {
        public abstract float TimeScale { get; set; }

        public abstract bool ForwardIteration { get; }

        public abstract int PreviousListIndex { get; protected set; }

        public abstract int CurrentListIndex { get; protected set; }

        public abstract float Velocity { get; set; }

        public abstract int StopVelocityAtIndex { get; set; }

        public abstract void SetupController();

        public abstract void UpdateController();

        public abstract void UpdateList();
    }

    public abstract class GameDataListController<TGameData, TGameDataView> : GameDataListControllerBase
                where TGameData : GameData
                where TGameDataView : GameDataView
    {
        [SerializeField]
        protected float fixedTimeScale;
        
        public override float TimeScale
        {
            get
            {
                return fixedTimeScale;
            }
            set
            {
                fixedTimeScale = value;
            }
        }

        public override bool ForwardIteration
        {
            get
            {
                return Velocity > 0;
            }
        }

        public override int PreviousListIndex { get; protected set; }

        public override int CurrentListIndex { get; protected set; }

        public override float Velocity { get; set; }

        public override int StopVelocityAtIndex { get; set; }

        public override void SetupController()
        {
            //minor optimization to store this value
            fixedTimeScale = Time.fixedDeltaTime;
            StopVelocityAtIndex = -1;
            CurrentListIndex = 0;
            PreviousListIndex = -1;
        }
    }
}
