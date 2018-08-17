using UnityEngine;
using System.Collections;

using System;

namespace nv
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
}