using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace nv
{
    public class NVCallback : Attribute
    {
    }
    
    [System.Serializable]
    public class CommunicationNode
    {
        protected static CommunicationNode root;
        protected CommunicationNode next;
        protected CommunicationNode prev;

        protected static Action<object,object> invokeAction;

        protected readonly Dictionary<Type, MethodInfo> enabledCallbacks = new Dictionary<Type, MethodInfo>();
        protected List<SerializableMethodInfo> enabledMethodInfos = new List<SerializableMethodInfo>();

        public virtual object Subject { get; private set; }

        public static void Invoke( object data, object subject )
        {
            invokeAction.Invoke( data, subject );
        }

        public virtual void Invoke( object data )
        {
            Invoke( data, Subject );
        }

        public virtual void Register( object subject )
        {
            if( subject == null )
            {
                UnRegister();
                return;
            }

            this.Subject = subject;

            CommunicationNode.Add( this );
            RefreshCallbackBindings();
        }

        public virtual void UnRegister()
        {
            if( Subject != null )
            {
                CommunicationNode.Remove( this );
                Subject = null;
            }
        }

        static CommunicationNode()
        {
            InvokeAction -= DefaultInvoke;
            InvokeAction += DefaultInvoke;
        }

        protected static void Add( CommunicationNode node )
        {
            //if the node has non-null connections, clear them by removing it before we process the insertion
            if( node.next != null || node.prev != null )
            {
                Remove( node );
            }

            if( root == null )
            {
                root = node;
                root.next = root;
                root.prev = root;
            }

            //add new nodes to the root
            CommunicationNode prev = root;
            CommunicationNode next = root.next;

            node.next = next;
            node.prev = prev;

            next.prev = node;
            prev.next = node;
        }

        protected static void Remove( CommunicationNode node )
        {
            if( node.next != null )
                node.next.prev = node.prev;
            if( node.prev != null )
                node.prev.next = node.next;

            node.next = null;
            node.prev = null;

            if( root != null && root.next == null && root.prev == null )
                root = null;
        }



        protected virtual void InvokeThis( object data )
        {
            MethodInfo method;
            if( enabledCallbacks.TryGetValue( data.GetType(), out method ) )
            {
                if( Subject != null )
                    method.Invoke( Subject, new object[] { data } );
            }
        }

        protected virtual void RefreshCallbackBindings()
        {
            enabledMethodInfos.Clear();
            enabledCallbacks.Clear();
            // get ALL public, protected, private, and internal methods defined on the node
            var methodInfos = Subject.GetType().GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
            foreach( var methodInfo in methodInfos )
            {
                bool isReceiverMethod = methodInfo.GetCustomAttributes( true ).OfType<NVCallback>().Any();
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if( isReceiverMethod )
                {
                    // the method has a [NVCallback] attribute
                    if( parameters.Length == 1 )
                    {
                        // the method has a single parameter
                        enabledMethodInfos.Add( new SerializableMethodInfo( methodInfo ) );
                        enabledCallbacks.Add( parameters[ 0 ].ParameterType, methodInfo );
                    }
                    else
                    {
                        //Debug.LogErrorFormat("{0} is an invalid receiver method!  It must have exactly 1 parameter!", methodInfo.Name);
                        Dev.Log( methodInfo.Name + "is an invalid receiver method!  It must have exactly 1 parameter!" );
                    }
                }
            }
        }

        protected static void DefaultInvoke( object data, object subject )
        {
            CommunicationNode current = root;

            if( current != null )
            {
                do
                {
                    //prevent sources from broadcasting to themselves
                    if( current != subject && current.Subject != subject )
                    {
                        current.InvokeThis( data );
                    }

                    current = current.next;

                } while( current != root );
            }
        }

        public static event Action<object, object> InvokeAction
        {
            add
            {
                //lock( invokeAction )
                {
                    invokeAction += value;
                }
            }
            remove
            {
                //lock( invokeAction )
                {
                    invokeAction -= value;
                }
            }
        }
    }
}
