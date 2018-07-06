using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_5_3_OR_NEWER || UNITY_5
using UnityEngine;
#endif

namespace Components
{
    public class JumpListOfStrings : JumpList<string>
    {
        public static JumpListOfStrings Create()
        {
#if UNITY_5_3_OR_NEWER || UNITY_5
            return ScriptableObject.CreateInstance<JumpListOfStrings>();
#else
        return new JumpListOfStrings();
#endif
        }
    }
    public class JumpListOfInts : JumpList<int>
    {
        public static JumpListOfInts Create()
        {
#if UNITY_5_3_OR_NEWER || UNITY_5
            return ScriptableObject.CreateInstance<JumpListOfInts>();
#else
        return new JumpListOfInts();
#endif
        }
    }
    public class JumpListOfFloats : JumpList<float>
    {
        public static JumpListOfFloats Create()
        {
#if UNITY_5_3_OR_NEWER || UNITY_5
            return ScriptableObject.CreateInstance<JumpListOfFloats>();
#else
        return new JumpListOfFloats();
#endif
        }
    }

#if UNITY_5_3_OR_NEWER || UNITY_5
    public partial class JumpList<T> : ScriptableObject, IList<T>
#else
public partial class JumpList<T> : IList<T>
#endif

    {
        [Serializable]
        public struct Element
        {
            public T value;
            public int index;
        }

        [Serializable]
        protected class Jump
        {
            public int jump = -1;
            public int count = 0;

            ///from, to, jumps remaining
            public Action<int, int, int> callback;
        }

        [Serializable]
        //for users to get a list of jumps in a clean way
        public struct JumpInfo
        {
            public int from;
            public int to;
            public int count;
        }

#if UNITY_5_3_OR_NEWER || UNITY_5
        [SerializeField]
#endif
        protected List<T> _data;

#if UNITY_5_3_OR_NEWER || UNITY_5
        [SerializeField]
#endif
        protected Dictionary<int, Jump> _jumps;

        protected virtual Jump CreateNewJump()
        {
            return new Jump();
        }

        //    protected virtual void DestroyJump(int index)
        //    {
        //#if UNITY_5_3_OR_NEWER || UNITY_5
        //        if(_jumps.ContainsKey(index) == false)
        //            return;

        //        if(Application.isPlaying)
        //            Destroy(_jumps[index]);
        //        else
        //            DestroyImmediate(_jumps[index]);
        //#endif
        //    }

        public virtual List<T> GetData()
        {
            List<T> data = new List<T>();
            for(int i = 0; i < _data.Count; ++i)
                data.Add(_data[i]);
            return data;
        }

        public virtual List<JumpInfo> GetAllJumps()
        {
            List<JumpInfo> jumpInfo = new List<JumpInfo>();
            List<int> jumpKeys = new List<int>(_jumps.Keys);
            for(int i = 0; i < jumpKeys.Count; ++i)
            {
                jumpInfo.Add(new JumpInfo
                {
                    from = jumpKeys[i]
                ,
                    to = _jumps[jumpKeys[i]].jump
                ,
                    count = _jumps[jumpKeys[i]].count
                });
            }
            return jumpInfo;
        }

        public virtual int GetNumJumps()
        {
            return _jumps.Count;
        }

#if UNITY_5_3_OR_NEWER || UNITY_5
        void OnEnable()
        {
            if(_data == null)
                _data = new List<T>();
            if(_jumps == null)
                _jumps = new Dictionary<int, Jump>();
        }

        void OnDestroy()
        {
            RemoveAllJumps();
        }
#endif

        /// <summary>
        /// May be used for non-seralized jumplists (like those created for tests)
        /// Ignore the unity warning about using CreateInstance if you do this.
        /// </summary>
        public JumpList()
        {
            _data = new List<T>();
            _jumps = new Dictionary<int, Jump>();

            JumpEnumeratorAutoReset = true;
            JumpsNeedReset = false;
            ForceEnumeratorJumpReset = false;
        }

        public virtual int Count
        {
            get
            {
                return _data.Count;
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        //will be set to true if JumpEnumeratorAutoReset is set to false and all jumps have been hit
        public virtual bool JumpsNeedReset
        {
            get; private set;
        }

        //set this to false to manually control when to reset a jumplist's iterator
        public virtual bool JumpEnumeratorAutoReset
        {
            get; set;
        }

        protected virtual bool ForceEnumeratorJumpReset
        {
            get; set;
        }

        public virtual T this[int index]
        {
            get
            {
                return _data[index];
            }

            set
            {
                _data[index] = value;
            }
        }

        public virtual int IndexOf(T item)
        {
            return _data.IndexOf(item);
        }

        public virtual void Insert(int index, T item)
        {
            _data.Insert(index, item);
        }

        public virtual void RemoveAt(int index)
        {
            _data.RemoveAt(index);
        }

        public virtual void Add(T item)
        {
            _data.Add(item);
        }

        public virtual void Clear()
        {
            _data.Clear();
        }

        public virtual bool Contains(T item)
        {
            return _data.Contains(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex = 0)
        {
            _data.CopyTo(array, arrayIndex);
        }

        public virtual bool Remove(T item)
        {
            return _data.Remove(item);
        }

        public virtual void RemoveRange(int index, int count)
        {
            _data.RemoveRange(index, count);
        }

        //forces the next run enumerator to reset its jumps
        public void ResetEnumeratorJumps()
        {
            ForceEnumeratorJumpReset = true;
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            for(int i = 0; i < _data.Count; ++i)
            {
                yield return _data[i];
            }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        //Jumplist specific methods

        protected virtual void AssertIndex(int i)
        {
            if(i < 0 || i >= _data.Count)
                throw new IndexOutOfRangeException();
        }

        //Returns false if this jump does not exist
        public virtual bool SetJumpCallback(int index, Action<int, int, int> callback)
        {
            AssertIndex(index);
            if(_jumps.ContainsKey(index) == false)
                return false;

            _jumps[index].callback = callback;
            return true;
        }

        public virtual void RemoveAllCallbacks()
        {
            for(int i = 0; i < _data.Count; ++i)
            {
                _jumps[i].callback = null;
            }
        }

        public virtual bool HasJump(int index)
        {
            return (_jumps.ContainsKey(index));
        }

        public virtual int GetJump(int index)
        {
            AssertIndex(index);
            if(_jumps.ContainsKey(index))
                return _jumps[index].jump;
            return -1;
        }

        public virtual Action<int, int, int> GetJumpCallback(int index)
        {
            AssertIndex(index);
            if(_jumps.ContainsKey(index))
                return _jumps[index].callback;
            return null;
        }

        public virtual int GetJumpCount(int index)
        {
            AssertIndex(index);
            if(_jumps.ContainsKey(index))
                return _jumps[index].count;
            return 0;
        }

        public virtual void SetJumpCount(int index, int count)
        {
            AssertIndex(index);
            if(_jumps.ContainsKey(index))
            {
                _jumps[index].count = count;
            }
            else
            {
                Jump j = CreateNewJump();
                j.count = count;

                //default the jump to point to the next element
                j.jump = (index + 1) % _data.Count;
                _jumps.Add(index, j);
            }
        }

        public virtual void SetJump(int from, int to, int count = 1, Action<int, int, int> callback = null)
        {
            AssertIndex(from);
            AssertIndex(to);
            if(_jumps.ContainsKey(from))
            {
                _jumps[from].jump = to;
                SetJumpCount(from, count);
            }
            else
            {
                Jump j = CreateNewJump();
                j.count = count;
                j.callback = callback;
                //default the jump to point to the next element
                j.jump = to;
                _jumps.Add(from, j);
            }
        }

        public virtual void RemoveJump(int index)
        {
            AssertIndex(index);
            if(_jumps.ContainsKey(index))
            {
                //DestroyJump(index);
                _jumps.Remove(index);
            }
        }

        public virtual void RemoveAllJumps()
        {
            //#if UNITY_5_3_OR_NEWER || UNITY_5
            //        foreach(var jump in _jumps)
            //            DestroyJump(jump.Key);
            //#endif
            _jumps.Clear();
        }

        public virtual int CalculateDistance(int from, int to, bool forward = true)
        {
            AssertIndex(from);
            AssertIndex(to);

            int direction = 1;
            if(!forward)
                direction = -1;

            int dist = 0;

            Dictionary<int, int> counters = new Dictionary<int, int>();

            int k = from;
            while(k != to)
            {
                List<int> keyList = new List<int>(_jumps.Keys);

                bool has_next_jump = false;

                //is there a jump in front of the iterator?
                int i = (forward ? 0 : keyList.Count - 1);
                bool checking_jumps = (keyList.Count > 0);

                for(; checking_jumps; i += direction)
                {
                    int jump_point = keyList[i];

                    int next = i + direction;
                    if(next < 0 || next >= keyList.Count)
                        checking_jumps = false;

                    if(forward)
                    {
                        if(jump_point < k)
                            continue;
                        if(to < jump_point)
                            continue;
                    }
                    else
                    {
                        if(jump_point > k)
                            continue;
                        if(to > jump_point)
                            continue;
                    }

                    if(!counters.ContainsKey(jump_point))
                        counters.Add(jump_point, _jumps[jump_point].count);

                    if(counters[jump_point] <= 0)
                        continue;

                    counters[jump_point] = counters[jump_point] - 1;

                    has_next_jump = true;
                    break;
                }

                //no more jumps in front of us
                if(has_next_jump == false)
                {
                    if(forward)
                    {
                        //we're going to loop around, add the end amount remaining
                        if(to < k)
                        {
                            //might need -1?
                            dist += _data.Count - k;
                            k = 0;
                        }
                        //else add the remaining amount
                        else
                        {
                            dist += to - k;
                            k = to;
                        }
                    }
                    else
                    {
                        //we're going to loop around, add the end amount remaining
                        if(to > k)
                        {
                            //might need -1?
                            dist += -k;
                            k = _data.Count;
                        }
                        //else add the remaining amount
                        else
                        {
                            dist += to - k;
                            k = to;
                        }
                    }
                }
                //there's a jump coming up
                else
                {
                    int jump_point = keyList[i];

                    //add/subtract 1 for the jump since we'll still visit the end point
                    dist += direction;

                    //get the distance to the jump
                    dist += (jump_point - k);

                    //move k to the jump end point
                    k = _jumps[jump_point].jump;
                }
            }

            return dist * direction;
        }

        public virtual int NextIndex(int i, bool forward = true)
        {
            if(forward)
                i = (i + 1) % _data.Count;
            else
                i = (i - 1);

            if(i < 0)
                i = _data.Count - 1;
            return i;
        }

        public virtual IEnumerator<Element> GetJumpEnumerator(int start = 0, bool forward = true, Action resetJumpsCallback = null)
        {
            Action resetCallback = resetJumpsCallback;
            Dictionary<int, int> counters = new Dictionary<int, int>();
            int i = start;
            for(;;)
            {
                if((JumpsNeedReset && JumpEnumeratorAutoReset) || ForceEnumeratorJumpReset)
                {
                    ForceEnumeratorJumpReset = false;
                    JumpsNeedReset = false;
                    counters.Clear();
                    if(resetCallback != null)
                        resetCallback.Invoke();
                }

                yield return new Element { value = _data[i], index = i };

                //is there a jump at this index?
                if(!_jumps.ContainsKey(i))
                {
                    i = NextIndex(i, forward);
                    continue;
                }

                //have we counted and made every jump in the list?
                //if so, reset the counters
                if(!JumpsNeedReset && _jumps.Count > 0 && _jumps.Count == counters.Count)
                {
                    bool resetCounters = true;
                    foreach(var jumpCounter in counters)
                    {
                        if(jumpCounter.Value > 0)
                        {
                            resetCounters = false;
                            break;
                        }
                    }
                    if(resetCounters)
                        JumpsNeedReset = true;
                }

                //do we have a counter tracking the jumps yet?
                if(!counters.ContainsKey(i))
                {
                    //if not, add and initialize it
                    counters.Add(i, _jumps[i].count);
                }

                if(counters[i] > 0)
                {
                    counters[i] = counters[i] - 1;

                    if(_jumps[i].callback != null)
                        _jumps[i].callback(i, _jumps[i].jump, counters[i]);

                    i = _jumps[i].jump;
                }
                else
                {
                    i = NextIndex(i, forward);
                }
            }
        }
    }
}