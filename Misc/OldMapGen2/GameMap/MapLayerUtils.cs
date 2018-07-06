using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

using nv;

//Main interface for interacting with data in the layer in interesting ways
public partial class MapLayer
{   
    public List<MapElement> GetEmptyElements()
    {  
        return data.Where( x => ( x.id == Color.clear ) ).ToList();
    }

    public List<MapElement> GetNonEmptyElements()
    {
        return data.Where( x => ( x.id != Color.clear ) ).ToList();
    }

    public List<MapElement> GetElementsOfType( Color type )
    {
        return data.Where(x => (x.id == (Color)type) ).ToList();
    }

    public List<MapElement> GetElementsInMask( List<Color> mask )
    {
        return data.Where( x => ( mask.Contains(x.id) ) ).ToList();
    }

    public List<MapElement> GetElementsNotInMask( List<Color> mask )
    {
        return data.Where( x => ( !mask.Contains( x.id ) ) ).ToList();
    }

    public List<MapElement> GetElementsInArea( Rect area )
    {
        Dev.Clamp( ref area, Area );

        if( !ValidRect( area ) )
            return null;

        List<MapElement> elements = new List<MapElement>();

        for( int j = IterJStart( area ); j < IterJEnd( area ); ++j )
        {
            for( int i = IterIStart( area ); i < IterIEnd( area ); ++i )
            {
                if( !ValidPosition( i, j ) )
                    continue;

                elements.Add( this[i,j] );
            }
        }

        return elements;
    }

    public List<MapElement> GetElementsInAreaOfType( Rect area, Color type )
    {
        Dev.Clamp( ref area, Area );

        if( !ValidRect( area ) )
            return null;

        List<MapElement> elements = new List<MapElement>();

        for( int j = IterJStart( area ); j < IterJEnd( area ); ++j )
        {
            for( int i = IterIStart( area ); i < IterIEnd( area ); ++i )
            {
                if( !ValidPosition( i, j ) )
                    continue;


                if( this[ i, j ].id == type )
                    elements.Add( this[ i, j ] );
            }
        }

        return elements;
    }

    public List<MapElement> GetElementsInAreaInMask( Rect area, List<Color> mask )
    {
        Dev.Clamp( ref area, Area );

        if( !ValidRect( area ) )
            return null;

        List<MapElement> elements = new List<MapElement>();

        for( int j = IterJStart( area ); j < IterJEnd( area ); ++j )
        {
            for( int i = IterIStart( area ); i < IterIEnd( area ); ++i )
            {
                if( !ValidPosition( i, j ) )
                    continue;

                if( this[ i, j ].HasIDInMask( mask ) )
                    elements.Add( this[ i, j ] );
            }
        }

        return elements;
    }

    public List<MapElement> GetElementsInAreaNotInMask( Rect area, List<Color> mask )
    {
        Dev.Clamp( ref area, Area );

        if( !ValidRect( area ) )
            return null;

        List<MapElement> elements = new List<MapElement>();

        for( int j = IterJStart( area ); j < IterJEnd( area ); ++j )
        {
            for( int i = IterIStart( area ); i < IterIEnd( area ); ++i )
            {
                if( !ValidPosition( i, j ) )
                    continue;

                if( !this[ i, j ].HasIDInMask( mask ) )
                    elements.Add( this[ i, j ] );
            }
        }

        return elements;
    }

