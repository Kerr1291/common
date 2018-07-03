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


        ArrayGrid<int> valueMap;

        public override IEnumerator Generate()
        {
            //cannot generate with nonzero map size
            if(mapSize.sqrMagnitude <= 0)
                yield break;

            generationAttempts = 0;
            roomCount = 0;

            ArrayGrid<MapElement> map = null;

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

                Vector2 p = Vector2.zero;
                Vector2 room = Vector2.zero;
                Vector2 roomSize = Vector2.zero;
                
                for(int i = 0; i < maxNumberOfRooms; ++i)
                {
                    roomSize.x = roomSizeLimitX.RandomValue();
                    roomSize.y = roomSizeLimitY.RandomValue();
                    
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

        IEnumerator<Vector2> IterateOverMap<T>(ArrayGrid<T> map)
        {
            var yIter = IterateOver(0, map.h);
            var xIter = IterateOver(0, map.w);

            while(yIter.MoveNext())
            {
                while(xIter.MoveNext())
                {
                    yield return new Vector2(xIter.Current, yIter.Current);
                }
            }
        }

        protected void FillDisconnectedRoomsWithDifferentValues(ArrayGrid<MapElement> map, ref int countOfRoomsFilled)
        {
            IEnumerator<Vector2> mapIter = IterateOverMap(map);

            while(mapIter.MoveNext())
            {
                Vector2 current = mapIter.Current;
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
                Vector2 current = mapIter.Current;
                if(valueMap[current] == defaultRoomElement.value)
                {
                    valueMap.FloodFill(current, new List<int>() { defaultWallElement.value }, false, roomNumber++);
                }
            }

            countOfRoomsFilled = roomNumber;
        }

        protected void ConnectClosestRooms(ArrayGrid<MapElement> map, bool withDoors, bool straightConnection = false)
        {
            int roomCount = 0;
            FillDisconnectedRoomsWithDifferentValues(map, ref roomCount);

            List< List<Vector2> > rooms = (new List<Vector2>[roomCount]).ToList();

            IEnumerator<Vector2> mapIter = IterateOverMap(map);

            while(mapIter.MoveNext())
            {
                Vector2 current = mapIter.Current;
                if(valueMap[current] != defaultWallElement.value)
                {
                    if(valueMap.GetAdjacentElementsOfType(current,false,defaultWallElement.value).Count > 0)
                    {
                        if(rooms[valueMap[current]] == null)
                            rooms[valueMap[current]] = new List<Vector2>();

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
            List<List<KeyValuePair<Vector2, Vector2>>> closestCellsMatrix = (new List<KeyValuePair<Vector2, Vector2>>[rooms.Count]).ToList();

            for(int a = 0; a < rooms.Count; ++a)
            {
                roomConnections[a] = (new bool[rooms.Count]).ToList();
                transitiveClosure[a] = (new bool[rooms.Count]).ToList();
                distanceMatrix[a] = (new int[rooms.Count]).ToList();
                closestCellsMatrix[a] = (new KeyValuePair<Vector2, Vector2>[rooms.Count]).ToList();

                for(int b = 0; b < rooms.Count; ++b)
                {
                    roomConnections[a][b] = false;
                    distanceMatrix[a][b] = int.MaxValue;
                }
            }


            // find the closest cells for each room - Random closest cell
            for(int roomA = 0; roomA < rooms.Count; ++roomA)
            {
                for(int roomB = 0; roomB < rooms.Count; ++roomB)
                {
                    /*
                    if (room_a==room_b)
					continue;
				std::pair < Position, Position > closest_cells;
				for (m=rooms[room_a].begin(),_m=rooms[room_a].end();m!=_m;++m)
				{
					// for each boder cell in room_a try each border cell of room_b
					int x1 = (*m).x;
					int y1 = (*m).y;

					for (k=rooms[room_b].begin(),_k=rooms[room_b].end();k!=_k;++k)
					{
						int x2 = (*k).x;
						int y2 = (*k).y;

						int dist_ab = Distance(x1,y1,x2,y2);
						
						if (dist_ab<distance_matrix[room_a][room_b] || (dist_ab==distance_matrix[room_a][room_b] && CoinToss()))
						{
							closest_cells = std::make_pair( Position(x1,y1), Position(x2,y2) );
							distance_matrix[room_a][room_b] = dist_ab;
						}
					}
				}
				closest_cells_matrix[room_a][room_b] = closest_cells;
                    */
                }
            }


            /*
            
		// Now connect the rooms to the closest ones

		for (int room_a=0;room_a<(int) rooms.size();++room_a)
		{
			int min_distance=INT_MAX;
			int closest_room;
			for (int room_b=0;room_b<(int) rooms.size();++room_b)
			{
				if (room_a==room_b)
					continue;
				int distance = distance_matrix[room_a][room_b];
				if (distance<min_distance)
				{
					min_distance = distance;
					closest_room=room_b;
				}
			}

			// connect room_a to closest one
			std::pair < Position, Position > closest_cells;
			closest_cells = closest_cells_matrix[room_a][closest_room];

			int x1=closest_cells.first.x;
			int y1=closest_cells.first.y;
			int x2=closest_cells.second.x;
			int y2=closest_cells.second.y;

			if (room_connections[room_a][closest_room]==false && AddCorridor(level,x1,y1,x2,y2,straight_connections))
			{
				room_connections[room_a][closest_room]=true;
				room_connections[closest_room][room_a]=true;
			}
		}

		// The closest rooms connected. Connect the rest until all areas are connected


		for(int to_connect_a=0;to_connect_a!=-1;)
		{
			size_t a,b,c;
			int to_connect_b;


			for (a=0;a<rooms.size();a++)
				for (b=0;b<rooms.size();b++)
					transitive_closure[a][b] = room_connections[a][b];

			for (a=0;a<rooms.size();a++)
			{
				for (b=0;b<rooms.size();b++)
				{
					if (transitive_closure[a][b]==true && a!=b)
					{
						for (c=0;c<rooms.size();c++)
						{
							if (transitive_closure[b][c]==true)
							{
								transitive_closure[a][c]=true;
								transitive_closure[c][a]=true;
							}
						}
					}
				}
			}

			// Check if all rooms are connected
			to_connect_a=-1;
			for (a=0;a<rooms.size() && to_connect_a==-1;++a)
			{
				for (b=0;b<rooms.size();b++)
				{
					if (a!=b && transitive_closure[a][b]==false)
					{
						to_connect_a=(int) a;
						break;
					}
				}
			}

			if (to_connect_a!=-1)
			{
				// connect rooms a & b
				do {
					to_connect_b = Random((int) rooms.size());
				} while(to_connect_b==to_connect_a);
				std::pair < Position, Position > closest_cells;
				closest_cells = closest_cells_matrix[to_connect_a][to_connect_b];

				int x1=closest_cells.first.x;
				int y1=closest_cells.first.y;
				int x2=closest_cells.second.x;
				int y2=closest_cells.second.y;

				AddCorridor(level,x1,y1,x2,y2,straight_connections);

				room_connections[to_connect_a][to_connect_b]=true;
				room_connections[to_connect_b][to_connect_a]=true;
			}
		}
            */
        }

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


        /*
        
	inline
	bool AddCorridor(CMap &level, const int& start_x1, const int& start_y1, const int& start_x2, const int& start_y2, bool straight=false)
	{	
		if (!level.OnMap(start_x1,start_y1) || !level.OnMap(start_x2,start_y2))
			return false;
		// we start from both sides 
		int x1,y1,x2,y2;

		x1=start_x1;
		y1=start_y1;
		x2=start_x2;
		y2=start_y2;

		int dir_x;
		int dir_y;

		if (start_x2>start_x1)
			dir_x=1;
		else
			dir_x=-1;

		if (start_y2>start_y1)
			dir_y=1;
		else
			dir_y=-1;


		// move into direction of the other end
		bool first_horizontal=CoinToss();
		bool second_horizontal=CoinToss();

		while(1)
		{
			if (!straight)
			{
				first_horizontal=CoinToss();
				second_horizontal=CoinToss();
			}

			if (x1!=x2 && y1!=y2)
			{
				if (first_horizontal)
					x1+=dir_x;
				else
					y1+=dir_y;
			}
			// connect rooms
			if (x1!=x2 && y1!=y2)
			{
				if (second_horizontal)
					x2-=dir_x;
				else
					y2-=dir_y;
			}

			if (level.GetCell(x1,y1)==LevelElementWall_value)
				level.SetCell(x1,y1,LevelElementCorridor_value);
			if (level.GetCell(x2,y2)==LevelElementWall_value)
				level.SetCell(x2,y2,LevelElementCorridor_value);

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
    }
}
