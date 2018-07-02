using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace nv
{
    public class SimpleCity : ScriptableObject
    {
        public Vector2 mapSize = new Vector2(100,100);

        public int requiredMinNumberOfBuildings = 5;

        public Range buildingSizeLimitX = new Range(5f, 10f);
        public Range buildingSizeLimitY = new Range(5f, 10f);

        //public Vector2 buildingSizeLimitX = new Vector2(5, 10);
        //public Vector2 buildingSizeLimitY = new Vector2(5, 10);
        public int innerBuildingRoomSize = 3;

        public int maxGenerationAttempts = 100;
        int generationAttempts = 0;
        int buildingCount = 0;

        [Range(0f,1f)]
        public float percentageOfGroundToFillWithTrees = .3f;

        [EditScriptable]
        public MapElement defaultGroundFillElement;

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

        IEnumerator WriteToFile(Texture2D tex, string filepath)
        {
            yield return new WaitForEndOfFrame();

            // Encode texture into PNG
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(filepath, bytes);

            yield break;
        }

        public IEnumerator Generate()
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

            string testOutputPath = "Assets/common/MapGeneration/";
            Texture2D debugOutput = map.ArrayGridToTexture(WriteColor);
            yield return WriteToFile(debugOutput, testOutputPath + "SimpleCity.png");

            yield break;
        }

        private void GenerateRoads(ArrayGrid<MapElement> map, Rect main)
        {
            AddRecursiveRooms(map, defaultCorridorElement, new Vector2(buildingSizeLimitX.Max, buildingSizeLimitY.Max), main, false);
        }

        private void GenerateTrees(ArrayGrid<MapElement> map)
        {
            var openGroundSpaces = map.GetPositionsOfType(defaultGroundFillElement);
            int spacesToFill = (int)(openGroundSpaces.Count * percentageOfGroundToFillWithTrees);
            Debug.Log("trees = " + spacesToFill);
            int abort = 0;
            for(int i = 0; i < spacesToFill; ++i)
            {
                Vector2 spotToFill = openGroundSpaces.GetRandomElementFromList();
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
                Vector2 roomSize = new Vector2((buildingSizeLimitX.Max * 2f), (buildingSizeLimitY.Max * 2f));
                int bufferSize = 2;

                for(;;)
                {
                    Vector2 newBuildingAreaSize = new Vector2(roomSize.x + bufferSize, roomSize.y + bufferSize);
                    Vector2 buildingPos = Vector2.zero;
                    bool result = map.GetPositionOfRandomAreaOfType(defaultGroundFillElement, newBuildingAreaSize, ref buildingPos);
                    if(result)
                    {
                        Debug.Log("Creating a building at " + buildingPos + " of size " + roomSize);
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
                            roomSize.x -= 1f;
                        }
                        else
                        {
                            roomSize.y -= 1f;
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

        void GenerateBuildingRooms(ArrayGrid<MapElement> map, Vector2 buildingPos, Vector2 roomSize)
        {
            Rect building = new Rect();
            Rect smaller = new Rect();

            buildingPos.x++;
            buildingPos.y++;

            building.position = buildingPos;
            building.size = roomSize;

            smaller = building;

            smaller.min = smaller.min + Vector2.one;
            smaller.max = smaller.max - Vector2.one;

            map.FillArea(building, defaultBuildingWallElement);
            map.FillArea(smaller, defaultRoomElement);

            AddRecursiveRooms(map, defaultBuildingWallElement, new Vector2(innerBuildingRoomSize, innerBuildingRoomSize), smaller);

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

        private ArrayGrid<MapElement> CreateBaseMap()
        {
            return new ArrayGrid<MapElement>(mapSize, defaultGroundFillElement);
        }

        void AddRecursiveRooms(ArrayGrid<MapElement> map, MapElement roomElement, Vector2 minRoomSize, Rect room, bool withDoors = true)
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
                Vector2 newMax = newRoom.max;
                newMax.y = room.min.y + split;
                newRoom.max = newMax;
                AddRecursiveRooms(map, roomElement, minRoomSize, newRoom, withDoors);

                newRoom = room;
                Vector2 newMin = newRoom.min;
                newMin.y = room.min.y + split;
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
                Vector2 newMax = newRoom.max;
                newMax.x = room.min.x + split;
                newRoom.max = newMax;
                AddRecursiveRooms(map, roomElement, minRoomSize, newRoom, withDoors);

                newRoom = room;
                Vector2 newMin = newRoom.min;
                newMin.x = room.min.x + split;
                newRoom.min = newMin;
                AddRecursiveRooms(map, roomElement, minRoomSize, newRoom, withDoors);
            }
        }

        Color WriteColor(MapElement type)
        {
            return WriteColor(type.type);
        }

        Color WriteColor(int type)
        {
            if(type == 1)
                return Color.green;
            if(type == 2)
                return Color.grey;
            if(type == 3)
                return Color.white;
            if(type == 4)
                return Color.blue;
            if(type == 5)
                return Color.red;
            if(type == 6)
                return Color.yellow;
            if(type == 7)
                return Color.cyan;
            return Color.clear;
        }
    }
}
