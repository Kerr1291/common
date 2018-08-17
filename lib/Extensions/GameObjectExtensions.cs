using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace nv
{
    public static class GameObjectExtensions
    {
        public static bool FindAndDestroyGameObjectInChildren( this GameObject gameObject, string name )
        {
            bool found = false;
            GameObject toDestroy = gameObject.FindGameObjectInChildren(name);
            if( toDestroy != null )
            {
                GameObject.Destroy( toDestroy );
                found = true;
            }
            return found;
        }

        public static GameObject FindGameObjectInChildren( this GameObject gameObject, string name )
        {
            if( gameObject == null )
                return null;

            foreach( var t in gameObject.GetComponentsInChildren<Transform>( true ) )
            {
                if( t.name == name )
                    return t.gameObject;
            }
            return null;
        }

        public static GameObject FindGameObjectNameContainsInChildren( this GameObject gameObject, string name )
        {
            if( gameObject == null )
                return null;

            foreach( var t in gameObject.GetComponentsInChildren<Transform>( true ) )
            {
                if( t.name.Contains( name ) )
                    return t.gameObject;
            }
            return null;
        }

        public static string PrintSceneHierarchyPath( this GameObject gameObject )
        {
            if( gameObject == null )
                return "WARNING: NULL GAMEOBJECT";

            string objStr = gameObject.name;

            if( gameObject.transform.parent != null )
                objStr = gameObject.transform.parent.gameObject.PrintSceneHierarchyPath() + "\\" + gameObject.name;

            return objStr;
        }

        public static void PrintSceneHierarchyTree( this GameObject gameObject, bool printComponents = false, System.IO.StreamWriter file = null )
        {
            if( gameObject == null )
                return;

            if( file != null )
            {
                file.WriteLine( "START =====================================================" );
                file.WriteLine( "Printing scene hierarchy for game object: " + gameObject.name );
            }
            else
            {
                Dev.Log( "START =====================================================" );
                Dev.Log( "Printing scene hierarchy for game object: " + gameObject.name );
            }

            foreach( Transform t in gameObject.GetComponentsInChildren<Transform>( true ) )
            {
                string objectNameAndPath = t.gameObject.PrintSceneHierarchyPath();

                if( file != null )
                {
                    file.WriteLine( objectNameAndPath );
                }
                else
                {
                    Dev.Log( objectNameAndPath );
                }


                if( printComponents )
                {
                    string componentHeader = "";
                    for( int i = 0; i < ( objectNameAndPath.Length - t.gameObject.name.Length ); ++i )
                        componentHeader += " ";

                    foreach( Component c in t.GetComponents<Component>() )
                    {
                        c.PrintComponentType( componentHeader, file );
                        c.PrintTransform( componentHeader, file );
                        c.PrintBoxCollider2D( componentHeader, file );
                    }
                }
            }

            if( file != null )
            {
                file.WriteLine( "END +++++++++++++++++++++++++++++++++++++++++++++++++++++++" );
            }
            else
            {
                Dev.Log( "END +++++++++++++++++++++++++++++++++++++++++++++++++++++++" );
            }
        }

        public static T GetOrAddComponent<T>( this GameObject source ) where T : UnityEngine.Component
        {
            T result = source.GetComponent<T>();
            if( result != null )
                return result;
            result = source.AddComponent<T>();
            return result;
        }
    }
}
