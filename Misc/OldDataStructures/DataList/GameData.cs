using UnityEngine;
using System.Collections;

using Components.Common;
using System;

namespace Components
{
    [System.Serializable]
    public class GameData : ListData
    {
        public string gameData;

        protected virtual bool Equals(GameData other)
        {
            return string.Equals(gameData, other.gameData);
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            return Equals((GameData)obj);
        }

        public override int GetHashCode()
        {
            return gameData.GetHashCode();
        }
    }

    ///Example of creating a custom symbol data type and custom view for it
    ///

    //[System.Serializable]
    //public class CustomSymbolData : SymbolData
    //{
    //    //your data and overrides here
    //}

    //public class CustomSymbolDataView : ListDataView<CustomSymbolData>
    //{
    //    //your data and overrides here
    //}    
}