    public List<MapElement> GetElementsInFloodFill( Vector2 start_point, List<Color> boundry_mask, bool search_diagonal )
    {
        if( !ValidPosition(start_point) )
            return null;

        //if we try to select a flood fill that starts on a boundry, abort
        if( this[start_point].HasIDInMask(boundry_mask) )
            return null;

        FloodFillData ffdata = new FloodFillData();

        ffdata.cellsToFill.Add(start_point);

        List<MapElement> elements = new List<MapElement>();

        //fill until we run out
        while( ffdata.cellsToFill.Count > 0 )
        {
            Vector2 p = ffdata.cellsToFill[0];

            Vector2 left = new Vector2(p.x-1,p.y);
            Vector2 right = new Vector2(p.x+1,p.y);
            Vector2 up = new Vector2(p.x,p.y-1);
            Vector2 down = new Vector2(p.x,p.y+1);
            
            FloodFillCheckAndAdd( this, left, ffdata, boundry_mask );
            FloodFillCheckAndAdd( this, right, ffdata, boundry_mask );
            FloodFillCheckAndAdd( this, up, ffdata, boundry_mask );
            FloodFillCheckAndAdd( this, down, ffdata, boundry_mask );

            if( search_diagonal )
            {
                Vector2 tl = new Vector2(p.x-1,p.y-1);
                Vector2 tr = new Vector2(p.x+1,p.y-1);
                Vector2 bl = new Vector2(p.x-1,p.y+1);
                Vector2 br = new Vector2(p.x+1,p.y+1);

                FloodFillCheckAndAdd( this, tl, ffdata, boundry_mask );
                FloodFillCheckAndAdd( this, tr, ffdata, boundry_mask );
                FloodFillCheckAndAdd( this, bl, ffdata, boundry_mask );
                FloodFillCheckAndAdd( this, br, ffdata, boundry_mask );
            }

            ffdata.cellsToFill.Remove( p );
            ffdata.visitedCells.Add( p );

            elements.Add( this[ p ] );
        }

        return elements;
    }

    public void FloodFill( Vector2 start_point, List<Color> boundry_mask, bool search_diagonal, Color fill_value )
    {
        if( !ValidPosition( start_point ) )
            return;

        //if we try to select a flood fill that starts on a boundry, abort
        if( this[ start_point ].HasIDInMask( boundry_mask ) )
            return;

        FloodFillData ffdata = new FloodFillData();

        ffdata.cellsToFill.Add( start_point );

        //fill until we run out
        while( ffdata.cellsToFill.Count > 0 )
        {
            Vector2 p = ffdata.cellsToFill[0];

            Vector2 left = new Vector2(p.x-1,p.y);
            Vector2 right = new Vector2(p.x+1,p.y);
            Vector2 up = new Vector2(p.x,p.y-1);
            Vector2 down = new Vector2(p.x,p.y+1);

            FloodFillCheckAndAdd( this, left, ffdata, boundry_mask );
            FloodFillCheckAndAdd( this, right, ffdata, boundry_mask );
            FloodFillCheckAndAdd( this, up, ffdata, boundry_mask );
            FloodFillCheckAndAdd( this, down, ffdata, boundry_mask );

            if( search_diagonal )
            {
                Vector2 tl = new Vector2(p.x-1,p.y-1);
                Vector2 tr = new Vector2(p.x+1,p.y-1);
                Vector2 bl = new Vector2(p.x-1,p.y+1);
                Vector2 br = new Vector2(p.x+1,p.y+1);

                FloodFillCheckAndAdd( this, tl, ffdata, boundry_mask );
                FloodFillCheckAndAdd( this, tr, ffdata, boundry_mask );
                FloodFillCheckAndAdd( this, bl, ffdata, boundry_mask );
                FloodFillCheckAndAdd( this, br, ffdata, boundry_mask );
            }

            ffdata.cellsToFill.Remove( p );
            ffdata.visitedCells.Add( p );

            SetElement(p,fill_value);
        }
    }

    public void FillArea( Rect area, Color fill_value )
    {
        Dev.Clamp( ref area, Area );

        if( !ValidRect( area ) )
            return;

        for( int j = IterJStart( area ); j < IterJEnd( area ); ++j )
        {
            for( int i = IterIStart( area ); i < IterIEnd( area ); ++i )
            {
                Vector2 p = new Vector2(i,j);

                if( !ValidPosition( p ) )
                    continue;

                SetElement( p, fill_value );
            }
        }
    }

