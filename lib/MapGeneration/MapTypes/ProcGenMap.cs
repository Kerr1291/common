using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

namespace nv
{
    public abstract class ProcGenMap : ScriptableObject
    {
        public abstract ArrayGrid<MapElement> GeneratedMap
        {
            get; protected set;
        }

        public Vector2Int mapSize = new Vector2Int(100, 100);
        
        [EditScriptable]
        public MapElement defaultFillElement;

        public abstract IEnumerator Generate();

        protected virtual IEnumerator WriteTestOutput(ArrayGrid<MapElement> map)
        {
            string testOutputPath = "Assets/common/lib/MapGeneration/";
            Texture2D debugOutput = map.ToTexture(WriteColor);
            yield return WriteToFile(debugOutput, testOutputPath + this.GetType().Name + ".png");
        }

        protected virtual IEnumerator WriteTestOutput(ArrayGrid<int> map)
        {
            Func<int, int> func = (int t) => { return t; };
            string testOutputPath = "Assets/common/lib/MapGeneration/";
            var debugOutput = map.ToData<int, int>(func);
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
            return WriteColor(type.debugColor);
        }

        protected virtual Color WriteColor(Color typeColor)
        {
            return typeColor;
        }

        //protected virtual Color WriteColor(int type)
        //{
        //    var matchingType = debugMappingData.Find(x => x.type == type);            
        //    return matchingType.color;
        //}

        protected virtual ArrayGrid<MapElement> CreateBaseMap()
        {
            return new ArrayGrid<MapElement>(mapSize, defaultFillElement);
        }


        static protected IEnumerator<Vector2Int> IterateOverMap<T>(ArrayGrid<T> map)
        {
            IEnumerator<Vector2Int> iterator = IterateOverArea(map.w, map.h);
            while(iterator.MoveNext())
                yield return iterator.Current;
        }

        static protected IEnumerator<int> IterateOver(int from, int to)
        {
            for(int i = from; i < to; ++i)
            {
                yield return i;
            }
            yield break;
        }

        static protected IEnumerator<Vector2Int> IterateOverArea(Vector2Int size)
        {
            IEnumerator<Vector2Int> iterator = IterateOverArea(size.x, size.y);
            while(iterator.MoveNext())
                yield return iterator.Current;
        }

        static protected IEnumerator<Vector2Int> IterateOverArea(int w, int h)
        {
            var yIter = IterateOver(0, h);

            while(yIter.MoveNext())
            {
                var xIter = IterateOver(0, w);

                while(xIter.MoveNext())
                {
                    yield return new Vector2Int(xIter.Current, yIter.Current);
                }
            }
        }
    }
}
