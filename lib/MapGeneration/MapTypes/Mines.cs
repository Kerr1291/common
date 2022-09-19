using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

namespace nv
{
    public class Mines : ProcGenMap
    {
        public int maxNumberOfRooms = 10;

        public Range roomSizeLimitX = new Range(6f, 11f);
        public Range roomSizeLimitY = new Range(6f, 11f);
        public Vector2Int roomSizeOffset = new Vector2Int(4, 4);

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
            Vector2Int s0 = Vector2Int.zero;
            Vector2Int s1 = Vector2Int.zero;
            Vector2Int p0 = Vector2Int.zero;
            Vector2Int p1 = Vector2Int.zero;
            Vector2Int p2 = Vector2Int.zero;
            Vector2Int diff = Vector2Int.zero;

            List<Rect> rooms = new List<Rect>();
            List<int> roomTypes = new List<int>();

            // Place rooms
            for(int i = 0; i < maxNumberOfRooms; ++i)
            {
                // size of room
                s1 = new Vector2Int(roomSizeLimitX.RandomValuei(), roomSizeLimitY.RandomValuei());
                
                bool result = map.GetPositionOfRandomAreaOfType(defaultWallElement, s1 + roomSizeOffset, ref p0);
                if(result)
                {
                    p0 = p0.AddScalar(2);

                    // Connect the room to existing one

                    if(rooms.Count > 0)
                    {
                        int selectedRoom = GameRNG.Rand(rooms.Count);

                        Rect r = rooms[selectedRoom];

                        // center of this room
                        p1 = p0 + s1.DivideBy(2);

                        // center of second room
                        p2 = Vector2Int.FloorToInt(r.center);

                        // found the way to connect rooms
                        diff = p2 - p1;

                        diff.x = Mathf.Abs(diff.x);
                        diff.y = Mathf.Abs(diff.y);

                        s0 = p1;

                        while(!(diff.x == 0 && diff.y == 0))
                        {
                            if(GameRNG.Rand(diff.x + diff.y) < diff.x)// move horizontally
                            {
                                diff.x--;
                                s0.x += (s0.x > p2.x ? -1 : 1);
                            }
                            else
                            {
                                diff.y--;
                                s0.y += (s0.y > p2.y ? -1 : 1);
                            }

                            // Check what is on that position
                            if(map[s0] == defaultRoomElement)
                            {
                                break;
                            }
                            else if(map[s0] == defaultCorridorElement)
                            {
                                if(GameRNG.CoinToss())
                                    break;
                            }

                            map[s0] = defaultCorridorElement;
                        }
                    }

                    // add to list of rooms

                    Rect roomRect = Rect.MinMaxRect(p0.x, p0.y, p0.x + s1.x, p0.y + s1.y);

                    rooms.Add(roomRect);
                    roomTypes.Add(i);

                    // draw_room

                    int roomType = GameRNG.Rand(4); //select the type of room we should generate

                    if(s1.x == s1.y)
                        roomType = 3;

                    if(roomType != 2)
                    {
                        IEnumerator<Vector2Int> areaIter = Mathnv.GetAreaEnumerator(s1);

                        while(areaIter.MoveNext())
                        {
                            Vector2Int current = areaIter.Current;
                            if(roomType == 0 || roomType == 1)// rectangle room
                            {
                                map[p0 + current] = defaultRoomElement;
                            }
                            else// round room
                            {
                                if(Vector2Int.Distance(s1.DivideBy(2), current) < s1.x / 2)
                                {
                                    map[p0 + current] = defaultRoomElement;
                                }
                            }
                        }
                    }
                    else //diamond
                    {
                        IEnumerator<Vector2Int> areaIter = Mathnv.GetAreaEnumerator(s1.DivideBy(2) + Vector2Int.one);

                        while(areaIter.MoveNext())
                        {
                            Vector2Int current = areaIter.Current;
                            if(current.y >= current.x)
                            {
                                int x0 = p0.x + current.x + s1.DivideBy(2).x;
                                int x1 = p0.x + s1.DivideBy(2).x - current.x;

                                int y0 = p0.y + current.y;
                                int y1 = p0.y + s1.y - current.y;

                                map[x0, y0] = defaultRoomElement;
                                map[x0, y1] = defaultRoomElement;
                                map[x1, y0] = defaultRoomElement;
                                map[x1, y1] = defaultRoomElement;
                            }
                        }
                    }
                } // end of room addition
            }

            return true;
        }        
    }
}