    public void FillEdge( Rect area, Color fill_value )
    {
        Dev.Clamp( ref area, ValidArea );

        if( !ValidRect( area ) )
            return;

        for( int j = IterJStart( area ); j <= IterJEnd( area ); ++j )
        {
            for( int i = IterIStart( area ); i <= IterIEnd( area ); ++i )
            {
                Vector2 p = new Vector2(i,j);
                if( j == IterJStart( area ) )
                {
                    SetElement( p, fill_value );
                    continue;
                }
                if( i == IterIStart( area ) )
                {
                    SetElement( p, fill_value );
                    continue;
                }
                if( j == IterJEnd( area ) )
                {
                    SetElement( p, fill_value );
                    continue;
                }
                if( i == IterIEnd( area ) )
                {
                    SetElement( p, fill_value );
                    continue;
                }



                //if( !ValidPosition( p ) )
                //    continue;

                //if( IsOnBoundry(area, p) == false )
                //    continue;

                //SetElement( p, fill_value );
            }
        }
    }

    public void FillAreaInMask( Rect area, List<Color> mask, Color fill_value )
    {
        Dev.Clamp( ref area, Area );

        if( !ValidRect( area ) )
            return;

        for( int j = IterJStart( area ); j < IterJEnd( area ); ++j )
        {
            for( int i = IterIStart( area ); i < IterIEnd( area ); ++i )
            {
                Vector2 p = new Vector2(i,j);
                if( !ValidPosition( p ) )
                    continue;

                if( this[ p ].HasIDInMask( mask ) )
                    SetElement( p, fill_value );
            }
        }
    }

    public void FillAreaNotInMask( Rect area, List<Color> mask, Color fill_value )
    {
        Dev.Clamp( ref area, Area );

        if( !ValidRect( area ) )
            return;

        for( int j = IterJStart( area ); j < IterJEnd( area ); ++j )
        {
            for( int i = IterIStart( area ); i < IterIEnd( area ); ++i )
            {
                Vector2 p = new Vector2(i,j);
                if( !ValidPosition( p ) )
                    continue;

                if( !this[ p ].HasIDInMask( mask ) )
                    SetElement( p, fill_value );
            }
        }
    }

    public void FillEdgeInMask( Rect area, List<Color> mask, Color fill_value )
    {
        Dev.Clamp( ref area, Area );

        if( !ValidRect( area ) )
            return;

        for( int j = IterJStart( area ); j < IterJEnd( area ); ++j )
        {
            for( int i = IterIStart( area ); i < IterIEnd( area ); ++i )
            {
                Vector2 p = new Vector2(i,j);
                if( !ValidPosition( p ) )
                    continue;

                if( IsOnBoundry( area, p ) == false )
                    continue;

                if( this[ p ].HasIDInMask( mask ) )
                    SetElement( p, fill_value );
            }
        }
    }

    public void FillEdgeNotInMask( Rect area, List<Color> mask, Color fill_value )
    {
        Dev.Clamp( ref area, Area );

        if( !ValidRect( area ) )
            return;

        for( int j = IterJStart( area ); j < IterJEnd( area ); ++j )
        {
            for( int i = IterIStart( area ); i < IterIEnd( area ); ++i )
            {
                Vector2 p = new Vector2(i,j);
                if( !ValidPosition( p ) )
                    continue;

                if( IsOnBoundry( area, p ) == false )
                    continue;

                if( !this[ p ].HasIDInMask( mask ) )
                    SetElement( p, fill_value );
            }
        }
    }

    public MapElement GetRandomElement()
    {
        Vector2 p = GameRNG.Rand(ValidArea);
        return this[p];
    }

    public MapElement GetRandomNonEmptyElement()
    {
        List<MapElement> elements = GetNonEmptyElements();

        if( elements.Count <= 0 )
            return null;

        int i = GameRNG.Rand(elements.Count);

        return elements[ i ];
    }

    //Warning: Expensive call
    public MapElement GetRandomEmptyElement()
    {
        List<MapElement> elements = GetEmptyElements();

        if( elements.Count <= 0 )
            return null;

        int i = GameRNG.Rand(elements.Count);

        return elements[ i ];
    }

    public MapElement GetRandomElementOfType( Color type )
    {
        List<MapElement> elements = GetElementsOfType(type);

        if( elements.Count <= 0 )
            return null;

        int i = GameRNG.Rand(elements.Count);

        return elements[i];
    }

    public MapElement GetRandomElementInArea( Rect area )
    {
        Dev.Clamp(ref area, ValidArea);

        Vector2 p = GameRNG.Rand(area);

        return this[ p ];
    }

