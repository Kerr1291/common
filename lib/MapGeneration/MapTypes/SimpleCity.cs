using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace nv
{
    public class SimpleCity : ProcGenMap
    {
        public int requiredMinNumberOfBuildings = 5;

        public Range buildingSizeLimitX = new Range(5f, 10f);
        public Range buildingSizeLimitY = new Range(5f, 10f);

        //public Vector2 buildingSizeLimitX = new Vector2(5, 10);
        //public Vector2 buildingSizeLimitY = new Vector2(5, 10);
        public int innerBuildingRoomSize = 3;

        public int maxGenerationAttempts = 100;
        int generationAttempts = 0;
        int buildingCount = 0;

        public override ArrayGrid<MapElement> GeneratedMap
        {
            get; protected set;
        }

        [Range(0f,1f)]
        public float percentageOfGroundToFillWithTrees = .3f;

        [EditScriptable]
        public MapElement defaultRoomElement;

        [EditScriptable]
        public MapElement defaultBuildingWallElement;

        [EditScriptable]
        public MapElement defaultCorridorElement;

        [EditScriptable]
        public MapElement defaultDoorElement;

        [EditScriptable]
        public MapElement defaultTreeElement;

        public override IEnumerator Generate()
        {
            //cannot generate with nonzero map size
            if(mapSize.sqrMagnitude <= 0)
                yield break;

            generationAttempts = 0;
            buildingCount = 0;

            ArrayGrid<MapElement> map;

            Debug.Log("Starting generation");
            for(;;)
            {
                //brief yield before each genration attempt
                yield return null;
                map = CreateBaseMap();

                //get the area to generate over
                Rect main = map.Area;

                //section the area into subsections using roads as separators
                GenerateRoads(map, main);

                bool success = GenerateBuildings(map);

                // plant some trees
                if(success)
                {
                    GenerateTrees(map);
                    break;
                }
            }

            Debug.Log("Done");
            yield return WriteTestOutput(map);

            GeneratedMap = map;

            yield break;
        }

        private void GenerateRoads(ArrayGrid<MapElement> map, Rect main)
        {
            AddRecursiveRooms(map, defaultCorridorElement, new Vector2Int((int)buildingSizeLimitX.Max, (int)buildingSizeLimitY.Max), main, false);
        }

        private void GenerateTrees(ArrayGrid<MapElement> map)
        {
            var openGroundSpaces = map.GetPositionsOfType(defaultFillElement);
            int spacesToFill = (int)(openGroundSpaces.Count * percentageOfGroundToFillWithTrees);
            //Debug.Log("trees = " + spacesToFill);
            int abort = 0;
            for(int i = 0; i < spacesToFill; ++i)
            {
                Vector2Int spotToFill = openGroundSpaces.GetRandomElementFromList();
                //Debug.Log("trying to put tree at " + spotToFill);
                var nearby = map.GetAdjacentElementsOfType(spotToFill, true, defaultBuildingWallElement);

                //TODO: fix this
                if(nearby.Count <= 0)
                {
                    //Debug.Log("Planting tree at " + spotToFill);
                    map[spotToFill] = defaultTreeElement;
                    openGroundSpaces.Remove(spotToFill);
                }
                else
                {
                    --i;
                    abort++;
                }

                if(abort >= 10000)
                    break;
            }
        }

        private bool GenerateBuildings(ArrayGrid<MapElement> map)
        {
            while(buildingCount < requiredMinNumberOfBuildings)
            {
                Vector2Int roomSize = new Vector2Int((int)(buildingSizeLimitX.Max * 2), (int)(buildingSizeLimitY.Max * 2));
                int bufferSize = 2;

                for(;;)
                {
                    Vector2Int newBuildingAreaSize = new Vector2Int(roomSize.x + bufferSize, roomSize.y + bufferSize);
                    Vector2Int buildingPos = Vector2Int.zero;
                    bool result = map.GetPositionOfRandomAreaOfType(defaultFillElement, newBuildingAreaSize, ref buildingPos);
                    if(result)
                    {
                        //Debug.Log("Creating a building at " + buildingPos + " of size " + roomSize);
                        GenerateBuildingRooms(map, buildingPos, roomSize);
                        //map.FillArea(new Rect(buildingPos, roomSize), defaultDoorElement);
                        //map.FillArea(new Rect(buildingPos, Vector2.one), defaultTreeElement);
                        buildingCount++;
                        if(buildingCount >= requiredMinNumberOfBuildings)
                            return true;
                    }
                    else
                    {
                        if(GameRNG.CoinToss())
                        {
                            roomSize.x -= 1;
                        }
                        else
                        {
                            roomSize.y -= 1;
                        }

                        //time to start over
                        if(roomSize.x < buildingSizeLimitX.Min || roomSize.y < buildingSizeLimitY.Min)
                        {
                            Debug.Log("starting over because building size is too small and we don't have enough buildings. " + roomSize + " rooms so far: "+buildingCount);
                            buildingCount = 0;
                            generationAttempts++;
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        void GenerateBuildingRooms(ArrayGrid<MapElement> map, Vector2Int buildingPos, Vector2Int roomSize)
        {
            Rect building = new Rect();
            Rect smaller = new Rect();

            buildingPos.x++;
            buildingPos.y++;

            building.position = buildingPos;
            building.size = roomSize;

            smaller = building;

            smaller.min = smaller.min + Vector2Int.one;
            smaller.max = smaller.max - Vector2Int.one;

            map.FillArea(building, defaultBuildingWallElement);
            map.FillArea(smaller, defaultRoomElement);

            AddRecursiveRooms(map, defaultBuildingWallElement, new Vector2Int(innerBuildingRoomSize, innerBuildingRoomSize), smaller);

            // add a door leading out (improve to lead to nearest road)
            if(GameRNG.CoinToss())
            {
                if(GameRNG.CoinToss())
                {
                    int x = (int)building.min.x + GameRNG.Rand((int)roomSize.x - 2) + 1;
                    int y = (int)building.min.y;

                    map[x, y] = defaultDoorElement;
                }
                else
                {
                    int x = (int)building.min.x + GameRNG.Rand((int)roomSize.x - 2) + 1;
                    int y = (int)building.max.y - 1;

                    map[x, y] = defaultDoorElement;
                }
            }
            else
            {
                if(GameRNG.CoinToss())
                {
                    int x = (int)building.min.x;
                    int y = (int)building.min.y + GameRNG.Rand((int)roomSize.y - 2) + 1;

                    map[x, y] = defaultDoorElement;
                }
                else
                {
                    int x = (int)building.max.x - 1;
                    int y = (int)building.min.y + GameRNG.Rand((int)roomSize.y - 2) + 1;

                    map[x, y] = defaultDoorElement;
                }
            }
        }

        void AddRecursiveRooms(ArrayGrid<MapElement> map, MapElement roomElement, Vector2Int minRoomSize, Rect room, bool withDoors = true)
        {
            int sizeX = (int)room.size.x;
            if(sizeX % 2 != 0)
            {
                sizeX -= GameRNG.CoinToss() ? 1 : 0;
            }

            int sizeY = (int)room.size.y;
            if(sizeY % 2 != 0)
            {
                sizeY -= GameRNG.CoinToss() ? 1 : 0;
            }

            bool splitHorizontal = false;

            if(sizeY * 4 > sizeX)
            {
                splitHorizontal = true;
            }
            if(sizeX * 4 > sizeY)
            {
                splitHorizontal = false;
            }
            else
            {
                splitHorizontal = GameRNG.CoinToss();
            }

            if(splitHorizontal)
            {
                if(sizeY / 2 < minRoomSize.y)
                    return;
                int split = sizeY / 2 + GameRNG.Rand(sizeY / 2 - (int)minRoomSize.y);
                for(int x = (int)room.min.x; x < (int)room.max.x; ++x)
                {
                    map[x, (int)room.min.y + split] = roomElement;
                }

                if(withDoors)
                {
                    map[(int)room.min.x + GameRNG.Rand(sizeX - 1) + 1, (int)room.min.y + split] = defaultDoorElement;
                }

                Rect newRoom = room;
                Vector2Int newMax = newRoom.max.ToInt();
                newMax.y = room.min.ToInt().y + split;
                newRoom.max = newMax;
                AddRecursiveRooms(map, roomElement, minRoomSize, newRoom, withDoors);

                newRoom = room;
                Vector2Int newMin = newRoom.min.ToInt();
                newMin.y = room.min.ToInt().y + split;
                newRoom.min = newMin;
                AddRecursiveRooms(map, roomElement, minRoomSize, newRoom, withDoors);
            }
            else
            {
                if(sizeX / 2 < minRoomSize.x)
                    return;
                int split = sizeX / 2 + GameRNG.Rand(sizeX / 2 - (int)minRoomSize.x);
                for(int y = (int)room.min.y; y < (int)room.max.y; ++y)
                {
                    map[(int)room.min.x + split, y] = roomElement;
                }

                if(withDoors)
                {
                    map[(int)room.min.x + split, (int)room.min.y + GameRNG.Rand(sizeY - 1) + 1] = defaultDoorElement;
                }

                Rect newRoom = room;
                Vector2Int newMax = newRoom.max.ToInt();
                newMax.x = room.min.ToInt().x + split;
                newRoom.max = newMax;
                AddRecursiveRooms(map, roomElement, minRoomSize, newRoom, withDoors);

                newRoom = room;
                Vector2Int newMin = newRoom.min.ToInt();
                newMin.x = room.min.ToInt().x + split;
                newRoom.min = newMin;
                AddRecursiveRooms(map, roomElement, minRoomSize, newRoom, withDoors);
            }
        }
    }
}
