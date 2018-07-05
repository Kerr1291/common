using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

namespace nv
{
    public class StandardDungeon : ProcGenMap
    {
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
                Vector2Int room = Vector2Int.zero;
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
                                map[x, y] = defaultRoomElement;
                            }
                        }
                    }
                }

                ConnectClosestRooms(map, valueMap, true, true);

                generationAttempts++;
            }

            yield return WriteTestOutput(map);

            yield break;
        }

        IEnumerator<int> IterateOver(int from, int to)
        {
            for(int i = from; i < to; ++i)
            {
                yield return i;
            }
            yield break;
        }

        IEnumerator<Vector2Int> IterateOverMap<T>(ArrayGrid<T> map)
        {
            var yIter = IterateOver(0, map.h);
            var xIter = IterateOver(0, map.w);

            while(yIter.MoveNext())
            {
                while(xIter.MoveNext())
                {
                    yield return new Vector2Int(xIter.Current, yIter.Current);
                }
            }
        }

        IEnumerator<Vector2Int> IterateOverArea(int w, int h)
        {
            var yIter = IterateOver(0, h);
            var xIter = IterateOver(0, w);

            while(yIter.MoveNext())
            {
                while(xIter.MoveNext())
                {
                    yield return new Vector2Int(xIter.Current, yIter.Current);
                }
            }
        }

        protected void FillDisconnectedRoomsWithDifferentValues(ArrayGrid<MapElement> map, ArrayGrid<int> valueMap, ref int countOfRoomsFilled)
        {
            IEnumerator<Vector2Int> mapIter = IterateOverMap(map);

            while(mapIter.MoveNext())
            {
                Vector2Int current = mapIter.Current;
                if(map[current] == defaultRoomElement)
                {
                    valueMap[current] = defaultRoomElement.value;
                }
                else if(map[current] == defaultWallElement)
                {
                    valueMap[current] = defaultWallElement.value;
                }
            }

            mapIter = IterateOverMap(map);
            int roomNumber = 0;
            while(mapIter.MoveNext())
            {
                Vector2Int current = mapIter.Current;
                if(valueMap[current] == defaultRoomElement.value)
                {
                    valueMap.FloodFill(current, new List<int>() { defaultWallElement.value }, false, roomNumber++);
                }
            }

            countOfRoomsFilled = roomNumber;
        }

        protected void ConnectClosestRooms(ArrayGrid<MapElement> map, ArrayGrid<int> valueMap, bool withDoors, bool straightConnections = false)
        {
            int roomCount = 0;
            FillDisconnectedRoomsWithDifferentValues(map, valueMap, ref roomCount);

            List< List<Vector2Int> > rooms = (new List<Vector2Int>[roomCount]).ToList();

            IEnumerator<Vector2Int> mapIter = IterateOverMap(map);

            while(mapIter.MoveNext())
            {
                Vector2Int current = mapIter.Current;
                if(valueMap[current] != defaultWallElement.value)
                {
                    if(valueMap.GetAdjacentElementsOfType(current, false, defaultWallElement.value).Count > 0)
                    {
                        if(rooms[valueMap[current]] == null)
                            rooms[valueMap[current]] = new List<Vector2Int>();

                        rooms[valueMap[current]].Add(current);
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
                int ciX = (int)ci.x;
                int ciY = (int)ci.y;
                if(ciX == ciY)
                    continue;

                KeyValuePair<Vector2Int, Vector2Int> closestCells = new KeyValuePair<Vector2Int, Vector2Int>();
                
                foreach(Vector2Int cellA in rooms[ciX])
                {
                    foreach(Vector2Int cellB in rooms[ciY])
                    {
                        int distAB = (int)Vector2Int.Distance(cellA, cellB);

                        if((distAB < distanceMatrix[ciX][ciY]) || (distAB == distanceMatrix[ciX][ciY] && GameRNG.CoinToss()))
                        {
                            closestCells = new KeyValuePair<Vector2Int, Vector2Int>(cellA, cellB);
                            distanceMatrix[ciX][ciY] = distAB;
                        }
                    }
                }

                closestCellsMatrix[ciX][ciY] = closestCells;
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

                if(valueMap[p0] == defaultWallElement.value)
                    valueMap[p0] = defaultCorridorElement.value;
                if(valueMap[p1] == defaultWallElement.value)
                    valueMap[p1] = defaultCorridorElement.value;

                // connect corridors if on the same level
                if(p0.x == p1.x)
                {
                    while(p0.y != p1.y)
                    {

                    }
                }
            }

            return false;
        }

        /*
        
	inline
	bool AddCorridor(CMap &level, const int& start_x1, const int& start_y1, const int& start_x2, const int& start_y2, bool straight=false)
	{	
			// connect corridors if on the same level
			if (x1==x2)
			{
				while(y1!=y2)
				{
					y1+=dir_y;
					if (level.GetCell(x1,y1)==LevelElementWall_value)
						level.SetCell(x1,y1,LevelElementCorridor_value);
				}
				if (level.GetCell(x1,y1)==LevelElementWall_value)
					level.SetCell(x1,y1,LevelElementCorridor_value);
				return true;
			}
			if (y1==y2)
			{
				while(x1!=x2)
				{
					x1+=dir_x;
					if (level.GetCell(x1,y1)==LevelElementWall_value)
						level.SetCell(x1,y1,LevelElementCorridor_value);
				}
				if (level.GetCell(x1,y1)==LevelElementWall_value)
					level.SetCell(x1,y1,LevelElementCorridor_value);
				return true;
			}
		}
		return true;
	}
        */

        protected void ConvertValuesToTiles()
        {
            /*
            
		for (unsigned int  y=0;y<level.GetHeight();++y)
		{
			for (unsigned int  x=0;x<level.GetWidth();++x)
			{
				if (level.GetCell(x,y)==LevelElementCorridor_value)
					level.SetCell(x,y,LevelElementCorridor);
				else if (level.GetCell(x,y)==LevelElementWall_value)
					level.SetCell(x,y,LevelElementWall);
				else 
					level.SetCell(x,y,LevelElementRoom);
			}
		}
            */
        }

        protected void AddDoors()
        {
            /*
            
	inline
	void AddDoors(CMap &level, float door_probability, float open_probability)
	{
		for (unsigned int  x=0;x<level.GetWidth();++x)
			for (unsigned int  y=0;y<level.GetHeight();++y)
			{
				Position pos(x,y);
				int room_cells = CountNeighboursOfType(level,LevelElementRoom,pos);
				int corridor_cells = CountNeighboursOfType(level,LevelElementCorridor,pos);
				int open_door_cells = CountNeighboursOfType(level,LevelElementDoorOpen,pos);
				int close_door_cells = CountNeighboursOfType(level,LevelElementDoorClose,pos);
				int door_cells = open_door_cells + close_door_cells;

				if (level.GetCell(x,y)==LevelElementCorridor)
				{
					if ((corridor_cells==1 && door_cells==0 && room_cells>0 && room_cells<4) ||
						(corridor_cells==0 && door_cells==0))
					{
						float exist = ((float) Random(1000))/1000;
						if (exist<door_probability)
						{
							float is_open = ((float) Random(1000))/1000;
							if (is_open<open_probability)
								level.SetCell(x,y,LevelElementDoorOpen);
							else
								level.SetCell(x,y,LevelElementDoorClose);
						}
					}
				} 
			}
	}
            */
        }


    }
}
