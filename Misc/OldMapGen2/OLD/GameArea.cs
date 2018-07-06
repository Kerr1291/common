using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using nv;

//Implmentation of marching squares taken from
////http://catlikecoding.com/unity/tutorials/marching-squares/

public class GameArea : Generator
{
    public bool debug = false;
    public GameObject debugPrefabA;
    public GameObject debugPrefabB;
    public GameObject debugPrefabC;
    public GameObject debugPrefabD;

    public GameObject voxelPrefab;
    public GameObject root;

    public GameArea xNeighbor, yNeighbor, xyNeighbor;

    public Color32 start;

    public Color32 wall;
    public Color32 floor;

    public float perlinRes = 5.0f;

    //for now, no changing the size
    public float voxelSize = 1;

    Mesh mesh;

    private List<Vector3> vertices;
    private List<int> triangles;

    //Note: potential optimization, make caches static if we're not going to use multitrheading
    int[] rowCacheMax;
    int[] rowCacheMin;

    int edgeCacheMin;
    int edgeCacheMax;

    int Resolution
    {
        get
        {
            return (int)map.Size.x;
        }
    }

    float UnitSize
    {
        get
        {
            return voxelSize;
        }
        set
        {
            voxelSize = value;
        }
    }

    public void Init( int size )
    {
        if( root == null )
            root = gameObject;

        GenerateMap(size);
    }

    public void InitMesh()
    {
        if( debug )
            Debug.Log(gameObject.name);

        GenerateMesh(map.First);
    }

    public override void GenerateMap(int size = 0)
    {
        base.GenerateMap();

        map = Map.EmptyMap;

        if( size > 0 )
        {
            map.Resize(new Vector2(size, size));
        }

        GenerateArea(map.First);

        map.Add();

        GenerateStart(map.First,map.Last);
    }

    public void GenerateStart( MapLayer boundry_layer, MapLayer trigger_layer)
    {
        MapElement start_pos = boundry_layer.GetRandomElementOfType(floor);
        if (start_pos == null)
            return;
        trigger_layer[ start_pos.position ].id = start;
    }

    public void GenerateArea(MapLayer boundry_layer)
    {
        //Rect room_area = new Rect(Vector2.zero, boundry_layer.Size);
        Rect room_area = new Rect(GameRNG.Rand(new Vector2(5, 5), new Vector2(25, 25)), Vector2.one);
        //Rect spot1 = new Rect(RNG.Rand(new Vector2(1, 1), new Vector2(1, 1)), Vector2.one);
        //Rect spot2 = new Rect(RNG.Rand(new Vector2(1, 5), new Vector2(1, 5)), Vector2.one);
        //Rect empty_area = new Rect(RNG.Rand(new Vector2(5,5),new Vector2(25,25)), RNG.Rand(new Vector2(5,5),new Vector2(25,25)));

        //Make sure the room is still on the map
        Dev.Clamp( ref room_area, boundry_layer.ValidArea );

        float res = perlinRes;
        for (int i = 0; i < boundry_layer.Count; ++i)
        {
            MapElement m = boundry_layer[i];
            Vector2 pos = m.pos;
            float p = Mathf.PerlinNoise(transform.localPosition.x/boundry_layer.Size.x*res + pos.x / boundry_layer.Size.x * res, transform.localPosition.z/boundry_layer.Size.y*res + pos.y / boundry_layer.Size.y * res);
            int v = (int)Mathf.Round(p);
                v = (int)Mathf.Clamp01(v);
            if (v == 1)
                m.id = floor;
        }
    }

    [ContextMenu("StartTest")]
    public void StartTest()
    {
        StartCoroutine(TestUpdateArea(map.First));
    }

    [ContextMenu( "StopTest" )]
    public void StopTest()
    {
        StopAllCoroutines();
    }

    IEnumerator TestUpdateArea( MapLayer boundry_layer )
    {
        float res = perlinRes;
        float x = 0;
        while(true)
        {
            x += Time.deltaTime;
            if( x > ( boundry_layer.Size.x * res ) )
                x -= boundry_layer.Size.x * res;

            for( int i = 0; i < boundry_layer.Count; ++i )
            {
                MapElement m = boundry_layer[i];
                //Vector2 pos = m.pos;
                Vector2 pos = m.pos;
                float p = Mathf.PerlinNoise(transform.localPosition.x/boundry_layer.Size.x*res + x + pos.x / boundry_layer.Size.x * res, transform.localPosition.z/boundry_layer.Size.y*res + pos.y / boundry_layer.Size.y * res);
                int v = (int)Mathf.Round(p);
                v = (int)Mathf.Clamp01( v );
                if( v == 1 )
                    m.id = floor;
                else
                    m.id = Color.clear;
            }
            Refresh( boundry_layer );
            yield return new WaitForEndOfFrame();
        }
    }

