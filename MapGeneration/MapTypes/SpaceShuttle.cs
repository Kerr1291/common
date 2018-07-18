using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

namespace nv
{
    public class SpaceShuttle : ProcGenMap
    {
        public int maxNumberOfRooms = 15;
        public bool mirrorVertical = false;
        public bool roomsTheSame = true;

        public Range roomSizeLimit = new Range(3f, 15f);
        
        public int maxGenerationAttempts = 100;
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
                {
                    GeneratedMap = map;
                    break;
                }
            }

            Debug.Log("Done");
            yield return WriteTestOutput(map);

            GeneratedMap = map;

            yield break;
        }

        bool GenerateMap(ArrayGrid<MapElement> map, ArrayGrid<int> valueMap)
        {
            Vector2Int p1 = Vector2Int.zero;
            Vector2Int p2 = Vector2Int.zero;
            Vector2Int r = Vector2Int.zero;

            List<Rect> rooms = new List<Rect>();
            List<int> roomTypes = new List<int>();

            int numberOfRooms = 0;

            for(numberOfRooms = 0; numberOfRooms < maxNumberOfRooms;)
            {
                if(numberOfRooms == 0)
                {
                    p1.x = map.w / 2 - GameRNG.Rand((int)roomSizeLimit.Max);
                    p1.y = map.h / 2 - GameRNG.Rand((int)roomSizeLimit.Max) - (int)roomSizeLimit.Min;

                    r.x = GameRNG.Rand((int)roomSizeLimit.Max) + (int)roomSizeLimit.Min;
                    r.y = roomSizeLimit.RandomValuei();

                    p2.x = p1.x + r.x;
                    p2.y = map.h;
                    if(p2.x >= map.w)
                        continue;
                }
                else
                {
                    p1.x = GameRNG.Rand(map.w - (int)roomSizeLimit.Min) + 1;
                    p1.y = GameRNG.Rand(map.h - (int)roomSizeLimit.Min) / 2 + 1;

                    r.x = roomSizeLimit.RandomValuei();
                    r.y = roomSizeLimit.RandomValuei();

                    p2.x = p1.x + r.x;
                    p2.y = p1.y + r.y;

                    if(p2.x >= map.w - 1 || p2.x >= map.h / 2 + 3)
                        continue;
                }

                bool randAgain = false;
                for(int j = 0; j < rooms.Count; ++j)
                {
                    randAgain = true;
                    if(rooms[j].Contains(p1))
                    {
                        if(!rooms[j].Contains(p2))
                        {
                            randAgain = false;
                            roomTypes[j]++;
                        }
                        break;
                    }
                    if(rooms[j].Contains(p2))
                    {
                        if(!rooms[j].Contains(p1))
                        {
                            randAgain = false;
                            roomTypes[j]++;
                        }
                        break;
                    }
                }

                if(randAgain)
                    continue;

                // Create room                
                Rect roomRect = Rect.MinMaxRect(p1.x, p1.y, p2.x, p2.y);
                if(numberOfRooms == 0)
                    roomTypes.Add(0);

                rooms.Add(roomRect);
                numberOfRooms++;
            }

            // create mirror
            for(int i = 0; i < numberOfRooms; ++i)
            {
                Rect roomRect = rooms[i];

                if(mirrorVertical)
                {
                    Vector2Int min = Vector2Int.FloorToInt(roomRect.min);
                    Vector2Int max = Vector2Int.FloorToInt(roomRect.max);

                    min.x = map.w - min.x - 1;
                    max.x = map.w - max.x - 1;

                    p1.x = max.x;

                    min.x = max.x;
                    max.x = p1.x;

                    roomRect = Rect.MinMaxRect(min.x, min.y, max.y, max.y);
                }
                else
                {
                    Vector2Int min = Vector2Int.FloorToInt(roomRect.min);
                    Vector2Int max = Vector2Int.FloorToInt(roomRect.max);

                    min.y = map.h - min.y - 1;
                    max.y = map.h - max.y - 1;

                    p1.y = max.y;

                    min.y = max.y;
                    max.y = p1.y;

                    roomRect = Rect.MinMaxRect(min.x, min.y, max.y, max.y);
                }

                rooms.Insert(i, roomRect);
            }

            for(int i = 0; i < rooms.Count; ++i)
            {
                IEnumerator<Vector2Int> areaIter = Mathnv.GetAreaEnumerator(Vector2Int.FloorToInt(rooms[i].min), Vector2Int.FloorToInt(rooms[i].max));

                while(areaIter.MoveNext())
                {
                    Vector2Int current = areaIter.Current;
                    if(valueMap[current] == valueWall)
                    {
                        valueMap[current] = roomTypes[i];
                    }
                }
            }

            int freeCells = 0;
            for(int x = 0; x < map.MaxValidPosition.x; ++x)
            {
                for(int y = 0; y < map.MaxValidPosition.y/2; ++y)
                {
                    if(valueMap[x,y] != valueMap[x+1,y] && valueMap[x+1,y] != valueWall)
                    {
                        valueMap[x, y] = valueWall;
                    }
                    else if(valueMap[x, y] != valueMap[x, y + 1] && valueMap[x, y + 1] != valueWall)
                    {
                        valueMap[x, y] = valueWall;
                    }
                    else if(valueMap[x, y] != valueMap[x + 1, y + 1] && valueMap[x + 1, y + 1] != valueWall)
                    {
                        valueMap[x, y] = valueWall;
                    }

                    if(valueMap[x, y] != valueWall)
                    {
                        freeCells += 2;// +2 for mirror
                    }

                    valueMap[x, map.h - y - 1] = valueMap[x, y];//mirroring
                }
            }

            if(freeCells < map.h * map.w / 4)
                return false;

            ConvertValuesToTiles(map, valueMap);
            ConnectClosestRooms(map, valueMap, true);

            return true;
        }

        protected void FillDisconnectedRoomsWithDifferentValues(ArrayGrid<MapElement> map, ArrayGrid<int> valueMap, ref int countOfRoomsFilled)
        {
            IEnumerator<Vector2Int> mapIter = IterateOverMap(map);

            while(mapIter.MoveNext())
            {
                Vector2Int current = mapIter.Current;
                if(map[current] == defaultRoomElement)
                {
                    valueMap[current] = valueRoom;
                }
                else if(map[current] == defaultWallElement)
                {
                    valueMap[current] = valueWall;
                }
            }

            mapIter = IterateOverMap(map);
            int roomNumber = 0;
            while(mapIter.MoveNext())
            {
                Vector2Int current = mapIter.Current;
                if(valueMap[current] == valueRoom)
                {
                    valueMap.FloodFill(current, new List<int>() { valueWall }, false, 1 + (roomNumber++));
                }
            }

            countOfRoomsFilled = roomNumber;
        }

        protected void ConnectClosestRooms(ArrayGrid<MapElement> map, ArrayGrid<int> valueMap, bool withDoors, bool straightConnections = false)
        {
            int roomCount = 0;
            FillDisconnectedRoomsWithDifferentValues(map, valueMap, ref roomCount);

            List<List<Vector2Int>> rooms = (new List<Vector2Int>[roomCount]).ToList();

            for(int i = 0; i < rooms.Count; ++i)
            {
                rooms[i] = new List<Vector2Int>();
            }

            IEnumerator<Vector2Int> mapIter = IterateOverMap(map);

            while(mapIter.MoveNext())
            {
                Vector2Int current = mapIter.Current;
                if(valueMap[current] != valueWall)
                {
                    if(valueMap.GetAdjacentElementsOfType(current, false, valueWall).Count > 0)
                    {
                        int roomIndex = valueMap[current] - 1;
                        rooms[roomIndex].Add(current);
                    }
                }
            }

            if(rooms.Count < 2)
                return;

            // for warshall algorithm
            // set the connection matrix

            GameRNG.RandomShuffle(rooms);

            List<List<bool>> roomConnections = (new List<bool>[rooms.Count]).ToList();
            List<List<bool>> transitiveClosure = (new List<bool>[rooms.Count]).ToList(); ;
            List<List<int>> distanceMatrix = (new List<int>[rooms.Count]).ToList();
            List<List<KeyValuePair<Vector2Int, Vector2Int>>> closestCellsMatrix = (new List<KeyValuePair<Vector2Int, Vector2Int>>[rooms.Count]).ToList();

            for(int a = 0; a < rooms.Count; ++a)
            {
                roomConnections[a] = (new bool[rooms.Count]).ToList();
                transitiveClosure[a] = (new bool[rooms.Count]).ToList();
                distanceMatrix[a] = (new int[rooms.Count]).ToList();
                closestCellsMatrix[a] = (new KeyValuePair<Vector2Int, Vector2Int>[rooms.Count]).ToList();

                for(int b = 0; b < rooms.Count; ++b)
                {
                    roomConnections[a][b] = false;
                    distanceMatrix[a][b] = int.MaxValue;
                }
            }

            IEnumerator<Vector2Int> roomAreas = IterateOverArea(rooms.Count, rooms.Count);

            // find the closest cells for each room - Random closest cell
            while(roomAreas.MoveNext())
            {
                Vector2Int ci = roomAreas.Current;
                if(ci.x == ci.y)
                    continue;

                KeyValuePair<Vector2Int, Vector2Int> closestCells = new KeyValuePair<Vector2Int, Vector2Int>();

                foreach(Vector2Int cellA in rooms[ci.y])
                {
                    foreach(Vector2Int cellB in rooms[ci.x])
                    {
                        int distAB = Mathf.CeilToInt((cellA - cellB).magnitude);

                        if((distAB < distanceMatrix[ci.y][ci.x]) || (distAB == distanceMatrix[ci.y][ci.x] && GameRNG.CoinToss()))
                        {
                            closestCells = new KeyValuePair<Vector2Int, Vector2Int>(cellA, cellB);
                            distanceMatrix[ci.y][ci.x] = distAB;
                        }
                    }
                }

                closestCellsMatrix[ci.y][ci.x] = closestCells;
            }


            // Now connect the rooms to the closest ones

            for(int roomA = 0; roomA < rooms.Count; ++roomA)
            {
                int minDist = int.MaxValue;
                int closestRoom = 0;

                for(int roomB = 0; roomB < rooms.Count; ++roomB)
                {
                    if(roomA == roomB)
                        continue;

                    int distance = distanceMatrix[roomA][roomB];
                    if(distance < minDist)
                    {
                        minDist = distance;
                        closestRoom = roomB;
                    }
                }

                // connect roomA to closest one

                KeyValuePair<Vector2Int, Vector2Int> closestCells = closestCellsMatrix[roomA][closestRoom];

                if(!roomConnections[roomA][closestRoom] && AddCorridor(map, valueMap, closestCells, straightConnections))
                {
                    roomConnections[roomA][closestRoom] = true;
                    roomConnections[closestRoom][roomA] = true;
                }
            }

            // The closest rooms connected. Connect the rest until all areas are connected

            for(int toConnectA = 0; toConnectA != -1;)
            {
                roomAreas = IterateOverArea(rooms.Count, rooms.Count);

                while(roomAreas.MoveNext())
                {
                    Vector2Int ci = roomAreas.Current;
                    transitiveClosure[ci.y][ci.x] = roomConnections[ci.y][ci.x];
                }

                roomAreas = IterateOverArea(rooms.Count, rooms.Count);

                while(roomAreas.MoveNext())
                {
                    Vector2Int ci = roomAreas.Current;
                    if(transitiveClosure[ci.y][ci.x] == true && ci.y != ci.x)
                    {
                        for(int ciZ = 0; ciZ < rooms.Count; ++ciZ)
                        {
                            if(transitiveClosure[ci.x][ciZ] == true)
                            {
                                transitiveClosure[ci.y][ciZ] = true;
                                transitiveClosure[ciZ][ci.y] = true;
                            }
                        }
                    }
                }

                toConnectA = -1;

                roomAreas = IterateOverArea(rooms.Count, rooms.Count);

                while(roomAreas.MoveNext() && toConnectA == -1)
                {
                    Vector2Int ci = roomAreas.Current;
                    if(transitiveClosure[ci.y][ci.x] == false && ci.x != ci.y)
                    {
                        toConnectA = ci.y;
                        break;
                    }
                }

                if(toConnectA != -1)
                {
                    int toConnectB = toConnectA;
                    while(toConnectB == toConnectA)
                    {
                        toConnectB = GameRNG.Rand(rooms.Count);
                    }

                    KeyValuePair<Vector2Int, Vector2Int> closestCells = closestCellsMatrix[toConnectA][toConnectB];

                    AddCorridor(map, valueMap, closestCells, straightConnections);
                    {
                        roomConnections[toConnectA][toConnectB] = true;
                        roomConnections[toConnectB][toConnectA] = true;
                    }
                }
            }
        }

        protected bool AddCorridor(ArrayGrid<MapElement> map, ArrayGrid<int> valueMap, KeyValuePair<Vector2Int, Vector2Int> closestCells, bool straightConnections = false)
        {
            if(!map.IsValidPosition(closestCells.Key) || !map.IsValidPosition(closestCells.Value))
                return false;

            // we start from both sides 

            Vector2Int p0 = closestCells.Key;
            Vector2Int p1 = closestCells.Value;
            Vector2Int dir = new Vector2Int(p0.x > p1.x ? -1 : 1, p0.y > p1.y ? -1 : 1);

            bool firstHorizontal = GameRNG.CoinToss();
            bool secondHorizontal = GameRNG.CoinToss();

            for(; ; )
            {
                if(!straightConnections)
                {
                    firstHorizontal = GameRNG.CoinToss();
                    secondHorizontal = GameRNG.CoinToss();
                }

                // connect rooms
                dir.x = p0.x > p1.x ? -1 : 1;
                dir.y = p0.y > p1.y ? -1 : 1;

                if(p0.x != p1.x && p0.y != p1.y)
                {
                    if(firstHorizontal)
                        p0.x += dir.x;
                    else
                        p0.y += dir.y;
                }
                if(p0.x != p1.x && p0.y != p1.y)
                {
                    if(secondHorizontal)
                        p1.x -= dir.x;
                    else
                        p1.y -= dir.y;
                }

                if(valueMap[p0] == valueWall)
                    valueMap[p0] = valueHall;
                if(valueMap[p1] == valueWall)
                    valueMap[p1] = valueHall;

                // connect corridors if on the same level
                if(p0.x == p1.x)
                {
                    dir.y = p0.y > p1.y ? -1 : 1;
                    while(p0.y != p1.y)
                    {
                        p0.y += dir.y;
                        if(valueMap[p0] == valueWall)
                        {
                            valueMap[p0] = valueHall;
                        }
                    }

                    if(valueMap[p0] == valueWall)
                    {
                        valueMap[p0] = valueHall;
                    }

                    return true;
                }

                if(p0.y == p1.y)
                {
                    dir.x = p0.x > p1.x ? -1 : 1;
                    while(p0.x != p1.x)
                    {
                        p0.x += dir.x;
                        if(valueMap[p0] == valueWall)
                        {
                            valueMap[p0] = valueHall;
                        }
                    }

                    if(valueMap[p0] == valueWall)
                    {
                        valueMap[p0] = valueHall;
                    }

                    return true;
                }

            }

            return true;
        }

        protected void ConvertValuesToTiles(ArrayGrid<MapElement> map, ArrayGrid<int> valueMap)
        {
            IEnumerator<Vector2Int> mapIter = IterateOverMap(map);

            while(mapIter.MoveNext())
            {
                Vector2Int current = mapIter.Current;
                if(valueMap[current] == valueHall)
                {
                    map[current] = defaultCorridorElement;
                }
                else if(valueMap[current] == valueWall)
                {
                    map[current] = defaultWallElement;
                }
                else
                {
                    map[current] = defaultRoomElement;
                }
            }
        }
    }
}