    public MapElement GetRandomElementInAreaOfType( Rect area, Color type )
    {
        Dev.Clamp( ref area, Area );

        List<MapElement> elements = GetElementsInAreaOfType(area,type);

        if( elements.Count <= 0 )
            return null;

        int i = GameRNG.Rand(elements.Count);

        return elements[ i ];
    }

    public MapElement GetRandomElementInAreaInMask( Rect area, List<Color> mask )
    {
        Dev.Clamp( ref area, Area );

        List<MapElement> elements = GetElementsInAreaInMask(area,mask);

        if( elements.Count <= 0 )
            return null;

        int i = GameRNG.Rand(elements.Count);

        return elements[ i ];
    }

    public MapElement GetRandomElementInAreaNotInMask( Rect area, List<Color> mask )
    {
        Dev.Clamp( ref area, Area );

        List<MapElement> elements = GetElementsInAreaNotInMask(area,mask);

        if( elements.Count <= 0 )
            return null;

        int i = GameRNG.Rand(elements.Count);

        return elements[ i ];
    }

    public Rect GetRandomArea( Vector2 size )
    {
        Rect area = new Rect(Vector2.zero,Size);

        if( size.x > w )
            return area;
        if( size.y > h )
            return area;

        area = new Rect( Vector2.zero, size );

        Vector2 p = GameRNG.Rand(ValidArea);

        if( p.x + size.x > w )
        {
            p.x += w - ( p.x + size.x );
        }

        if( p.y + size.y > h )
        {
            p.y += h - ( p.y + size.y );
        }

        area.position = p;

        return area;
    }

    public List<MapElement> GetAdjacentElements( Vector2 pos, bool search_diagonal )
    {
        if( !ValidPosition( pos ) )
            return null;

        FloodFillData ffdata = new FloodFillData();

        ffdata.cellsToFill.Add( pos );
        
        Vector2 p = ffdata.cellsToFill[0];

        Vector2 left = new Vector2(p.x-1,p.y);
        Vector2 right = new Vector2(p.x+1,p.y);
        Vector2 up = new Vector2(p.x,p.y-1);
        Vector2 down = new Vector2(p.x,p.y+1);

        FloodFillAdd( this, left, ffdata );
        FloodFillAdd( this, right, ffdata );
        FloodFillAdd( this, up, ffdata );
        FloodFillAdd( this, down, ffdata );

        if( search_diagonal )
        {
            Vector2 tl = new Vector2(p.x-1,p.y-1);
            Vector2 tr = new Vector2(p.x+1,p.y-1);
            Vector2 bl = new Vector2(p.x-1,p.y+1);
            Vector2 br = new Vector2(p.x+1,p.y+1);

            FloodFillAdd( this, tl, ffdata );
            FloodFillAdd( this, tr, ffdata );
            FloodFillAdd( this, bl, ffdata );
            FloodFillAdd( this, br, ffdata );
        }      

        return GetElements(ffdata.cellsToFill);
    }

    public List<MapElement> GetAdjacentElementsOfType( Vector2 pos, bool search_diagonal, Color type )
    {
        if( !ValidPosition( pos ) )
            return null;

        FloodFillData ffdata = new FloodFillData();

        ffdata.cellsToFill.Add( pos );

        Vector2 p = ffdata.cellsToFill[0];

        Vector2 left = new Vector2(p.x-1,p.y);
        Vector2 right = new Vector2(p.x+1,p.y);
        Vector2 up = new Vector2(p.x,p.y-1);
        Vector2 down = new Vector2(p.x,p.y+1);

        FloodFillAddIfType( this, left, ffdata, type );
        FloodFillAddIfType( this, right, ffdata, type );
        FloodFillAddIfType( this, up, ffdata, type );
        FloodFillAddIfType( this, down, ffdata, type );

        if( search_diagonal )
        {
            Vector2 tl = new Vector2(p.x-1,p.y-1);
            Vector2 tr = new Vector2(p.x+1,p.y-1);
            Vector2 bl = new Vector2(p.x-1,p.y+1);
            Vector2 br = new Vector2(p.x+1,p.y+1);

            FloodFillAddIfType( this, tl, ffdata, type );
            FloodFillAddIfType( this, tr, ffdata, type );
            FloodFillAddIfType( this, bl, ffdata, type );
            FloodFillAddIfType( this, br, ffdata, type );
        }

        return GetElements( ffdata.cellsToFill );
    }

