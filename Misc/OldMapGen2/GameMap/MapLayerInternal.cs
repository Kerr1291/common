using UnityEngine;
using System;
using System.Collections.Generic;

using nv;

public partial class MapLayer
{  
    ////Private helpers
    void InitLayerPositions()
    {
        for( int j = 0; j < h; ++j )
        {
            for( int i = 0; i < w; ++i )
            {
                //this[ i, j ].position = new Vector2( i, j );
                this[ i, j ].x = i;
                this[ i, j ].y = j;
                //this[ i, j ].name = name + ": (x:" + i.ToString() + ", y:" + j.ToString() + ")";
            }
        }

        //TODO: disable this
        //VerifyUniqueCells(); 
    }

    //WARNING: Very slow
    void VerifyUniqueCells()
    {
        for( int j = 0; j < h; ++j )
        {
            for( int i = 0; i < w; ++i )
            {
                if( !IsUnique(this[i,j],i,j) )
                {
                    Debug.LogError("Warning: Map has inner references (bad)");
                    return;
                }
            }
        }
    }

    bool IsUnique(MapElement m, int x, int y)
    {
        for( int j = 0; j < h; ++j )
        {
            for( int i = 0; i < w; ++i )
            {
                if( i == x && j == y )
                    continue;

                if( m == this[i,j] )
                {
                    Debug.Log( "Bad reference cell at " + new Vector2(x,y) );
                    Debug.Log( "(other)Bad reference cell at " + new Vector2(i,j) );
                    return false;
                }
            }
        }
        return true;
    }

    static bool ValidRect( Rect r )
    {
        if( r.width <= 0 )
            return false;
        if( r.height <= 0 )
            return false;
        return true;
    }

    ///Helpers for iterating over blocks of data in map areas
    static int IterJStart( Rect r )
    {
        return (int)Dev.RectTopLeft( r ).y;
    }
    static int IterIStart( Rect r )
    {
        return (int)Dev.RectTopLeft( r ).x;
    }
    static int IterJEnd( Rect r )
    {
        return (int)Dev.RectBottomRight( r ).y;
    }
    static int IterIEnd( Rect r )
    {
        return (int)Dev.RectBottomRight( r ).x;
    }


    class FloodFillData
    {
        public List<Vector2> visitedCells = new List<Vector2>();
        public List<Vector2> cellsToFill = new List<Vector2>();
    }

    ///Helper for floodfill
    static void FloodFillCheckAndAdd( MapLayer layer, Vector2 pos, FloodFillData fill_data, List<Color> boundry_mask )
    {
        if( layer.ValidPosition( pos )
            && fill_data.visitedCells.Contains( pos ) == false
            && fill_data.cellsToFill.Contains( pos ) == false )
        {
            bool boundry = layer.GetElement(pos).HasIDInMask(boundry_mask);
            if( !boundry )
                fill_data.cellsToFill.Add( pos );
        }
    }

    static void FloodFillAdd( MapLayer layer, Vector2 pos, FloodFillData fill_data )
    {
        if( layer.ValidPosition( pos )
            && fill_data.visitedCells.Contains( pos ) == false
            && fill_data.cellsToFill.Contains( pos ) == false )
        {
            fill_data.cellsToFill.Add( pos );
        }
    }

    static void FloodFillAddIfType( MapLayer layer, Vector2 pos, FloodFillData fill_data, Color type )
    {
        if( layer.ValidPosition( pos )
            && fill_data.visitedCells.Contains( pos ) == false
            && fill_data.cellsToFill.Contains( pos ) == false )
        {
            if( layer.GetElement( pos ).id == type )
                fill_data.cellsToFill.Add( pos );
        }
    }

    static void FloodFillAddIfNotType( MapLayer layer, Vector2 pos, FloodFillData fill_data, Color type )
    {
        if( layer.ValidPosition( pos )
            && fill_data.visitedCells.Contains( pos ) == false
            && fill_data.cellsToFill.Contains( pos ) == false )
        {
            if( layer.GetElement( pos ).id != type )
                fill_data.cellsToFill.Add( pos );
        }
    }

    static bool IsOnBoundry( Rect r, Vector2 p )
    {
        if( r.x == p.x )
            return true;
        if( r.y == p.y )
            return true;
        if( r.xMax == p.x )
            return true;
        if( r.yMax == p.y )
            return true;
        return false;
    }
}
