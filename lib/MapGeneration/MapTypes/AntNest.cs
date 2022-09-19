using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

namespace nv
{
    public class AntNest : ProcGenMap
    {
        public Range strideLimitX = new Range(10f, 100f);
        public Range strideLimitY = new Range(10f, 100f);
        public int maxTunnelIterations = 1000;

        public bool placeRooms = true;
        public Range roomSizeX = new Range(3, 3);
        public Range roomSizeY = new Range(3, 3);

        public int maxGenerationAttempts = 100;
        public bool debugOutput = true;
        int generationAttempts = 0;

        public override ArrayGrid<MapElement> GeneratedMap
        {
            get; protected set;
        }

        [EditScriptable]
        public MapElement defaultRoomElement;

        [EditScriptable]
        public MapElement defaultCorridorElement;

        [EditScriptable]
        public MapElement defaultDoorElement;

        public MapElement defaultWallElement
        {
            get
            {
                return defaultFillElement;
            }
        }

        //values used by the room connection algorithm
        int valueRoom = 0;
        int valueWall = -1;
        int valueHall = -2;

        public override IEnumerator Generate()
        {
            //cannot generate with nonzero map size
            if(mapSize.sqrMagnitude <= 0)
                yield break;

            generationAttempts = 0;

            ArrayGrid<MapElement> map = null;
            ArrayGrid<int> valueMap = null;

            Debug.Log("Starting generation");
            for(; ; )
            {
                //brief yield before each genration attempt
                yield return null;
                if(generationAttempts > maxGenerationAttempts)
                {
                    Debug.LogError("Failed to generate map with current settings");
                    break;
                }

                map = CreateBaseMap();
                valueMap = new ArrayGrid<int>(mapSize, valueWall);

                bool success = GenerateMap(map, valueMap);

                generationAttempts++;
                if(success)
                    break;
            }

            Debug.Log("Done");
            yield return WriteTestOutput(map);

            GeneratedMap = map;

            yield break;
        }

        bool GenerateMap(ArrayGrid<MapElement> map, ArrayGrid<int> valueMap)
        {
            map[map.CenterPosition] = defaultCorridorElement;

            Vector2Int s0 = Vector2Int.zero;
            Vector2 s1 = Vector2Int.zero;
            Vector2Int p = Vector2Int.zero;
            Vector2 d = Vector2Int.zero;

            int iterCount = map.Count / 3;

            for(int i = 0; i < iterCount; i++)
            {
                s1 = GameRNG.RandomPointOnCircle(map.Size.DivideBy(2)) + map.CenterPosition;

                d.x = strideLimitX.RandomNormalizedValue();
                d.y = strideLimitY.RandomNormalizedValue();

                d.x -= .5f;
                d.y -= .5f;

                int counter = 0;
                for(; ; )
                {
                    //try again
                    if(counter > maxTunnelIterations)
                    {
                        --i;
                        break;
                    }

                    s1 += d;

                    p = Vector2Int.FloorToInt(s1);
                    p = map.Wrap(p);

                    if(map.IsValidPosition(p) && map.GetAdjacentPositionsOfType(p,false,defaultCorridorElement).Count > 0)
                    {
                        map[p] = defaultCorridorElement;
                        break;
                    }
                }
            }

            if(placeRooms)
            {
                var areaIter = Mathnv.GetAreaEnumerator(Vector2Int.one, map.MaxValidPosition - Vector2Int.one);
                while(areaIter.MoveNext())
                {
                    Vector2Int current = areaIter.Current;

                    Range limitX = new Range(map.w / 2 - strideLimitX.Min, map.w / 2 + strideLimitX.Min);
                    Range limitY = new Range(map.h / 2 - strideLimitY.Min, map.h / 2 + strideLimitY.Min);

                    if((limitX.Contains(current.x) && limitY.Contains(current.y)) || map[current] == defaultWallElement)
                        continue;

                    int n = map.GetAdjacentPositionsOfType(current, false, defaultCorridorElement).Count;

                    if(n == 1)
                    {
                        Rect room = new Rect(Vector2.zero, new Vector2(roomSizeX.RandomValuei(), roomSizeY.RandomValuei()));
                        
                        var roomIter = Mathnv.GetAreaEnumerator(Vector2Int.FloorToInt(room.min), Vector2Int.FloorToInt(room.max));
                        while(roomIter.MoveNext())
                        {
                            Vector2Int rCurrent = roomIter.Current;
                            map[current + rCurrent] = defaultRoomElement;
                        }
                    }
                }
            }

            return true;
        }
    }
}
