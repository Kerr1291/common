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
        public Vector2 buildingSizeLimitX = new Vector2(5, 10);
        public Vector2 buildingSizeLimitY = new Vector2(5, 10);
        public int innerBuildingRoomSize = 3;

        public int maxGenerationAttempts = 100;

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
            if(mapSize.sqrMagnitude <= 0)
                yield break;

            ArrayGrid<MapElement> map;

            Debug.Log("Starting generation");
            for(;;)
            {
                yield return null;
                map = new ArrayGrid<MapElement>(mapSize, defaultGroundFillElement);

                Rect main = map.Area;

                AddRecursiveRooms(map, defaultCorridorElement, new Vector2(buildingSizeLimitX.x, buildingSizeLimitY.x), main, false);

                int buildCount = 0;
                int tries = 0;
                while(buildCount < requiredMinNumberOfBuildings)
                {
                    int sizeX = (int)(buildingSizeLimitX.y * 2f);
                    int sizeY = (int)(buildingSizeLimitY.y * 2f);
                    int bufferSize = 2;
                    Vector2 newBuildingAreaSize = new Vector2(sizeX + bufferSize, sizeY + bufferSize);

                    for(;;)
                    {
                        Vector2 buildingPos = Vector2.zero;
                        bool result = map.GetPositionOfRandomAreaOfType(defaultGroundFillElement, newBuildingAreaSize, ref buildingPos);
                        if(result)
                        {
                            Debug.Log("making a room");
                            Rect building = new Rect();
                            Rect smaller = new Rect();

                            buildingPos.x++;
                            buildingPos.y++;

                            building.min = buildingPos;
                            building.max = new Vector2(sizeX, sizeY);

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
                                    int x = (int)building.min.x + GameRNG.Rand(sizeX - 2) + 1;
                                    int y = (int)building.min.y;

                                    map[x, y] = defaultDoorElement;
                                }
                                else
                                {
                                    int x = (int)building.min.x + GameRNG.Rand(sizeX - 2) + 1;
                                    int y = (int)building.max.y - 1;

                                    map[x, y] = defaultDoorElement;
                                }
                            }
                            else
                            {
                                if(GameRNG.CoinToss())
                                {
                                    int x = (int)building.min.x;
                                    int y = (int)building.min.y + GameRNG.Rand(sizeX - 2) + 1;

                                    map[x, y] = defaultDoorElement;
                                }
                                else
                                {
                                    int x = (int)building.max.x - 1;
                                    int y = (int)building.min.y + GameRNG.Rand(sizeX - 2) + 1;

                                    map[x, y] = defaultDoorElement;
                                }
                            }

                            buildCount++;
                            if(buildCount >= requiredMinNumberOfBuildings)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if(GameRNG.CoinToss())
                            {
                                sizeX--;
                            }
                            else
                            {
                                sizeY--;
                            }

                            if(sizeX <= buildingSizeLimitX.x || sizeY <= buildingSizeLimitY.x)
                            {
                                tries++;
                                break;
                            }
                        }
                    }

                    Debug.Log("Buildcount was "+buildCount);
                }

                
                //Debug.Log("Planting trees");
                // plant some trees
                if(tries < maxGenerationAttempts)
                {
                    var openGroundSpaces = map.GetPositionsOfType(defaultGroundFillElement);
                    int spacesToFill = (int)(openGroundSpaces.Count * percentageOfGroundToFillWithTrees);
                    Debug.Log("trees = " + spacesToFill);
                    int abort = 0;
                    for(int i = 0; i < spacesToFill; ++i)
                    {
                        Vector2 spotToFill = openGroundSpaces.GetRandomElementFromList();
                        //Debug.Log("trying to put tree at " + spotToFill);
                        var nearby = map.GetAdjacentElementsOfType(spotToFill, false, defaultBuildingWallElement);

                        //TODO: fix this
                        if(true || nearby.Count <= 0)
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
                    break;
                }
            }

            Debug.Log("Done");

            string testOutputPath = "Assets/common/MapGeneration/";
            Texture2D debugOutput = map.ArrayGridToTexture(WriteColor);
            yield return WriteToFile(debugOutput, testOutputPath + "SimpleCity.png");

            yield break;
        }

        void AddRecursiveRooms(ArrayGrid<MapElement> map, MapElement roomElement, Vector2 minRoomSize, Rect room, bool withDoors = true)
        {
            Dev.Where();
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
                return Color.clear;
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
