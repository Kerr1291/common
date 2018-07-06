using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using nv;

[Serializable]
public partial class MapLayer
{
    const int k_default_layer_size = 64;
    
    public string name;

    [SerializeField]
    List<MapElement> data;

    [SerializeField]
    Vector2 size;

    public void Clear()
    {
        if( data == null )
            return;
        
        data.Clear();
        data.TrimExcess();
        size = Vector2.zero;
    }

    public bool DataIsValid()
    {
        if( w * h != Count )
        {
            Dev.LogWarning( "Error: size " + ( w * h ) + " != data size " + Count + " for layer " + name + "; Possible data corruption." );
            return false;
        }
        return true;
    }

    public static MapLayer EmptyLayer
    {
        get
        {
            return new MapLayer();
        }
    }

    public MapLayer() 
    { 
        ResizeLayer( k_default_layer_size, k_default_layer_size);
        name = "New Layer";
        InitLayerPositions();
    }

    public MapLayer(int x, int y)
    {
        ResizeLayer( x, y );
        name = "New Layer";
        InitLayerPositions();
    }

    public MapLayer( Vector2 size )
    {
        ResizeLayer( (int)size.x, (int)size.y );
        name = "New Layer";
        InitLayerPositions();
    }

    public List<MapElement> Elements
    {
        get
        {
            return data;
        }
    }

    //calls GetElement at each given position, will place null in the output list if a position is invalid
    public List<MapElement> GetElements( List<Vector2> positions )
    {
        List<MapElement> elements = new List<MapElement>();
        for( int i = 0; i < positions.Count; ++i )
        {
            elements.Add( GetElement( positions[ i ] ) );
        }
        return elements;
    }

    //Direct access, no bounds checking
    public MapElement this[ Vector2 p ]
    {
        get
        {
            return this[(int)p.x, (int)p.y];
        }
        set
        {
            this[ (int)( p.x ), (int)( p.y ) ] = value;
        }
    }

    //Direct access, no bounds checking
    public MapElement this[int x, int y]
    {
        get
        {
            return data[(y * w + x )];
        }
        set
        {
            data[(y * w + x )] = value;
        }
    }

    //Safe access, with bounds checking
    public MapElement GetElement( Vector2 pos )
    {
        return GetElement( (int)( pos.x ), (int)( pos.y ) );
    }

    //Safe access, with bounds checking
    public MapElement GetElement( int x, int y )
    {
        if( !ValidPosition( x, y ) )
            return null;
        return this[ x, y ];
    }

    //Safe access, with bounds checking; Returns reference to the element (if valid)
    public MapElement SetElement( int x, int y, Color id )
    {
        if( ValidPosition( x, y ) == false )
            return null;
        this[ x, y ].id = id;
        return this[ x, y ];
    }

    //Safe access, with bounds checking; Returns reference to the element (if valid)
    public MapElement SetElement( Vector2 pos, Color id )
    {
        return SetElement( (int)pos.x, (int)pos.y, id );
    }

    public void CopyArea( MapLayer source, Rect source_area, Rect dest_area )
    {
        CopyArea( source, source_area, this, dest_area );
    }

    public static void CopyArea( MapLayer source, Rect source_area, MapLayer dest, Rect dest_area )
    {
        Dev.Clamp( ref source_area, source.Area );
        Dev.Clamp( ref dest_area, dest.Area );

        int minX = (int)Mathf.Min(dest_area.width, source_area.width);
        int minY = (int)Mathf.Min(dest_area.height, source_area.height);

        for( int j = 0; j < minY; ++j )
        {
            for( int i = 0; i < minX; ++i )
            {
                dest.SetElement( i + (int)dest_area.x, j + (int)dest_area.y, source[ i + (int)source_area.x, j + (int)source_area.y ].id );
            }
        }
    }

    //This will make the destination area reference the source area
    //Warning: only use this if you know what you're doing!
    public static void ReferenceArea( MapLayer source, Rect source_area, MapLayer dest, Rect dest_area )
    {
        Dev.Clamp( ref source_area, source.Area );
        Dev.Clamp( ref dest_area, dest.Area );

        int minX = (int)Mathf.Min(dest_area.width, source_area.width);
        int minY = (int)Mathf.Min(dest_area.height, source_area.height);

        for( int j = 0; j < minY; ++j )
        {
            for( int i = 0; i < minX; ++i )
            {
                dest[ i + (int)dest_area.x, j + (int)dest_area.y] = source[ i + (int)source_area.x, j + (int)source_area.y ];
            }
        }
    }

    public bool MoveElement( Vector2 from, Vector2 to )
    {
        if( !ValidPosition( from ) )
            return false;
        if( !ValidPosition( to ) )
            return false;

        this[ to ].id = this[ from ].id;
        this[ from ].id = Color.clear;
        return true;
    }

    public bool MoveElementdx( Vector2 from, Vector2 dx )
    {
        Vector2 ddx = from + dx;
        return MoveElement( from, ddx );
    }

    public bool CopyElement( Vector2 from, Vector2 to )
    {
        if( !ValidPosition( from ) )
            return false;
        if( !ValidPosition( to ) )
            return false;

        this[ to ].id = this[ from ].id;
        return true;
    }

