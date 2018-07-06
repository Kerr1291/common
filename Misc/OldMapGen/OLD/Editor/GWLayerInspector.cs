using UnityEngine;
using UnityEditor;
using System.Collections;

public partial class GeneratorWindow
{ 
    [SerializeField]
    MapLayer currentLayer;

    [SerializeField]
    Texture2D currentLayerTexture;

    Vector2 renderLayerMapScrollPos;

    int mapViewScale = 1;

    bool emptyView = false;

    void GenerateCurrentLayerTexture()
    {
        if( currentLayerTexture != null )
        {
            if( Application.isPlaying == false )
                DestroyImmediate( currentLayerTexture );
            else
                Destroy( currentLayerTexture );
        }

        currentLayerTexture = currentLayer.ToTexture( emptyView );

        //GenerateCachedData();
    }

    void Field_LayerSize()
    {
        Vector2 newLayerSize = currentLayer.Size;

        EditorUtils.DelayedVector2Field( "Layer Size", ref newLayerSize );        
        Utils.ClampToInt(ref newLayerSize);

        if( newLayerSize.x != currentLayer.w || currentLayer.h != newLayerSize.y )
        {
            currentLayer.ResizeLayer( (int)( newLayerSize.x ), (int)( newLayerSize.y ) );
            GenerateCurrentLayerTexture();
        }
    }

    void Field_LayerZoom()
    {
        mapViewScale = EditorGUILayout.IntField( "Map View Scale", mapViewScale );
        mapViewScale = Mathf.Max( mapViewScale, 1 );
    }

    void Field_EmptyView()
    {
        bool prev = emptyView;
        emptyView = EditorGUILayout.Toggle( "Non-Empty View Mode", emptyView );
        if( prev != emptyView )
            GenerateCurrentLayerTexture();
    }    

    void DrawLayerWindow()
    {
        Field_LayerSize();
        Field_EmptyView();
        Field_LayerZoom();

        EditorGUILayout.BeginHorizontal();
        DrawSettingsWindow();
        RenderLayerToEditor();
        EditorGUILayout.EndHorizontal();
    }

    Rect GetMapLayerRect()
    {
        int texture_scale = mapViewScale;
        Rect crect = EditorGUILayout.GetControlRect();
        crect.size = currentLayer.Size * texture_scale;
        return crect;
    }

    void RenderLayerToEditor()
    {
        EditorGUILayout.BeginVertical();
        renderLayerMapScrollPos = EditorGUILayout.BeginScrollView( renderLayerMapScrollPos, true, true );
        {
            Rect crect = GetMapLayerRect();
            if( currentLayerTexture == null )
                GenerateCurrentLayerTexture();            

            //hack to force the area inside the scrollview to expand to the appropriate size: http://answers.unity3d.com/questions/1076151/guilayoutbeginarea-doesnt-expand-scrollview.html
            //allows expandable 2d scroll area with GUILayout editor functions
            GUILayout.Label( "", GUILayout.Width( crect.width ), GUILayout.Height( crect.height ) );
            {
                SelectMapCell( crect, 1 );
                EditorGUI.DrawTextureTransparent( crect, currentLayerTexture, ScaleMode.StretchToFill );
            }
        }   
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    void SelectMapCell( Rect map_pos, int scale )
    {
        Vector2 texPos = map_pos.position;
        Vector2 mousePos = Event.current.mousePosition - texPos * 2;

        if( mousePos.x < 0 )
            return;
        if( mousePos.y < 0 )
            return;

        if( mousePos.x > map_pos.width )
            return;
        if( mousePos.y > map_pos.height )
            return;

        mousePos.x = Mathf.Clamp( mousePos.x, 0.0f, map_pos.width - 1.0f );
        mousePos.y = Mathf.Clamp( mousePos.y, 0.0f, map_pos.height - 1.0f );

        int x = (int)(mousePos.x);
        int y = (int)(mousePos.y);

        if( x >= currentLayer.w )
            return;

        if( y >= currentLayer.h )
            return;

        y = currentLayer.h - y;

        if( Event.current.type == EventType.MouseDown && Event.current.button == 0 )
        {
            //Color id_color = Color.red;

            if( x >= currentLayer.w )
                return;

            if( y >= currentLayer.h )
                return;

            //Debug.Log("drawing test");
            //Rect test = new Rect(new Vector2(x,y), Vector2.one * 25);

            //Utils.Clamp(ref test, currentLayer.ValidArea);

            //currentLayer.FillArea( test, id_color );
            //GenerateCurrentLayerTexture();

            //currentLayer[ x, y ].id = id_color;
            //for( int j = (int)Utils.RectTopLeft( test ).y; j < (int)Utils.RectBottomRight( test ).y; ++j )
            //{
            //    for( int i = (int)Utils.RectTopLeft( test ).x; i < (int)Utils.RectBottomRight( test ).x; ++i )
            //    {
            //        currentLayerTexture.SetPixel( i, j, currentLayer[i,j].id );
            //    }
            //}
            //currentLayerTexture.SetPixel( x, y, id_color );
            //currentLayerTexture.Apply();
            Repaint();
        }
    }
}
