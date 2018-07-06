using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Map : ScriptableObject, IEnumerable<MapLayer>
{
    [SerializeField]
    List<MapLayer> layers;

    public Vector2 Size
    {
        get
        {
            return First.Size;
        }
    }

    public void Resize( Vector2 size )
    {
        for(int i = 0; i < Count; ++i )
        {
            layers[i].ResizeLayer(size);
        }
    }

    public void Clear()
    {
        for(int i = 0; i < layers.Count; ++i )
            layers[i].Clear();

        layers.Clear();
        layers.TrimExcess();
    }

    public IEnumerator<MapLayer> GetEnumerator()
    {
        return layers.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    static public void UnloadMap( ref Map map )
    { 
        if( map == null )
            return;
        
        map.Clear();

        if( Application.isPlaying == false )
            DestroyImmediate( map );
        else
            Destroy( map );

        map = null;
    }

    //Create new map layer of the same size as the last layer
    public void Add()
    {
        layers.Add( new MapLayer(Last.Size) );
    }

    public void Add( MapLayer new_layer )
    {
        layers.Add( new_layer );
    }

    public void Remove( MapLayer layer )
    {
        layers.Remove( layer );
    }

    public int Count
    {
        get
        {
            return layers.Count;
        }
    }

    public static Map EmptyMap
    {
        get
        {
            return CreateInstance<Map>();
        }
    }

    void OnEnable()
    {
        if( name == string.Empty )
            name = "New Map";

        if( layers == null )
        {
            layers = new List<MapLayer>();
            layers.Add(MapLayer.EmptyLayer);
        }

        List<MapLayer> remove_layers = new List<MapLayer>();

        for( int i = 0; i < Count; ++i )
        {
            if( this[i].DataIsValid() == false )
            {
                DLog.Log("Removing bad layer "+this[i].name);
                remove_layers.Add(this[i]);
            }
        }

        for( int i = 0; i < remove_layers.Count; ++i )
        {
            Remove(remove_layers[i]);
        }

        hideFlags = HideFlags.HideAndDontSave;
    }
    
    public MapLayer First
    {
        get
        {
            return layers[ 0 ];
        }
    }

    public MapLayer Last
    {
        get
        {
            return layers[ layers.Count-1 ];
        }
    }

    public MapLayer this[int index]
    {
        get
        {
            return layers[index];
        }
        set
        {
            layers.Insert(index, value);
        }
    }

    public int LayerNotFound
    {
        get
        {
            return -1;
        }
    }

    public int FirstIndex
    {
        get
        {
            return 0;
        }
    }

    public int LastIndex
    {
        get
        {
            return layers.Count - 1;
        }
    }

    public int GetLayerIndex(MapLayer layer)
    {
        for( int i = 0; i < layers.Count; ++i )
        {
            if( layers[ i ] == layer )
                return i;
        }
        return LayerNotFound;
    }

    public MapLayer GetNextLayer(MapLayer current)
    {
        for( int i = 0; i < layers.Count; ++i )
        {
            if( layers[ i ] == current )
            {
                if( (i+1) >= layers.Count )
                    return layers[ i ];
                else
                    return layers[ i+1 ];
            }
        }
        return null;
    }

    public MapLayer GetPrevLayer( MapLayer current )
    {
        for( int i = 0; i < layers.Count; ++i )
        {
            if( layers[ i ] == current )
            {
                if( ( i - 1 ) < 0 )
                    return layers[ i ];
                else
                    return layers[ i - 1 ];
            }
        }
        return null;
    }
}