    public bool CopyElementdx( Vector2 from, Vector2 dx )
    {
        Vector2 ddx = from + dx;
        return CopyElement( from, ddx );
    }


    //Direct access, no bounds checking
    public MapElement this[ float i ]
    {
        get
        {
            return data[ (int)i ];
        }
        set
        {
            data[ (int)i ] = value;
        }
    }

    //Direct access, no bounds checking
    public MapElement this[ int i ]
    {
        get
        {
            return data[ i ];
        }
        set
        {
            data[ i ] = value;
        }
    }

    public Vector2 Size
    {
        get { return size; }
    }

    //width of map
    public int w
    {
        get
        {
            return (int)( Size.x );
        }
    }

    //height of map
    public int h
    {
        get
        {
            return (int)( Size.y );
        }
    }

    public Rect Area
    {
        get { return new Rect(Vector2.zero, Size); }
    }

    public Rect ValidArea
    {
        get { return new Rect( Area.x, Area.y, Area.width-1, Area.height-1 ); }
    }

    //raw element count
    public int Count
    {
        get
        {
            return data.Count;
        }
    }

    //max valid index
    public Vector2 MaxValidPosition
    {
        get
        {
            Vector2 max = new Vector2(w-1, h-1);
            max.x = Mathf.Max( max.x, 0.0f );
            max.y = Mathf.Max( max.y, 0.0f );
            return max;
        }
    }

    public bool ValidPosition( Vector2 pos )
    {
        return ValidPosition( (int)( pos.x ), (int)( pos.y ) );
    }

    public bool ValidPosition( int x, int y )
    {
        return Dev.Contains(x, y, Vector2.zero, Size);
    }

    //resize the layer. if possible, old data will be preserved
    public void ResizeLayer( int x, int y )
    {
        if( x == w && y == h )
            return;

        //growing the array
        if( x > w || y > h )
        {
            List<MapElement> newArray;
            newArray = new List<MapElement>( x * y );
            for( int j = 0; j < y; ++j )
            {
                for( int i = 0; i < x; ++i )
                {
                    newArray.Add( MapElement.EmptyElement );
                }
            }
            if( data != null )
            {
                int minX = Mathf.Min(w, x);
                int minY = Mathf.Min(h, y);

                for( int j = 0; j < minY; ++j )
                {
                    for( int i = 0; i < minX; ++i )
                    {
                        //newArray[ ( j * x + i ) ].id = data[ ( j * w + i ) ].id;

                        newArray[ ( j * x + i ) ][ 0 ] = data[ ( j * w + i ) ][ 0 ];
                        newArray[ ( j * x + i ) ][ 1 ] = data[ ( j * w + i ) ][ 1 ];
                        newArray[ ( j * x + i ) ][ 2 ] = data[ ( j * w + i ) ][ 2 ];
                        newArray[ ( j * x + i ) ][ 3 ] = data[ ( j * w + i ) ][ 3 ];
                    }
                }
            }
            Clear();

            data = newArray;
        }
        //shrinking the array
        else
        {
            if( data != null )
            {
                int minX = Mathf.Min(w, x);
                int minY = Mathf.Min(h, y);

                for( int j = 0; j < minY; ++j )
                {
                    for( int i = 0; i < minX; ++i )
                    {
                        data[ ( j * x + i ) ][ 0 ] = data[ ( j * w + i ) ][ 0 ];
                        data[ ( j * x + i ) ][ 1 ] = data[ ( j * w + i ) ][ 1 ];
                        data[ ( j * x + i ) ][ 2 ] = data[ ( j * w + i ) ][ 2 ];
                        data[ ( j * x + i ) ][ 3 ] = data[ ( j * w + i ) ][ 3 ];
                    }
                }
                int previous_size = data.Count;
                data.RemoveRange(x * y, previous_size - x * y);
                data.Capacity = x * y;
            }
            else
            {
                data = new List<MapElement>( x * y );
                for (int j = 0; j < y; ++j)
                {
                    for (int i = 0; i < x; ++i)
                    {
                        data.Add(MapElement.EmptyElement);
                    }
                }
            }
        }
        
        size = new Vector2( x, y );
        InitLayerPositions();
    }

    public void ResizeLayer( Vector2 new_size )
    {
        ResizeLayer( (int)new_size.x, (int)new_size.y );
    }

    public void ResizeLayer( Rect new_size )
    {
        ResizeLayer( new_size.size );
    }

    public Texture2D ToTexture( bool empty_view = false )
    {
        Texture2D tex = new Texture2D( (int)Size.x, (int)Size.y, TextureFormat.ARGB32, false, false );
        tex.filterMode = FilterMode.Point;
        for( int j = 0; j < h; ++j )
        {
            for( int i = 0; i < w; ++i )
            {
                if( empty_view )
                {
                    if( !this[ i, j ].Empty )
                    {
                        tex.SetPixel( i, j, Color.red );
                    }
                    else
                    {
                        tex.SetPixel( i, j, Color.clear );
                    }
                }
                else
                {
                    tex.SetPixel( i, j, this[ i, j ].id );
                }
            }
        }
        tex.Apply();
        return tex;
    }
}
