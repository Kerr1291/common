using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace nv
{
    public abstract class ProcGenMap : ScriptableObject
    {
        public Vector2 mapSize = new Vector2(100, 100);

        [EditScriptable]
        public MapElement defaultFillElement;

        public abstract IEnumerator Generate();

        protected virtual IEnumerator WriteTestOutput(ArrayGrid<MapElement> map)
        {
            string testOutputPath = "Assets/common/MapGeneration/";
            Texture2D debugOutput = map.ArrayGridToTexture(WriteColor);
            yield return WriteToFile(debugOutput, testOutputPath + this.GetType().Name + ".png");
        }

        protected virtual IEnumerator WriteToFile(Texture2D tex, string filepath)
        {
            yield return new WaitForEndOfFrame();

            // Encode texture into PNG
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(filepath, bytes);

            yield break;
        }

        protected virtual Color WriteColor(MapElement type)
        {
            return WriteColor(type.type);
        }

        protected virtual Color WriteColor(int type)
        {
            if(type == 1)
                return Color.green;
            if(type == 2)
                return Color.grey;
            if(type == 3)
                return Color.white;
            if(type == 4)
                return Color.blue;
            if(type == 5)
                return Color.red;
            if(type == 6)
                return Color.yellow;
            if(type == 7)
                return Color.cyan;
            return Color.clear;
        }

        protected virtual ArrayGrid<MapElement> CreateBaseMap()
        {
            return new ArrayGrid<MapElement>(mapSize, defaultFillElement);
        }
    }
}
