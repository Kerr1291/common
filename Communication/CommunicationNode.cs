using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

//using UnityEngine;

namespace nv
{
    public interface INVMessageHandler
    {
        void HandleMessage(object msg);
    }

    public class CommunicationHub
    {
        static CommunicationHub instance;

        List<INVMessageHandler> nodes = new List<INVMessageHandler>();

        public static void AddCommsNode(INVMessageHandler n)
        {
            if(instance.nodes.Contains(n))
            {
                instance.nodes.Add(n);
            }
        }

        public static void RemoveCommsNode(INVMessageHandler n)
        {
            if(instance.nodes.Contains(n))
            {
                instance.nodes.Remove(n);
            }
        }

        /// <summary>
        /// Publish a message to all message handlers.
        /// </summary>
        /// <param name="msg">The <see cref="object"/> to publish.</param>
        public static void BroadcastMessage(object msg)
        {
            if(instance == null)
                instance = new CommunicationHub();

            foreach(INVMessageHandler n in instance.nodes)
            {
                n.HandleMessage(msg);
            }
        }
    }    
    

    public class NVMessageHandler : Attribute
    {
    }

    [System.Serializable]
    public class CommunicationNode<T> : INVMessageHandler, IDisposable
    {
        private readonly Dictionary<Type, MethodInfo> enabledMessageCallbacks = new Dictionary<Type, MethodInfo>();

        //[SerializeField]
        //[HideInInspector]
        public List<SerializableMethodInfo> enabledMethodInfos = new List<SerializableMethodInfo>();

        public CommunicationNode()
        {
            CommunicationHub.AddCommsNode(this);
            enabledMethodInfos.Clear();
            enabledMessageCallbacks.Clear();
            // get ALL public, protected, private, and internal methods defined on the node
            var methodInfos = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach(var methodInfo in methodInfos)
            {
                bool isReceiverMethod = methodInfo.GetCustomAttributes(true).OfType<NVMessageHandler>().Any();
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if(isReceiverMethod)
                {
                    // the method has a [NVMessageHandler] attribute
                    if(parameters.Length == 1)
                    {
                        // the method has a single parameter
                        enabledMethodInfos.Add(new SerializableMethodInfo(methodInfo));
                        enabledMessageCallbacks.Add(parameters[0].ParameterType, methodInfo);
                    }
                    else
                    {
                        //Debug.LogErrorFormat("{0} is an invalid receiver method!  It must have exactly 1 parameter!", methodInfo.Name);
                    }
                }
            }
        }

        public virtual void HandleMessage(object msg)
        {
            MethodInfo method;
            if(enabledMessageCallbacks.TryGetValue(msg.GetType(), out method))
            {
                method.Invoke(this, new object[] { msg });
            }
        }

        /// <summary>
        /// Broadcasts a message to all receivers.
        /// </summary>
        /// <param name="msg">The <see cref="object"/> to broadcast.</param>
        public void BroadcastMessage(object msg)
        {
            CommunicationHub.BroadcastMessage(msg);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if(!disposedValue)
            {
                if(disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                //TODO: test me!
                CommunicationHub.RemoveCommsNode(this);
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
         ~CommunicationNode() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
           Dispose(false);
         }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    //public class Foo
    //{
    //    CommunicationNode<Foo> comms = new CommunicationNode<Foo>();

    //    void Bar()
    //    {
    //        comms.BroadcastMessage(5);
    //    }

    //    [NVMessageHandler]
    //    void Baz(int m)
    //    {

    //    }
    //}
}
