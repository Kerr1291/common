using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

namespace nv
{
    [System.Serializable]
    public struct DebugProcGenMappingData
    {
        public int type;
        public Color color;
    }

    public abstract class ProcGenMap : ScriptableObject
    {
        public List<DebugProcGenMappingData> debugMappingData;

        public Vector2Int mapSize = new Vector2Int(100, 100);

        [EditScriptable]
        public MapElement defaultFillElement;

        public abstract IEnumerator Generate();

        protected virtual IEnumerator WriteTestOutput(ArrayGrid<MapElement> map)
        {
            string testOutputPath = "Assets/common/MapGeneration/";
            Texture2D debugOutput = map.ArrayGridToTexture(WriteColor);
            yield return WriteToFile(debugOutput, testOutputPath + this.GetType().Name + ".png");
        }

        protected virtual IEnumerator WriteTestOutput(ArrayGrid<int> map)
        {
            Func<int, int> func = (int t) => { return t; };
            string testOutputPath = "Assets/common/MapGeneration/";
            var debugOutput = map.ArrayGridToData<int, int>(func);
            yield return WriteToFile(debugOutput, testOutputPath + this.GetType().Name + "_values.txt");
        }

        protected virtual IEnumerator WriteToFile(List<List<int>> outData, string filepath)
        {
            yield return new WaitForEndOfFrame();

            List<string> stringData = new List<string>();
            foreach(var set in outData)
            {
                string line = String.Join("", set.ConvertAll(s => s.ToString()).ToArray());
                stringData.Add(line);
            }
            File.WriteAllLines(filepath, stringData.ToArray());

            yield break;
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
            var matchingType = debugMappingData.Find(x => x.type == type);            
            return matchingType.color;
        }

        protected virtual ArrayGrid<MapElement> CreateBaseMap()
        {
            return new ArrayGrid<MapElement>(mapSize, defaultFillElement);
        }
    }
}
