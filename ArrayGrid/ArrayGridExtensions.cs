using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace nv
{
    public static class ArrayGridExtensions
    {
        public static Texture2D ToTexture<T>(this ArrayGrid<T> grid, System.Func<T, Color> toColor = null)
        {
            Texture2D tex = new Texture2D((int)grid.w, (int)grid.h, TextureFormat.ARGB32, false, false);
            tex.filterMode = FilterMode.Point;
            for(int j = 0; j < (int)grid.h; ++j)
            {
                for(int i = 0; i < (int)grid.w; ++i)
                {
                    if(toColor == null)
                    {
                        if(grid[i, j] != null)
                        {
                            tex.SetPixel(i, j, Color.red);
                        }
                        else
                        {
                            tex.SetPixel(i, j, Color.black);
                        }
                    }
                    else
                    {
                        tex.SetPixel(i, j, toColor(grid[i, j]));
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        public static List<List<U>> ToData<T, U>(this ArrayGrid<T> grid, System.Func<T, U> toData = null)
        {
            List<List<U>> outData = (new List<U>[grid.h]).ToList();
            for(int i = 0; i < outData.Count; ++i)
            {
                outData[i] = (new U[grid.w]).ToList();
            }

            for(int j = 0; j < (int)grid.h; ++j)
            {
                for(int i = 0; i < (int)grid.w; ++i)
                {
                    if(toData == null)
                    {
                        if(grid[i, j] != null)
                        {
                            outData[i][j] = default(U);
                        }
                        else
                        {
                            //outData[i][j] = default(U);
                        }
                    }
                    else
                    {
                        outData[i][j] = toData(grid[i, j]);
                    }
                }
            }
            return outData;
        }

        public static IEnumerator<Vector2Int> GetEnumerator2D<T>(this ArrayGrid<T> grid)
        {
            IEnumerator<Vector2Int> iterator = Mathnv.GetAreaEnumerator(grid.w, grid.h);
            while(iterator.MoveNext())
                yield return iterator.Current;
        }

        public static ArrayGrid<T> MapToSubGrid<T>(this ArrayGrid<T> grid, Vector2Int sourceAreaPos, Vector2Int sourceAreaSize, Vector2Int subGridSize, bool clampOutOfBounds = true)
        {
            if(!clampOutOfBounds)
            {
                if(!grid.IsValidPosition(sourceAreaPos))
                    return new ArrayGrid<T>();

                if(!grid.IsValidPosition(sourceAreaPos + sourceAreaSize - Vector2Int.one))
                    return new ArrayGrid<T>();
            }
            else
            {
                sourceAreaPos.Clamp(Vector2Int.zero, grid.MaxValidPosition);

                Vector2Int sourceAreaMax = sourceAreaPos + sourceAreaSize;
                sourceAreaMax.Clamp(Vector2Int.zero, grid.Size);

                sourceAreaSize = sourceAreaMax - sourceAreaPos;

                if(sourceAreaSize.x <= 0 || sourceAreaSize.y <= 0)
                    return new ArrayGrid<T>();
            }

            ArrayGrid<T> subGrid = new ArrayGrid<T>(subGridSize);
            
            var subGridIter = subGrid.GetEnumerator2D();

            Vector2 scale = new Vector2((float)sourceAreaSize.x / subGridSize.x, (float)sourceAreaSize.y / subGridSize.y);

            while(subGridIter.MoveNext())
            {
                Vector2Int subCurrent = subGridIter.Current;
                Vector2 scaledSubCurrent = new Vector2(subCurrent.x * scale.x, subCurrent.y * scale.y);
                Vector2Int sourcePos = sourceAreaPos + Vector2Int.FloorToInt(scaledSubCurrent);

                //Debug.Log("==");
                //Debug.Log(subCurrent);
                //Debug.Log(sourcePos);
                //Debug.Log(scaledSubCurrent);
                //Debug.Log(scale);
                //Debug.Log(sourceAreaPos);
                //Debug.Log(sourceAreaSize);
                //Debug.Log(subGridSize);
                subGrid[subCurrent] = grid[sourcePos];
            }

            return subGrid;
        }


        ////TODO: finish/fix this next - not done yet
        //public MapElement GetScaledElement(Vector2Int subMapPosition, Vector2Int subElementPos, float resolution)
        //{
        //    Vector2 scaledPosition = new Vector2(subElementPos.x, subElementPos.y) / resolution;
        //    Vector2Int realMapPosition = subMapPosition + Vector2Int.FloorToInt(scaledPosition);
        //    return GeneratedMap[realMapPosition];
        //}
    }
}