    public void GeneratePointData(MapLayer mesh_input)
    {
        Vector3 size = Vector3.one * UnitSize;
        voxelPrefab.transform.localScale = size * 0.1f;

        for (int i = 0; i < mesh_input.Count; ++i)
        {
            MapElement m = mesh_input[i];
            if (m.id == Color.clear || m == null)
                continue;

            Vector3 p = Dev.VectorXZ(m.pos, voxelPrefab.transform.localPosition.y);
            CreateVoxel(p);
        }
    }
    private void CreateVoxel(Vector3 voxelPos)
    {
        Vector3 p = new Vector3(voxelPos.x * UnitSize, voxelPos.y, voxelPos.z * UnitSize);
        GameObject g = Instantiate(voxelPrefab, Vector3.zero, voxelPrefab.transform.localRotation, root.transform) as GameObject;
        g.transform.localPosition = p;
    }

    private void CreateDebugPoint(Vector3 voxelPos, float height, int type = 0)
    {
        //nv.Dev.XYtoXZ( ref voxelPos, height );

        Vector3 p = new Vector3(voxelPos.x * UnitSize, voxelPos.y + height, voxelPos.z * UnitSize);
        //Debug.Log(p);

        GameObject prefab = debugPrefabA;
        if( type == 1 )
            prefab = debugPrefabB;
        if( type == 2 )
            prefab = debugPrefabC;
        if( type == 3 )
            prefab = debugPrefabD;

        GameObject g = Instantiate(prefab, Vector3.zero, prefab.transform.localRotation, root.transform) as GameObject;
        g.transform.localPosition = p;
    }

    public void GenerateMesh( MapLayer mesh_input )
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "ChunkMesh";
        vertices = new List<Vector3>();
        triangles = new List<int>();
        
        rowCacheMax = new int[ Resolution * 2 + 1 ];
        rowCacheMin = new int[ Resolution * 2 + 1 ];
        Refresh(mesh_input);

