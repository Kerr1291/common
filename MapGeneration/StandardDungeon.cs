using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

namespace nv
{
    public class StandardDungeon : ProcGenMap
    {
        //public int roomNumberOffset = 1000;
        public int maxNumberOfRooms = 10;
        public bool placeDoors = true;

        public Range roomSizeLimitX = new Range(5f, 13f);
        public Range roomSizeLimitY = new Range(5f, 10f);
        
        public int maxGenerationAttempts = 100;
        int generationAttempts = 0;
        int roomCount = 0;

        [EditScriptable]
        public MapElement defaultRoomElement;

        [EditScriptable]
        public MapElement defaultWallElement;

        [EditScriptable]
        public MapElement defaultCorridorElement;

        [EditScriptable]
        public MapElement defaultDoorElement;

        //values used by the room connection algorithm
        int valueRoom = 0;
        int valueWall = -1;
        int valueHall = -2;

        public override ArrayGrid<MapElement> GeneratedMap
        {
            get; protected set;
        }

        public override IEnumerator Generate()
        {
            //cannot generate with nonzero map size
            if(mapSize.sqrMagnitude <= 0)
                yield break;

            generationAttempts = 0;
            roomCount = 0;

            ArrayGrid<MapElement> map = null;
            ArrayGrid<int> valueMap = null;

            Debug.Log("Starting generation");
            for(;;)
            {
                //brief yield before each genration attempt
                yield return null;
                if(generationAttempts > maxGenerationAttempts)
                {
                    Debug.LogError("Failed to generate map with current settings");
                    break;
                }

                map = CreateBaseMap();
                valueMap = new ArrayGrid<int>(mapSize, 0);

                Vector2Int p = Vector2Int.zero;
                Vector2Int roomSize = Vector2Int.zero;
                
                for(int i = 0; i < maxNumberOfRooms; ++i)
                {
                    roomSize.x = roomSizeLimitX.RandomValuei();
                    roomSize.y = roomSizeLimitY.RandomValuei();
                    
                    bool result = map.GetPositionOfRandomAreaOfType(defaultFillElement, roomSize, ref p);
                    if(result)
                    {
                        for(int y = 1; y < roomSize.y-1;++y)
                        {
                            for(int x = 1; x < roomSize.x - 1; ++x)
                            {
                                map[p.x + x, p.y + y] = defaultRoomElement;
                            }
                        }
                    }
                }

                ConnectClosestRooms(map, valueMap, true, true);
                ConvertValuesToTiles(map, valueMap);

                if(placeDoors)
                    AddDoors(map, valueMap);

                //TODO: logic to check if dungeon is valid

                generationAttempts++;
                GeneratedMap = map;

                break;
            }

            Debug.Log("Done");

            yield return WriteTestOutput(map);
            yield return WriteTestOutput(valueMap);

            yield break;
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
            
            List< List<Vector2Int> > rooms = (new List<Vector2Int>[roomCount]).ToList();

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
                        Dev.LogVar(roomIndex);
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
                
                foreach(Vector2Int cellA in rooms[ci.x])
                {
                    foreach(Vector2Int cellB in rooms[ci.y])
                    {
                        int distAB = (int)Vector2Int.Distance(cellA, cellB);

                        if((distAB < distanceMatrix[ci.x][ci.y]) || (distAB == distanceMatrix[ci.x][ci.y] && GameRNG.CoinToss()))
                        {
                            closestCells = new KeyValuePair<Vector2Int, Vector2Int>(cellA, cellB);
                            distanceMatrix[ci.x][ci.y] = distAB;
                        }
                    }
                }

                closestCellsMatrix[ci.x][ci.y] = closestCells;
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

                if(roomConnections[roomA][closestRoom] == true)
                    continue;

                // connect roomA to closest one

                KeyValuePair<Vector2Int, Vector2Int> closestCells = closestCellsMatrix[roomA][closestRoom];

                if(AddCorridor(map, valueMap, closestCells, straightConnections))
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
                    transitiveClosure[ci.x][ci.y] = roomConnections[ci.x][ci.y];
                }

                roomAreas = IterateOverArea(rooms.Count, rooms.Count);

                while(roomAreas.MoveNext())
                {
                    Vector2Int ci = roomAreas.Current;
                    if( transitiveClosure[ci.x][ci.y] == true && ci.x != ci.y)
                    {
                        for( int ciZ = 0; ciZ < rooms.Count; ++ciZ )
                        {
                            if(transitiveClosure[ci.y][ciZ] == true)
                            {
                                transitiveClosure[ci.x][ciZ] = true;
                                transitiveClosure[ciZ][ci.x] = true;
                            }
                        }
                    }
                }

                toConnectA = -1;

                roomAreas = IterateOverArea(rooms.Count, rooms.Count);

                while(roomAreas.MoveNext())
                {
                    Vector2Int ci = roomAreas.Current;
                    if(transitiveClosure[ci.x][ci.y] == false && ci.x != ci.y)
                    {
                        toConnectA = ci.x;
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
            Vector2Int dir = new Vector2Int(p1.x > p0.x ? 1 : -1, p1.y > p0.y ? 1 : -1);

            bool firstHorizontal = GameRNG.CoinToss();
            bool secondHorizontal = GameRNG.CoinToss();

            for(;;)
            {
                if(!straightConnections)
                {
                    firstHorizontal = GameRNG.CoinToss();
                    secondHorizontal = GameRNG.CoinToss();
                }

                // connect rooms

                if(p0 != p1)
                {
                    if(secondHorizontal)
                        p1.x -= dir.x;
                    else
                        p1.y -= dir.y;
                }

                if(valueMap[p0] == valueWall)
                    valueMap[p0] = valueHall;

                //TODO: fix nullref here
                if(valueMap[p1] == valueWall)
                    valueMap[p1] = valueHall;

                // connect corridors if on the same level
                if(p0.x == p1.x)
                {
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

        protected void AddDoors(ArrayGrid<MapElement> map, ArrayGrid<int> valueMap, float doorProbability = 1f)
        {
            IEnumerator<Vector2Int> mapIter = IterateOverMap(map);

            while(mapIter.MoveNext())
            {
                Vector2Int current = mapIter.Current;
                var adjacentElements = map.GetAdjacentElements(current, true);

                int roomCells = adjacentElements.Where(x => x == defaultRoomElement).Count();
                int corridorCells = adjacentElements.Where(x => x == defaultCorridorElement).Count();
                int doorCells = adjacentElements.Where(x => x == defaultDoorElement).Count();

                if(map[current] == defaultCorridorElement)
                {
                    if((corridorCells == 1 && doorCells == 0 && roomCells > 0 && roomCells < 4)
                     ||(corridorCells == 0 && doorCells == 0))
                    {
                        float exist = GameRNG.Randf();
                        if(exist < doorProbability)
                        {
                            map[current] = defaultDoorElement;
                        }
                    }
                }
            }
        }
    }
}