    public List<MapElement> GetAdjacentElementsNotOfType( Vector2 pos, bool search_diagonal, Color type )
    {
        if( !ValidPosition( pos ) )
            return null;

        FloodFillData ffdata = new FloodFillData();

        ffdata.cellsToFill.Add( pos );

        Vector2 p = ffdata.cellsToFill[0];

        Vector2 left = new Vector2(p.x-1,p.y);
        Vector2 right = new Vector2(p.x+1,p.y);
        Vector2 up = new Vector2(p.x,p.y-1);
        Vector2 down = new Vector2(p.x,p.y+1);

        FloodFillAddIfNotType( this, left, ffdata, type );
        FloodFillAddIfNotType( this, right, ffdata, type );
        FloodFillAddIfNotType( this, up, ffdata, type );
        FloodFillAddIfNotType( this, down, ffdata, type );

        if( search_diagonal )
        {
            Vector2 tl = new Vector2(p.x-1,p.y-1);
            Vector2 tr = new Vector2(p.x+1,p.y-1);
            Vector2 bl = new Vector2(p.x-1,p.y+1);
            Vector2 br = new Vector2(p.x+1,p.y+1);

            FloodFillAddIfNotType( this, tl, ffdata, type );
            FloodFillAddIfNotType( this, tr, ffdata, type );
            FloodFillAddIfNotType( this, bl, ffdata, type );
            FloodFillAddIfNotType( this, br, ffdata, type );
        }

        return GetElements( ffdata.cellsToFill );
    }

    public List<MapElement> GetAdjacentEmptyElements( Vector2 pos, bool search_diagonal )
    {
        return GetAdjacentElementsOfType(pos, search_diagonal, Color.clear);
    }

    public List<MapElement> GetAdjacentNonEmptyElements( Vector2 pos, bool search_diagonal )
    {
        if( !ValidPosition( pos ) )
            return null;

        Color type = Color.clear;

        FloodFillData ffdata = new FloodFillData();

        ffdata.cellsToFill.Add( pos );

        Vector2 p = ffdata.cellsToFill[0];

        Vector2 left = new Vector2(p.x-1,p.y);
        Vector2 right = new Vector2(p.x+1,p.y);
        Vector2 up = new Vector2(p.x,p.y-1);
        Vector2 down = new Vector2(p.x,p.y+1);

        FloodFillAddIfNotType( this, left, ffdata, type );
        FloodFillAddIfNotType( this, right, ffdata, type );
        FloodFillAddIfNotType( this, up, ffdata, type );
        FloodFillAddIfNotType( this, down, ffdata, type );

        if( search_diagonal )
        {
            Vector2 tl = new Vector2(p.x-1,p.y-1);
            Vector2 tr = new Vector2(p.x+1,p.y-1);
            Vector2 bl = new Vector2(p.x-1,p.y+1);
            Vector2 br = new Vector2(p.x+1,p.y+1);

            FloodFillAddIfNotType( this, tl, ffdata, type );
            FloodFillAddIfNotType( this, tr, ffdata, type );
            FloodFillAddIfNotType( this, bl, ffdata, type );
            FloodFillAddIfNotType( this, br, ffdata, type );
        }

        return GetElements( ffdata.cellsToFill );
    }

    public void SetElements( List<MapElement> elements )
    {
        if( elements == null )
            return;

        for(int i = 0; i < elements.Count; ++i )
        {
            if( elements[ i ] != null )
                SetElement( elements[i].position, elements[ i ].id );
        }
    }

    public static void FillElements( ref List<MapElement> elements, Color type )
    {
        if( elements == null )
            return;

        for( int i = 0; i < elements.Count; ++i )
        {
            if( elements[i] != null )
                elements[ i ].id = type;
        }
    }
}