        //GeneratePointData(mesh_input);
    }

    private void CacheFirstCorner( MapElement a )
    {
        if( a != null && !a.Empty )
        {
            rowCacheMax[ 0 ] = vertices.Count;
            vertices.Add( a.wposition );
        }
    }
    private void CacheFirstCornerWithOffset( MapElement a, Vector3 offset )
    {
        if( a != null && !a.Empty )
        {
            rowCacheMax[ 0 ] = vertices.Count;
            vertices.Add( a.wposition + offset );
        }
    }

    private void FillFirstRowCache( MapLayer mesh_input )
    {
        CacheFirstCorner( mesh_input[0,0] );
        int i = 0;
        for( ; i < Resolution - 1; i++ )
        {
            CacheNextEdgeAndCorner( i * 2, mesh_input[ i ], mesh_input[ i + 1 ] );
        }
        if( xNeighbor != null )
        {
            Vector3 offsetx = new Vector3(Resolution,0,0);
            CacheNextEdgeAndCornerWithOffset( i * 2, mesh_input[ i ], xNeighbor.map.First[0], offsetx );
        }
    }

    private void CacheNextEdgeAndCorner( int i, MapElement xMin, MapElement xMax )
    {
        if( !MapElement.CompareID(xMin,xMax) )
        {
            rowCacheMax[ i + 1 ] = vertices.Count;
            vertices.Add( xMin.wxEdgePosition );
        }
        if( xMax != null && !xMax.Empty )
        {
            rowCacheMax[ i + 2 ] = vertices.Count;
            vertices.Add( xMax.wposition );
        }
    }

    private void CacheNextEdgeAndCornerWithOffset( int i, MapElement xMin, MapElement xMax, Vector3 offset )
    {
        if( !MapElement.CompareID( xMin, xMax ) )
        {
            rowCacheMax[ i + 1 ] = vertices.Count;
            vertices.Add( xMin.wxEdgePosition );
        }
        if( xMax != null && !xMax.Empty )
        {
            rowCacheMax[ i + 2 ] = vertices.Count;
            vertices.Add( xMax.wposition + offset );
        }
    }

    private void CacheNextEdgeAndCornerWithOffset( int i, MapElement xMin, MapElement xMax, Vector3 minOffset, Vector3 maxOffset )
    {
        if( !MapElement.CompareID( xMin, xMax ) )
        {
            rowCacheMax[ i + 1 ] = vertices.Count;
            vertices.Add( xMin.wxEdgePosition + minOffset );
        }
        if( xMax != null && !xMax.Empty )
        {
            rowCacheMax[ i + 2 ] = vertices.Count;
            vertices.Add( xMax.wposition + maxOffset );
        }
    }

    private void CacheNextMiddleEdge( MapElement yMin, MapElement yMax )
    {
        edgeCacheMin = edgeCacheMax;
        if( !MapElement.CompareID( yMin, yMax ) )
        {
            edgeCacheMax = vertices.Count;
            vertices.Add( yMin.wyEdgePosition );
        }
    }

    private void CacheNextMiddleEdgeWithOffset( MapElement yMin, MapElement yMax, Vector3 offset )
    {
        edgeCacheMin = edgeCacheMax;
        if( !MapElement.CompareID( yMin, yMax ) )
        {
            edgeCacheMax = vertices.Count;
            vertices.Add( yMin.wyEdgePosition + offset );
        }
    }

    private void Refresh(MapLayer mesh_input)
    {
        Triangulate(mesh_input);
    }

    private void Triangulate(MapLayer mesh_input)
    {
        vertices.Clear();
        triangles.Clear();
        mesh.Clear();

        FillFirstRowCache( mesh_input );

        TriangulateRows(mesh_input);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
    }

    private void TriangulateRows(MapLayer mesh_input)
    {
        for (int j = 0; j < mesh_input.ValidArea.size.y; ++j)
        {
            SwapRowCaches();
            CacheFirstCorner( mesh_input[ 0, j + 1 ] );
            CacheNextMiddleEdge( mesh_input[ 0, j ], mesh_input[ 0, j+1 ] );

            for (int i = 0; i < mesh_input.ValidArea.size.x; ++i)
            {
                int cacheIndex = i * 2;

                CacheNextEdgeAndCorner( cacheIndex, mesh_input[ i, j+1 ], mesh_input[ i+1, j+1 ] );

                CacheNextMiddleEdge( mesh_input[ i + 1, j ], mesh_input[ i + 1, j + 1 ] );

                TriangulateCell( cacheIndex 
                               , mesh_input[i, j]
                               , mesh_input[i + 1, j]
                               , mesh_input[i, j + 1]
                               , mesh_input[i + 1, j + 1] );
            }
            if (xNeighbor != null)
            {
                TriangulateGapCell(mesh_input, j);
            }
        }
        if( yNeighbor != null )
        {
            TriangulateGapRow( mesh_input );
        }
    }

    private void SwapRowCaches()
    {
        int[] rowSwap = rowCacheMin;
        rowCacheMin = rowCacheMax;
        rowCacheMax = rowSwap;
    }

    private void TriangulateGapRow(MapLayer mesh_input)
    {
        Vector3 offset = new Vector3(0,0,Resolution);

        SwapRowCaches();
        MapElement ta = mesh_input[0,(int)mesh_input.ValidArea.size.y];
        MapElement tb = mesh_input[1, (int)mesh_input.ValidArea.size.y];
        MapElement tc = yNeighbor.map.First[0, 0];
        MapElement td = yNeighbor.map.First[1, 0];
        //CacheFirstCornerWithOffset( tc, offset );
        //CacheNextMiddleEdgeWithOffset( mesh_input[ (int)mesh_input.ValidArea.size.y * Resolution ], tc, offset );
        CacheFirstCornerWithOffset( tc, offset );
        CacheNextMiddleEdge( mesh_input[ (int)mesh_input.ValidArea.size.y * Resolution ], tc );

        //CacheFirstCornerWithOffset( yNeighbor.map.First[ 0, 0 ], offset );
        //CacheNextMiddleEdgeWithOffset( yNeighbor.map.First[ 1, 0 ], yNeighbor.map.First[ 0, 0 ], offset );

        for( int i = 0; i < (int)mesh_input.ValidArea.size.y; ++i)
        {
            MapElement a = mesh_input[i,(int)mesh_input.ValidArea.size.y];
            MapElement b = mesh_input[i+1, (int)mesh_input.ValidArea.size.y];
            MapElement c = yNeighbor.map.First[i, 0];
            MapElement d = yNeighbor.map.First[i+1, 0];

            if( debug )
            {
                CreateDebugPoint( a.wpos, .1f, 0 );
                CreateDebugPoint( b.wposition, .2f, 1 );
                CreateDebugPoint( offset + c.wpos, .1f, 2 );
                CreateDebugPoint( offset + d.wposition, .2f, 3 );
            }

            int cacheIndex = i * 2;

            CacheNextEdgeAndCornerWithOffset( cacheIndex, c, d, offset, offset );
            CacheNextMiddleEdge( b, d );
            
            TriangulateCell( cacheIndex, a, b, c, d );
            //TriangulateCellWithOffset( a, b, c, d, Vector3.zero, Vector3.zero, offset, offset);
        }

        if( xyNeighbor != null )
        {
            MapElement ax = mesh_input[(int)mesh_input.ValidArea.size.y,(int)mesh_input.ValidArea.size.y];
            MapElement bx = xNeighbor.map.First[0, (int)mesh_input.ValidArea.size.y];
            MapElement cx = yNeighbor.map.First[(int)mesh_input.ValidArea.size.y, 0];
            MapElement dx = xyNeighbor.map.First[0, 0];
        
            Vector3 offsetx = new Vector3(Resolution,0,0);
            Vector3 offsetz = new Vector3(0,0,Resolution);

            int cacheIndex = ((int)mesh_input.ValidArea.size.y) * 2;

            CacheNextEdgeAndCornerWithOffset( cacheIndex, cx, dx, offsetx + offsetz );
            CacheNextMiddleEdgeWithOffset( bx, dx, offsetx );

            //TriangulateCellWithOffset( ax, bx, cx, dx, Vector3.zero, offsetx, offsetz, offsetx + offsetz );
            TriangulateCell( cacheIndex, ax, bx, cx, dx );
        }
    }

    private void TriangulateGapCell(MapLayer mesh_input, int row)
    {
        MapElement a = mesh_input[(int)mesh_input.ValidArea.size.x, row];
        MapElement b = xNeighbor.map.First[0, row];
        MapElement c = mesh_input[(int)mesh_input.ValidArea.size.x, row+1];
        MapElement d = xNeighbor.map.First[0, row+1];

        Vector3 offset = new Vector3(Resolution,0,0);

        if( debug )
        {
            CreateDebugPoint( a.wpos, .1f, 0 );
            CreateDebugPoint( offset + b.wposition, .1f, 1 );
            CreateDebugPoint( c.wpos, .2f, 2 );
            CreateDebugPoint( offset + d.wposition, .2f, 3 );
        }

        int cacheIndex = ((int)mesh_input.ValidArea.size.y) * 2;

        CacheNextEdgeAndCornerWithOffset( cacheIndex, c, d, offset );
        CacheNextMiddleEdgeWithOffset( b, d, offset );

        TriangulateCell( cacheIndex, a, b, c, d );
        //TriangulateCellWithOffset( a, b, c, d, Vector3.zero, offset, Vector3.zero, offset );
    }

    //If we combine the numbers of filled corners using the binary OR operator, we end up with a number in the 0–15 range.
    //This number tells us which of the sixteen possible types our cell has.
    int GetCellType(MapElement a, MapElement b, MapElement c, MapElement d)
    {
        int cellType = 0;
        if( a != null && !a.Empty )
        {
            cellType |= 1;
        }
        if( b != null && !b.Empty )
        {
            cellType |= 2;
        }
        if( c != null && !c.Empty )
        {
            cellType |= 4;
        }
        if( d != null && !d.Empty )
        {
            cellType |= 8;
        }
        return cellType;
    }

    ////taken from 
    ////http://catlikecoding.com/unity/tutorials/marching-squares/
    //private void TriangulateCell(MapElement a, MapElement b, MapElement c, MapElement d)
    //{
    //    int cellType = GetCellType(a,b,c,d);

    //    switch (cellType)
    //    {
    //        //simple case, no triangle
    //        case 0:
    //            return;

    //        //corner case
    //        case 1:
    //            AddTriangle( (a.wposition),  (a.wyEdgePosition),  (a.wxEdgePosition));
    //            break;
    //        case 2:
    //            AddTriangle( (b.wposition),  (a.wxEdgePosition),  (b.wyEdgePosition));
    //            break;
    //        case 4:
    //            AddTriangle( (c.wposition),  (c.wxEdgePosition),  (a.wyEdgePosition));
    //            break;
    //        case 8:
    //            AddTriangle( (d.wposition),  (b.wyEdgePosition),  (c.wxEdgePosition));
    //            break;

    //        //edge case
    //        case 3:
    //            AddQuad( (a.wposition),  (a.wyEdgePosition),  (b.wyEdgePosition),  (b.wposition));
    //            break;
    //        case 5:
    //            AddQuad( (a.wposition),  (c.wposition),  (c.wxEdgePosition),  (a.wxEdgePosition));
    //            break;
    //        case 10:
    //            AddQuad( (a.wxEdgePosition),  (c.wxEdgePosition),  (d.wposition),  (b.wposition));
    //            break;
    //        case 12:
    //            AddQuad( (a.wyEdgePosition),  (c.wposition),  (d.wposition),  (b.wyEdgePosition));
    //            break;

    //        //The cases with three filled and one empty voxel each require a square with a single corner cut away. 
    //        //This is basically a pentagon that's stretched out of shape. 
    //        //A pentagon can be created with five vertices and three triangles that form a small fan.
    //        case 7:
    //            AddPentagon( (a.wposition),  (c.wposition),  (c.wxEdgePosition),  (b.wyEdgePosition),  (b.wposition));
    //            break;
    //        case 11:
    //            AddPentagon( (b.wposition),  (a.wposition),  (a.wyEdgePosition),  (c.wxEdgePosition),  (d.wposition));
    //            break;
    //        case 13:
    //            AddPentagon( (c.wposition),  (d.wposition),  (b.wyEdgePosition),  (a.wxEdgePosition),  (a.wposition));
    //            break;
    //        case 14:
    //            AddPentagon( (d.wposition),  (b.wposition),  (a.wxEdgePosition),  (a.wyEdgePosition),  (c.wposition));
    //            break;

    //        //Only the two opposite-corner cases are still missing, 6 and 9. 
    //        //I decided to disconnect them, so they require two triangles each, which you can copy from the single-triangle cases. 
    //        //Connecting them would've required a hexagon instead.
    //        case 6:
    //            AddTriangle( (b.wposition),  (a.wxEdgePosition),  (b.wyEdgePosition));
    //            AddTriangle( (c.wposition),  (c.wxEdgePosition),  (a.wyEdgePosition));
    //            break;
    //        case 9:
    //            AddTriangle( (a.wposition),  (a.wyEdgePosition),  (a.wxEdgePosition));
    //            AddTriangle( (d.wposition),  (b.wyEdgePosition),  (c.wxEdgePosition));
    //            break;

    //        //center case
    //        case 15:
    //            AddQuad( (a.wposition),  (c.wposition),  (d.wposition),  (b.wposition));
    //            break;
    //    }
    //}

    //private void TriangulateCellWithOffset( MapElement a, MapElement b, MapElement c, MapElement d, Vector3 offseta, Vector3 offsetb, Vector3 offsetc, Vector3 offsetd )
    //{
    //    int cellType = GetCellType(a,b,c,d);

    //    switch( cellType )
    //    {
    //        //simple case, no triangle
    //        case 0:
    //            return;

    //        //corner case
    //        case 1:
    //            AddTriangle( ( offseta + a.wposition ), ( offseta + a.wyEdgePosition ), ( offseta + a.wxEdgePosition ) );
    //            break;
    //        case 2:
    //            AddTriangle( ( offsetb + b.wposition ), ( offseta + a.wxEdgePosition ), ( offsetb + b.wyEdgePosition ) );
    //            break;
    //        case 4:
    //            AddTriangle( ( offsetc + c.wposition ), ( offsetc + c.wxEdgePosition ), ( offseta + a.wyEdgePosition ) );
    //            break;
    //        case 8:
    //            AddTriangle( ( offsetd + d.wposition ), ( offsetb + b.wyEdgePosition ), ( offsetc + c.wxEdgePosition ) );
    //            break;

    //        //edge case
    //        case 3:
    //            AddQuad( ( offseta + a.wposition ), ( offseta + a.wyEdgePosition ), ( offsetb + b.wyEdgePosition ), ( offsetb + b.wposition ) );
    //            break;
    //        case 5:
    //            AddQuad( ( offseta + a.wposition ), ( offsetc + c.wposition ), ( offsetc + c.wxEdgePosition ), ( offseta + a.wxEdgePosition ) );
    //            break;
    //        case 10:
    //            AddQuad( ( offseta + a.wxEdgePosition ), ( offsetc + c.wxEdgePosition ), ( offsetd + d.wposition ), ( offsetb + b.wposition ) );
    //            break;
    //        case 12:
    //            AddQuad( ( offseta + a.wyEdgePosition ), ( offsetc + c.wposition ), ( offsetd + d.wposition ), ( offsetb + b.wyEdgePosition ) );
    //            break;

    //        //The cases with three filled and one empty voxel each require a square with a single corner cut away. 
    //        //This is basically a pentagon that's stretched out of shape. 
    //        //A pentagon can be created with five vertices and three triangles that form a small fan.
    //        case 7:
    //            AddPentagon( ( offseta + a.wposition ), ( offsetc + c.wposition ), ( offsetc + c.wxEdgePosition ), ( offsetb + b.wyEdgePosition ), ( offsetb + b.wposition ) );
    //            break;
    //        case 11:
    //            AddPentagon( ( offsetb + b.wposition ), ( offseta + a.wposition ), ( offseta + a.wyEdgePosition ), ( offsetc + c.wxEdgePosition ), ( offsetd + d.wposition ) );
    //            break;
    //        case 13:
    //            AddPentagon( ( offsetc + c.wposition ), ( offsetd + d.wposition ), ( offsetb + b.wyEdgePosition ), ( offseta + a.wxEdgePosition ), ( offseta + a.wposition ) );
    //            break;
    //        case 14:
    //            AddPentagon( ( offsetd + d.wposition ), ( offsetb + b.wposition ), ( offseta + a.wxEdgePosition ), ( offseta + a.wyEdgePosition ), ( offsetc + c.wposition ) );
    //            break;

    //        //Only the two opposite-corner cases are still missing, 6 and 9. 
    //        //I decided to disconnect them, so they require two triangles each, which you can copy from the single-triangle cases. 
    //        //Connecting them would've required a hexagon insteaoffsetd + d.
    //        case 6:
    //            AddTriangle( ( offsetb + b.wposition ), ( offseta + a.wxEdgePosition ), ( offsetb + b.wyEdgePosition ) );
    //            AddTriangle( ( offsetc + c.wposition ), ( offsetc + c.wxEdgePosition ), ( offseta + a.wyEdgePosition ) );
    //            break;
    //        case 9:
    //            AddTriangle( ( offseta + a.wposition ), ( offseta + a.wyEdgePosition ), ( offseta + a.wxEdgePosition ) );
    //            AddTriangle( ( offsetd + d.wposition ), ( offsetb + b.wyEdgePosition ), ( offsetc + c.wxEdgePosition ) );
    //            break;

    //        //center case
    //        case 15:
    //            AddQuad( ( offseta + a.wposition ), ( offsetc + c.wposition ), ( offsetd + d.wposition ), ( offsetb + b.wposition ) );
    //            break;
    //    }
    //}   


    //private void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
    //{
    //    int vertexIndex = vertices.Count;
    //    vertices.Add(a);
    //    vertices.Add(b);
    //    vertices.Add(c);
    //    triangles.Add(vertexIndex);
    //    triangles.Add(vertexIndex + 1);
    //    triangles.Add(vertexIndex + 2);
    //}

    //private void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    //{
    //    int vertexIndex = vertices.Count;
    //    vertices.Add(a);
    //    vertices.Add(b);
    //    vertices.Add(c);
    //    vertices.Add(d);
    //    triangles.Add(vertexIndex);
    //    triangles.Add(vertexIndex + 1);
    //    triangles.Add(vertexIndex + 2);
    //    triangles.Add(vertexIndex);
    //    triangles.Add(vertexIndex + 2);
    //    triangles.Add(vertexIndex + 3);
    //}

    //private void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
    //{
    //    int vertexIndex = vertices.Count;
    //    vertices.Add(a);
    //    vertices.Add(b);
    //    vertices.Add(c);
    //    vertices.Add(d);
    //    vertices.Add(e);
    //    triangles.Add(vertexIndex);
    //    triangles.Add(vertexIndex + 1);
    //    triangles.Add(vertexIndex + 2);
    //    triangles.Add(vertexIndex);
    //    triangles.Add(vertexIndex + 2);
    //    triangles.Add(vertexIndex + 3);
    //    triangles.Add(vertexIndex);
    //    triangles.Add(vertexIndex + 3);
    //    triangles.Add(vertexIndex + 4);
    //}

    private void AddTriangle( int a, int b, int c )
    {
        triangles.Add( a );
        triangles.Add( b );
        triangles.Add( c );
    }

    private void AddQuad( int a, int b, int c, int d )
    {
        triangles.Add( a );
        triangles.Add( b );
        triangles.Add( c );
        triangles.Add( a );
        triangles.Add( c );
        triangles.Add( d );
    }

    private void AddPentagon( int a, int b, int c, int d, int e )
    {
        triangles.Add( a );
        triangles.Add( b );
        triangles.Add( c );
        triangles.Add( a );
        triangles.Add( c );
        triangles.Add( d );
        triangles.Add( a );
        triangles.Add( d );
        triangles.Add( e );
    }

    //taken from 
    //http://catlikecoding.com/unity/tutorials/marching-squares/
    private void TriangulateCell( int i, MapElement a, MapElement b, MapElement c, MapElement d )
    {
        int cellType = GetCellType(a,b,c,d);

        switch( cellType )
        {
            //simple case, no triangle
            case 0:
                return;

            //corner case
            case 1:
                AddTriangle( ( rowCacheMin[i] ), ( edgeCacheMin ), ( rowCacheMin[i+1] ) );
                break;
            case 2:
                AddTriangle( ( rowCacheMin[i+2] ), ( rowCacheMin[i+1] ), ( edgeCacheMax ) );
                break;
            case 4:
                AddTriangle( ( rowCacheMax[i] ), ( rowCacheMax[i+1] ), ( edgeCacheMin ) );
                break;
            case 8:
                AddTriangle( ( rowCacheMax[i+2] ), ( edgeCacheMax ), ( rowCacheMax[i+1] ) );
                break;

            //edge case
            case 3:
                AddQuad( ( rowCacheMin[i] ), ( edgeCacheMin ), ( edgeCacheMax ), ( rowCacheMin[i+2] ) );
                break;
            case 5:
                AddQuad( ( rowCacheMin[i] ), ( rowCacheMax[i] ), ( rowCacheMax[i+1] ), ( rowCacheMin[i+1] ) );
                break;
            case 10:
                AddQuad( ( rowCacheMin[i+1] ), ( rowCacheMax[i+1] ), ( rowCacheMax[i+2] ), ( rowCacheMin[i+2] ) );
                break;
            case 12:
                AddQuad( ( edgeCacheMin ), ( rowCacheMax[i] ), ( rowCacheMax[i+2] ), ( edgeCacheMax ) );
                break;

            //The cases with three filled and one empty voxel each require a square with a single corner cut away. 
            //This is basically a pentagon that's stretched out of shape. 
            //A pentagon can be created with five vertices and three triangles that form a small fan.
            case 7:
                AddPentagon( ( rowCacheMin[i] ), ( rowCacheMax[i] ), ( rowCacheMax[i+1] ), ( edgeCacheMax ), ( rowCacheMin[i+2] ) );
                break;
            case 11:
                AddPentagon( ( rowCacheMin[i+2] ), ( rowCacheMin[i] ), ( edgeCacheMin ), ( rowCacheMax[i+1] ), ( rowCacheMax[i+2] ) );
                break;
            case 13:
                AddPentagon( ( rowCacheMax[i] ), ( rowCacheMax[i+2] ), ( edgeCacheMax ), ( rowCacheMin[i+1] ), ( rowCacheMin[i] ) );
                break;
            case 14:
                AddPentagon( ( rowCacheMax[i+2] ), ( rowCacheMin[i+2] ), ( rowCacheMin[i+1] ), ( edgeCacheMin ), ( rowCacheMax[i] ) );
                break;

            //Only the two opposite-corner cases are still missing, 6 and 9. 
            //I decided to disconnect them, so they require two triangles each, which you can copy from the single-triangle cases. 
            //Connecting them would've required a hexagon instead.
            case 6:
                AddTriangle( ( rowCacheMin[i+2] ), ( rowCacheMin[i+1] ), ( edgeCacheMax ) );
                AddTriangle( ( rowCacheMax[i] ), ( rowCacheMax[i+1] ), ( edgeCacheMin ) );
                break;
            case 9:
                AddTriangle( ( rowCacheMin[i] ), ( edgeCacheMin ), ( rowCacheMin[i+1] ) );
                AddTriangle( ( rowCacheMax[i+2] ), ( edgeCacheMax ), ( rowCacheMax[i+1] ) );
                break;

            //center case
            case 15:
                AddQuad( ( rowCacheMin[i] ), ( rowCacheMax[i] ), ( rowCacheMax[i+2] ), ( rowCacheMin[i+2] ) );
                break;
        }
    }

    //private void TriangulateCellWithOffset( int i, MapElement a, MapElement b, MapElement c, MapElement d, Vector3 offseta, Vector3 offsetb, Vector3 offsetc, Vector3 offsetd )
    //{
    //    int cellType = GetCellType(a,b,c,d);

    //    switch( cellType )
    //    {
    //        //simple case, no triangle
    //        case 0:
    //            return;

    //        //corner case
    //        case 1:
    //            AddTriangle( ( offseta + rowCacheMin[i] ), ( offseta + edgeCacheMin ), ( offseta + rowCacheMin[i+1] ) );
    //            break;
    //        case 2:
    //            AddTriangle( ( offsetb + rowCacheMin[i+2] ), ( offseta + rowCacheMin[i+1] ), ( offsetb + edgeCacheMax ) );
    //            break;
    //        case 4:
    //            AddTriangle( ( offsetc + rowCacheMax[i] ), ( offsetc + rowCacheMax[i+1] ), ( offseta + edgeCacheMin ) );
    //            break;
    //        case 8:
    //            AddTriangle( ( offsetd + rowCacheMax[i+2] ), ( offsetb + edgeCacheMax ), ( offsetc + rowCacheMax[i+1] ) );
    //            break;

    //        //edge case
    //        case 3:
    //            AddQuad( ( offseta + rowCacheMin[i] ), ( offseta + edgeCacheMin ), ( offsetb + edgeCacheMax ), ( offsetb + rowCacheMin[i+2] ) );
    //            break;
    //        case 5:
    //            AddQuad( ( offseta + rowCacheMin[i] ), ( offsetc + rowCacheMax[i] ), ( offsetc + rowCacheMax[i+1] ), ( offseta + rowCacheMin[i+1] ) );
    //            break;
    //        case 10:
    //            AddQuad( ( offseta + rowCacheMin[i+1] ), ( offsetc + rowCacheMax[i+1] ), ( offsetd + rowCacheMax[i+2] ), ( offsetb + rowCacheMin[i+2] ) );
    //            break;
    //        case 12:
    //            AddQuad( ( offseta + edgeCacheMin ), ( offsetc + rowCacheMax[i] ), ( offsetd + rowCacheMax[i+2] ), ( offsetb + edgeCacheMax ) );
    //            break;

    //        //The cases with three filled and one empty voxel each require a square with a single corner cut away. 
    //        //This is basically a pentagon that's stretched out of shape. 
    //        //A pentagon can be created with five vertices and three triangles that form a small fan.
    //        case 7:
    //            AddPentagon( ( offseta + rowCacheMin[i] ), ( offsetc + rowCacheMax[i] ), ( offsetc + rowCacheMax[i+1] ), ( offsetb + edgeCacheMax ), ( offsetb + rowCacheMin[i+2] ) );
    //            break;
    //        case 11:
    //            AddPentagon( ( offsetb + rowCacheMin[i+2] ), ( offseta + rowCacheMin[i] ), ( offseta + edgeCacheMin ), ( offsetc + rowCacheMax[i+1] ), ( offsetd + rowCacheMax[i+2] ) );
    //            break;
    //        case 13:
    //            AddPentagon( ( offsetc + rowCacheMax[i] ), ( offsetd + rowCacheMax[i+2] ), ( offsetb + edgeCacheMax ), ( offseta + rowCacheMin[i+1] ), ( offseta + rowCacheMin[i] ) );
    //            break;
    //        case 14:
    //            AddPentagon( ( offsetd + rowCacheMax[i+2] ), ( offsetb + rowCacheMin[i+2] ), ( offseta + rowCacheMin[i+1] ), ( offseta + edgeCacheMin ), ( offsetc + rowCacheMax[i] ) );
    //            break;

    //        //Only the two opposite-corner cases are still missing, 6 and 9. 
    //        //I decided to disconnect them, so they require two triangles each, which you can copy from the single-triangle cases. 
    //        //Connecting them would've required a hexagon insteaoffsetd + d.
    //        case 6:
    //            AddTriangle( ( offsetb + rowCacheMin[i+2] ), ( offseta + rowCacheMin[i+1] ), ( offsetb + edgeCacheMax ) );
    //            AddTriangle( ( offsetc + rowCacheMax[i] ), ( offsetc + rowCacheMax[i+1] ), ( offseta + edgeCacheMin ) );
    //            break;
    //        case 9:
    //            AddTriangle( ( offseta + rowCacheMin[i] ), ( offseta + edgeCacheMin ), ( offseta + rowCacheMin[i+1] ) );
    //            AddTriangle( ( offsetd + rowCacheMax[i+2] ), ( offsetb + edgeCacheMax ), ( offsetc + rowCacheMax[i+1] ) );
    //            break;

    //        //center case
    //        case 15:
    //            AddQuad( ( offseta + rowCacheMin[i] ), ( offsetc + rowCacheMax[i] ), ( offsetd + rowCacheMax[i+2] ), ( offsetb + rowCacheMin[i+2] ) );
    //            break;
    //    }
    //}

    //void Update()
    //{
    //    Debug.DrawLine( transform.position, transform.position + new Vector3( Resolution, 0, 0 ) );
    //    Debug.DrawLine( transform.position, transform.position + new Vector3( 0, 0, Resolution ) );

    //    Debug.DrawLine( transform.position + new Vector3( Resolution, 0, Resolution ), transform.position + new Vector3( Resolution, 0, 0 ) );
    //    Debug.DrawLine( transform.position + new Vector3( Resolution, 0, Resolution ), transform.position + new Vector3( 0, 0, Resolution ) );
    //}
}
