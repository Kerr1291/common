using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

namespace nv
{
    public class Caves : ProcGenMap
    {
        public int iterations = 1;

        [Range(0f,1f)]
        public float density = .65f;

        public int maxGenerationAttempts = 100;
        public bool debugOutput = true;
        int generationAttempts = 0;

        public override ArrayGrid<MapElement> GeneratedMap
        {
            get; protected set;
        }

        public MapElement defaultRoomElement
        {
            get
            {
                return defaultFillElement;
            }
        }

        [EditScriptable]
        public MapElement defaultCorridorElement;

        [EditScriptable]
        public MapElement defaultDoorElement;

        [EditScriptable]
        public MapElement defaultWallElement;

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
            // create a game of life cave

            int densityCount = Mathf.FloorToInt(map.Count * density);

            //fill with random walls
            for(int i = 0; i < densityCount; ++i)
            {
                Vector2Int? fillPos = map.GetRandomPositionOfType(defaultFillElement);

                if(fillPos == null || !fillPos.HasValue)
                    return false;

                map[fillPos.Value] = defaultWallElement;
            }

            for(int i = 0; i < iterations; ++i)
            {
                IEnumerator<Vector2Int> mapIter = IterateOverMap(map);

                while(mapIter.MoveNext())
                {
                    Vector2Int current = mapIter.Current;

                    int n = map.GetAdjacentPositionsOfType(current, true, defaultWallElement).Count;

                    if(map[current] == defaultWallElement)
                    {
                        if(n < 4)
                        {
                            map[current] = defaultRoomElement;
                        }
                    }
                    else
                    {
                        if(n > 4)
                        {
                            map[current] = defaultWallElement;
                        }
                    }

                    if(map.IsPositionOnEdge(current))
                        map[current] = defaultWallElement;
                }
            }

            ConnectClosestRooms(map, valueMap, false, false);
            ConvertValuesToTiles(map,valueMap);

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
