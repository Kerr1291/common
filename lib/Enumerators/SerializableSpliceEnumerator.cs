using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using SerializableObject = UnityEngine.Object;
using System.Reflection;

namespace nv
{
    [Serializable]
    public abstract class SerializableSpliceEnumerator<TData,TSpliceEvent> : SpliceEnumerator<TData>, ISerializationCallbackReceiver
        where TSpliceEvent : UnityEvent<int,int,int>, new()
    {
        public UnityEvent onMoveNextEvent;
        public UnityEvent onResetEvent;
        public TSpliceEvent onSpliceEvent;

        public SerializableSpliceEnumerator(Func<int, TData> getItem = null, Func<int> itemsCount = null, Action<int, TData> setItem = null)
            : base(getItem, itemsCount, setItem)
        {
        }

        public SerializableSpliceEnumerator(SerializableSpliceEnumerator<TData, TSpliceEvent> other)
            : base(other)
        {
        }

        protected override void InvokeOnMoveNext()
        {
            if(isSerializing)
                return;

            base.InvokeOnMoveNext();
            onMoveNextEvent.Invoke();
        }

        protected override void InvokeOnReset()
        {
            if(isSerializing)
                return;

            base.InvokeOnReset();
            onResetEvent.Invoke();
        }

        protected override void InvokeOnSplice(int spliceFrom, int spliceTo, int spliceCountRemaining)
        {
            if(isSerializing)
                return;

            base.InvokeOnSplice(spliceFrom, spliceTo, spliceCountRemaining);
            onSpliceEvent.Invoke(spliceFrom, spliceTo, spliceCountRemaining);
        }





        [SerializeField, HideInInspector] protected List<SerializableMemberInfo> onMoveNextInfo;
        [SerializeField, HideInInspector] protected List<SerializableMemberInfo> onResetInfo;
        [SerializeField, HideInInspector] protected List<SerializableMemberInfo> onSpliceInfo;
        [SerializeField, HideInInspector] protected List<SerializableMemberInfo> setItemInfo;
        [SerializeField, HideInInspector] protected List<SerializableMemberInfo> getItemInfo;
        [SerializeField, HideInInspector] protected List<SerializableMemberInfo> itemsCountInfo;
        [SerializeField, HideInInspector] protected List<SerializableObject> onMoveNextInfoTargets;
        [SerializeField, HideInInspector] protected List<SerializableObject> onResetInfoTargets;
        [SerializeField, HideInInspector] protected List<SerializableObject> onSpliceInfoTargets;
        [SerializeField, HideInInspector] protected List<SerializableObject> setItemInfoTargets;
        [SerializeField, HideInInspector] protected List<SerializableObject> getItemInfoTargets;
        [SerializeField, HideInInspector] protected List<SerializableObject> itemsCountInfoTargets;

        //TODO: serialize jumps...

        bool isSerializing;

        protected virtual List<SerializableMemberInfo> GetSerializableMemberInfos(Delegate[] delegates)
        {
            return delegates.Where(m => m.Target is SerializableObject).Select(x =>
            {
                var info = new SerializableMemberInfo();
                info.Info = x.Method;
                return info;
            }).ToList();
        }

        protected virtual void SaveDelegateReferences(Action action, ref List<SerializableMemberInfo> methods, ref List<SerializableObject> targets)
        {
            if(action == null)
                return;
            methods = GetSerializableMemberInfos(action.GetInvocationList());
            targets = action.GetInvocationList().Where(m => m.Target is SerializableObject).Select(x => x.Target as SerializableObject).ToList();
        }

        protected virtual void SaveDelegateReferences(Action<int,int,int> action, ref List<SerializableMemberInfo> methods, ref List<SerializableObject> targets)
        {
            if(action == null)
                return;
            methods = GetSerializableMemberInfos(action.GetInvocationList());
            targets = action.GetInvocationList().Where(m => m.Target is SerializableObject).Select(x => x.Target as SerializableObject).ToList();
        }

        protected virtual void SaveDelegateReferences(Action<int,TData> action, ref List<SerializableMemberInfo> methods, ref List<SerializableObject> targets)
        {
            if(action == null)
                return;
            methods = GetSerializableMemberInfos(action.GetInvocationList());
            targets = action.GetInvocationList().Where(m => m.Target is SerializableObject).Select(x => x.Target as SerializableObject).ToList();
        }

        protected virtual void SaveDelegateReferences(Func<int, TData> action, ref List<SerializableMemberInfo> methods, ref List<SerializableObject> targets)
        {
            if(action == null)
                return;
            methods = GetSerializableMemberInfos(action.GetInvocationList());
            targets = action.GetInvocationList().Where(m => m.Target is SerializableObject).Select(x => x.Target as SerializableObject).ToList();
        }

        protected virtual void SaveDelegateReferences(Func<int> action, ref List<SerializableMemberInfo> methods, ref List<SerializableObject> targets)
        {
            if(action == null)
                return;
            methods = GetSerializableMemberInfos(action.GetInvocationList());
            targets = action.GetInvocationList().Where(m => m.Target is SerializableObject).Select(x => x.Target as SerializableObject).ToList();
        }

        protected virtual void LoadDelegateReferences<TAction>(ref TAction action, ref List<SerializableMemberInfo> methods, ref List<SerializableObject> targets)
            where TAction : class
        {
            if(methods == null)
                return;
            if(targets == null)
                return;

            Delegate result = null;
            for(int i = 0; i < methods.Count; ++i)
            {
                SerializableObject target = targets[i];

                if(target == null)
                    continue;

                MethodInfo mi = methods[i].Info as MethodInfo;                    

                //convert the reflected methodinfo and target into our delegate type and assign it
                if(result == null)
                    result = Delegate.CreateDelegate(typeof(TAction), target, mi, true);
                else
                    result = Delegate.Combine(result, Delegate.CreateDelegate(typeof(TAction), target, mi, true));
            }
            action = result as TAction;
            methods = null;
            targets = null;
        }


        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            SaveDelegateReferences(OnMoveNext, ref onMoveNextInfo, ref onMoveNextInfoTargets);
            SaveDelegateReferences(OnReset, ref onResetInfo, ref onResetInfoTargets);
            SaveDelegateReferences(OnSplice, ref onSpliceInfo, ref onSpliceInfoTargets);

            SaveDelegateReferences(SetItem, ref setItemInfo, ref setItemInfoTargets);
            SaveDelegateReferences(GetItem, ref getItemInfo, ref getItemInfoTargets);
            SaveDelegateReferences(ItemsCount, ref itemsCountInfo, ref itemsCountInfoTargets);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            isSerializing = true;
            LoadDelegateReferences(ref OnMoveNext, ref onMoveNextInfo, ref onMoveNextInfoTargets);
            LoadDelegateReferences(ref OnReset, ref onResetInfo, ref onResetInfoTargets);
            LoadDelegateReferences(ref OnSplice, ref onSpliceInfo, ref onSpliceInfoTargets);

            LoadDelegateReferences(ref SetItem, ref setItemInfo, ref setItemInfoTargets);
            LoadDelegateReferences(ref GetItem, ref getItemInfo, ref getItemInfoTargets);
            LoadDelegateReferences(ref ItemsCount, ref itemsCountInfo, ref itemsCountInfoTargets);
            isSerializing = false;
        }
    }
}