using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace nv
{
    public static class ComponentExtensions
    {
        public static void PrintComponentType( this Component c, string componentHeader = "", System.IO.StreamWriter file = null )
        {
            if( c == null )
                return;

            if( file != null )
            {
                file.WriteLine( componentHeader + @" \--Component: " + c.GetType().Name );
            }
            else
            {
                Dev.Log( componentHeader + @" \--Component: " + c.GetType().Name );
            }
        }

        public static void PrintTransform( this Component c, string componentHeader = "", System.IO.StreamWriter file = null )
        {
            if( c as Transform != null )
            {
                if( file != null )
                {
                    file.WriteLine( componentHeader + @" \--GameObject layer: " + ( c as Transform ).gameObject.layer );
                    file.WriteLine( componentHeader + @" \--GameObject tag: " + ( c as Transform ).gameObject.tag );
                    file.WriteLine( componentHeader + @" \--Transform Position: " + ( c as Transform ).position );
                    file.WriteLine( componentHeader + @" \--Transform Rotation: " + ( c as Transform ).rotation.eulerAngles );
                    file.WriteLine( componentHeader + @" \--Transform LocalScale: " + ( c as Transform ).localScale );
                }
                else
                {
                    Dev.Log( componentHeader + @" \--GameObject layer: " + ( c as Transform ).gameObject.layer );
                    Dev.Log( componentHeader + @" \--GameObject tag: " + ( c as Transform ).gameObject.tag );
                    Dev.Log( componentHeader + @" \--Transform Position: " + ( c as Transform ).position );
                    Dev.Log( componentHeader + @" \--Transform Rotation: " + ( c as Transform ).rotation.eulerAngles );
                    Dev.Log( componentHeader + @" \--Transform LocalScale: " + ( c as Transform ).localScale );
                }
            }
        }

        public static void PrintBoxCollider2D( this Component c, string componentHeader = "", System.IO.StreamWriter file = null )
        {
            if( c as BoxCollider2D != null )
            {
                if( file != null )
                {
                    file.WriteLine( componentHeader + @" \--BoxCollider2D Size: " + ( c as BoxCollider2D ).size );
                    file.WriteLine( componentHeader + @" \--BoxCollider2D Offset: " + ( c as BoxCollider2D ).offset );
                    file.WriteLine( componentHeader + @" \--BoxCollider2D Bounds-Min: " + ( c as BoxCollider2D ).bounds.min );
                    file.WriteLine( componentHeader + @" \--BoxCollider2D Bounds-Max: " + ( c as BoxCollider2D ).bounds.max );
                    file.WriteLine( componentHeader + @" \--BoxCollider2D isTrigger: " + ( c as BoxCollider2D ).isTrigger );
                }
                else
                {
                    Dev.Log( componentHeader + @" \--BoxCollider2D Size: " + ( c as BoxCollider2D ).size );
                    Dev.Log( componentHeader + @" \--BoxCollider2D Offset: " + ( c as BoxCollider2D ).offset );
                    Dev.Log( componentHeader + @" \--BoxCollider2D Bounds-Min: " + ( c as BoxCollider2D ).bounds.min );
                    Dev.Log( componentHeader + @" \--BoxCollider2D Bounds-Max: " + ( c as BoxCollider2D ).bounds.max );
                    Dev.Log( componentHeader + @" \--BoxCollider2D isTrigger: " + ( c as BoxCollider2D ).isTrigger );
                }
            }
        }
    }
}
