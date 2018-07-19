using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

namespace nv
{
    public class AntNest : ProcGenMap
    {
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
            //TODO: port algorithm below
            return true;
        }


        /*
         
    
    
		if (level.GetWidth()==0 || level.GetHeight()==0)
			return;

		level.Clear();

		int x,y;

		level.SetCell(level.GetWidth()/2,level.GetHeight()/2,LevelElementCorridor);

		double x1,y1;
		double k;
		double dx, dy;
		int px, py;

		for (int object=0;object<(int) level.GetWidth()*(int) level.GetHeight()/3;++object)
		{
			// degree
			k = Random(360)*3.1419532/180;
			// position on ellipse by degree
			x1 = (double) level.GetWidth()/2+((double)level.GetWidth()/2)*sin(k);	
			y1 = (double) level.GetHeight()/2+((double)level.GetHeight()/2)*cos(k);

			// object will move not too horizontal and not too vertival
			do {
				dx=Random(100);
				dy=Random(100);
			} while ((abs((int) dx)<10 && abs((int) dy)<10));
			dx-=50;
			dy-=50;
			dx/=100;
			dy/=100;

			int counter=0;
			while (1)
			{
				// didn't catch anything after 1000 steps (just to avoid infinite loops)
				if (counter++>1000)
				{
					object--;
					break;
				}
				// move object by small step
				x1+=dx;
				y1+=dy;

				// change float to int

				px=(int) x1;
				py=(int) y1;

				// go through the border to the other side

				if (px<0)
				{
					px=(int) level.GetWidth()-1;
					x1=px;
				}
				if (px>(int) level.GetWidth()-1)
				{
					px=0;
					x1=px;
				}
				if (py<0)
				{
					py=(int) level.GetHeight()-1;
					y1=py;
				}
				if (py>(int) level.GetHeight()-1)
				{
					py=0;
					y1=py;
				}

				// if object has something to catch, then catch it

				if ((px>0 && level.GetCell(px-1,py)==LevelElementCorridor) ||
					(py>0 && level.GetCell(px,py-1)==LevelElementCorridor) ||
					(px<(int) level.GetWidth()-1 && level.GetCell(px+1,py)==LevelElementCorridor) ||
					(py<(int) level.GetHeight()-1 && level.GetCell(px,py+1)==LevelElementCorridor))
				{
					level.SetCell(px,py,LevelElementCorridor);
					break;
				}
			}

		}

		if (with_rooms)
		{
			// add halls at the end of corridors
			for (y=1;y<(int) level.GetHeight()-1;y++)
			{
				for (x=1;x<(int) level.GetWidth()-1;x++)
				{
					if ((x>(int) level.GetWidth()/2-10 && x<(int) level.GetWidth()/2+10 && y>(int) level.GetHeight()/2-5 && y<(int) level.GetHeight()/2+5) || level.GetCell(x,y)==LevelElementWall)
						continue;

					int neighbours=CountNeighboursOfType(level,LevelElementCorridor,Position(x,y));

					if (neighbours==1)
					{
						for (px=-1;px<=1;px++)
							for (py=-1;py<=1;py++)
							{
								level.SetCell(x+px,y+py,LevelElementRoom);
							}
					}
				}		
			}
		} // end of if (with_rooms)
          
         
         */
    }
}
