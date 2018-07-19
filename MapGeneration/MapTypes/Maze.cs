using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

namespace nv
{
    public class Maze : ProcGenMap
    {
        public bool allowLoops = false;

        [Range(0f,1f)]
        public float loopProb = .8f;

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
            List<Vector2Int> drillers = new List<Vector2Int>();
            drillers.Add(new Vector2Int(map.w / 2, map.h / 2));
            while(drillers.Count > 0)
            {
                int i = 0;
                while(i < drillers.Count)
                {
                    Vector2Int di = drillers[i];

                    bool removeDriller = false;
                    int behavior = GameRNG.Rand(4);
                    if(behavior == 0)
                    {
                        di.y -= 2;
                        if(di.y < 0 || map[di] == defaultCorridorElement)
                        {
                            removeDriller = (!allowLoops || (allowLoops && GameRNG.Randf() < loopProb));
                        }
                        if(!removeDriller)
                            map[di.x, di.y + 1] = defaultCorridorElement;
                    }
                    else if(behavior == 1)
                    {
                        di.y += 2;
                        if(di.y >= map.h || map[di] == defaultCorridorElement)
                        {
                            removeDriller = true;
                        }
                        else
                        {
                            map[di.x, di.y - 1] = defaultCorridorElement;
                        }
                    }
                    else if(behavior == 2)
                    {
                        di.x -= 2;
                        if(di.x < 0 || map[di] == defaultCorridorElement)
                        {
                            removeDriller = true;
                        }
                        else
                        {
                            map[di.x + 1, di.y] = defaultCorridorElement;
                        }
                    }
                    else if(behavior == 3)
                    {
                        di.x += 2;
                        if(di.x >= map.w || map[di] == defaultCorridorElement)
                        {
                            removeDriller = true;
                        }
                        else
                        {
                            map[di.x - 1, di.y] = defaultCorridorElement;
                        }
                    }

                    if(removeDriller)
                        drillers.RemoveAt(i);
                    else
                    {
                        drillers[i] = di;
                        drillers.Add(di);
                        drillers.Add(di);
                        map[di] = defaultCorridorElement;
                        ++i;
                    }
                }
            }

            return true;
        }
    }
}
