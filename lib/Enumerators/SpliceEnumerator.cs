using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Math = nv.Mathnv;
using SerializeField = UnityEngine.SerializeField;
using Hide = UnityEngine.HideInInspector;

namespace nv
{
    //TODO: create versions of the Peek() methods that take ref arrays as params to allow for a version that doesn't make garbage
    public class SpliceEnumerator<TData> : IEnumerator<int>
    {
        //public delegate void SpliceDelegate(int currentIndex, int spliceToIndex, int remainingSpliceCount);

        public Action OnMoveNext;
        public Action OnReset;
        public Action<int, int, int> OnSplice;

        protected Action<int, TData> SetItem;
        protected Func<int, TData> GetItem;
        protected Func<int> ItemsCount;

        [SerializeField] protected int current = 0;
        [SerializeField] protected CircularIntBuffer previous = new CircularIntBuffer();

        /// <summary>
        /// iteration direction & how many stops per MoveNext() call
        /// </summary>
        public int velocity = -1;

        //key: the indices that contain splice points
        //value: a pair with the first value = the index to splice to; 
        //                   the second value = the number of times per "loop" of the reel this splice should be invoked.
        protected Dictionary<int, KeyValuePair<int, int>> jumps = new Dictionary<int, KeyValuePair<int, int>>();

        protected bool resetCountersOnNext = false;
        protected Dictionary<int, int> counters = new Dictionary<int, int>();

        protected bool inReset = false;

        protected IEnumerator<int> enumerator;

        public SpliceEnumerator(Func<int, TData> getItem = null, Func<int> itemsCount = null, Action<int, TData> setItem = null)
        {
            jumps = new Dictionary<int, KeyValuePair<int, int>>();
            previous = new CircularIntBuffer(10,10);
            if(getItem != null && itemsCount != null)
                Reset(getItem,itemsCount,setItem);
        }

        public SpliceEnumerator(SpliceEnumerator<TData> other)
        {
            SetItem = other.SetItem;
            GetItem = other.GetItem;
            ItemsCount = other.ItemsCount;

            this.velocity = other.velocity;
            this.current = other.current;
            this.jumps = new Dictionary<int, KeyValuePair<int, int>>(other.Splices);
            this.resetCountersOnNext = other.resetCountersOnNext;
            this.counters = new Dictionary<int, int>(other.Counters);
            this.previous = new CircularIntBuffer(other.previous);
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return SetItem == null;
            }
        }

        public virtual TData CurrentItem
        {
            get
            {
                if(Count <= 0)
                    return default(TData);

                return GetItem(Current);
            }
            set
            {
                if(IsReadOnly)
                    throw new System.InvalidOperationException("SetItem is null! This is a readonly object or the object has not been configured");

                SetItem(Current,value);
            }
        }

        public virtual int Current
        {
            get
            {
                return current;
            }
            set
            {
                if(value != current)
                    MoveTo(value);
            }
        }

        public virtual int GetCurrent()
        {
            return Current;
        }

        public virtual int Next
        {
            get
            {
                return CalculateNext(current, velocity, new Dictionary<int, int>(Counters));
            }
        }

        public virtual Dictionary<int, KeyValuePair<int, int>> Splices
        {
            get
            {
                if(jumps == null)
                    jumps = new Dictionary<int, KeyValuePair<int, int>>();
                return jumps;
            }
        }

        public virtual Dictionary<int, int> Counters
        {
            get
            {
                if(counters == null)
                    counters = new Dictionary<int, int>();
                return counters;
            }
        }

        public virtual CircularBuffer<int> Previous
        {
            get
            {
                return previous;
            }
        }

        protected virtual int Count
        {
            get
            {
                return ItemsCount != null ? ItemsCount() : 0;
            }
        }

        protected virtual IEnumerator<int> Enumerator
        {
            get
            {
                if(enumerator == null)
                {
                    enumerator = EnumerateItems();
                }
                return enumerator;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public virtual int GetIndex(int offset)
        {
            if(offset == 1)
                return Next;
            else if(offset > 1)
                return Peek(offset).Last();
            else if(offset == 0)
                return Current;
            else
            {
                int positiveOffset = -offset;
                if(positiveOffset < Previous.Count)
                    return Previous[Previous.Count - positiveOffset];
                else
                    return Math.Modulus(Current - positiveOffset, Count);
            }
        }

        public virtual bool MoveNext()
        {    
            previous.Enqueue(Current);

            Enumerator.MoveNext();

            if(!inReset)
                InvokeOnMoveNext();

            return true;
        }
        
        public virtual bool MoveTo(int stop)
        {
            stop = Math.Modulus(stop, Count);

            for(int i = 0; Current != stop; ++i)
            {
                previous.Enqueue(Current);
                Enumerator.MoveNext();
            }

            if(!inReset)
                InvokeOnMoveNext();

            return Current == stop;
        }
        
        public virtual bool MoveNext(int count, int? stopIndex = null)
        {
            for(int i = 0; i < count; ++i)
            {
                previous.Enqueue(Current);
                Enumerator.MoveNext();
                
                if(stopIndex != null && Current == stopIndex.Value)
                {
                    break;
                }
            }

            if(!inReset)
                InvokeOnMoveNext();

            return (stopIndex != null && stopIndex.Value == Current);
        }

        public virtual void Reset(Func<int, TData> getItem = null, Func<int> itemsCount = null, Action<int, TData> setItem = null)
        {
            SetItem = setItem;
            GetItem = getItem;
            ItemsCount = itemsCount;
            Reset();
        }

        public virtual void Reset()
        {
            if(GetItem == null || ItemsCount == null)
                return;

            inReset = true;

            previous = new CircularIntBuffer(System.Math.Max(Count,10), System.Math.Max(Count * 2,20));
            Counters.Clear();
            resetCountersOnNext = false;

            if(Count > 0)
            {
                //iterate until we return to the current position to populate the previous buffer with a nice history
                int prev = current;
                MoveNext();
                MoveTo(prev);
            }

            InvokeOnReset();

            inReset = false;
        }

        public virtual void Dispose()
        {
            OnMoveNext = null;
            OnReset = null;
            OnSplice = null;

            SetItem = null;
            GetItem = null;
            ItemsCount = null;
        }

        /// <summary>
        /// Get the N values from the sequence, starting with the current. Does not move this iterator.
        /// </summary>
        /// <param name="n">number of items to get</param>
        /// <returns>list of the next n items, starting with the current</returns>
        public virtual TData[] PeekElements(int n)
        {
            if(n < 0)
                return new TData[0];

            TData[] peek = new TData[n + 1];

            if(Count <= 0 || n < 0)
                return peek;
            else if(n == 0)
            {
                peek[0] = GetItem(Current);
                return peek;
            }
            else if(n == 1)
            {
                peek[0] = GetItem(Current);
                peek[1] = GetItem(Next);
                return peek;
            }

            SpliceEnumerator<TData> copy = new SpliceEnumerator<TData>(this);
            for(int i = 0; i < n; ++i, copy.MoveNext())
                peek[i] = (copy.CurrentItem);

            return peek;
        }

        /// <summary>
        /// Get all the values between current and the next stop.
        /// </summary>
        public virtual List<TData> PeekElementsTo(int index)
        {
            List<TData> peek = new List<TData>();

            index = Math.Modulus(index, Count);
            int next = Next;
            if(index == Current)
            {
                peek.Add(GetItem(Current));
                return peek;
            }
            else if(index == next)
            {
                peek.Add(GetItem(Current));
                peek.Add(GetItem(next));
                return peek;
            }

            SpliceEnumerator<TData> copy = new SpliceEnumerator<TData>(this);
            for(; copy.Current != index; copy.MoveNext())
                peek.Add(copy.CurrentItem);

            //include the stopping value
            peek.Add(copy.CurrentItem);

            return peek;
        }

        /// <summary>
        /// Get the N values from the sequence, starting with the current. Does not move this iterator.
        /// </summary>
        /// <param name="n">number of items to get</param>
        /// <returns>list of the next n items, starting with the current</returns>
        public virtual int[] Peek(int n)
        {
            if(n < 0)
                return new int[0];

            int[] peek = new int[n + 1];

            if(Count <= 0 || n < 0)
                return peek;
            else if(n == 0)
            {
                peek[0] = Current;
                return peek;
            }
            else if(n == 1)
            {
                peek[0] = Current;
                peek[1] = Next;
                return peek;
            }

            SpliceEnumerator<TData> copy = new SpliceEnumerator<TData>(this);
            for(int i = 0; i < n; ++i, copy.MoveNext())
                peek[i] = (copy.Current);

            return peek;
        }

        /// <summary>
        /// Get the N values from the sequence, starting with the given index. Does not move this iterator.
        /// </summary>
        /// <param name="n">number of items to get</param>
        /// <returns>list of the next n items, starting with the current</returns>
        public virtual int[] PeekFrom(int fromIndex, int n)
        {
            if(n < 0)
                return new int[0];

            fromIndex = Math.Modulus(fromIndex, Count);
            int[] peek = new int[n + 1];

            if(Count <= 0 || n < 0)
                return peek;
            else if(n == 0)
            {
                peek[0] = fromIndex;
                return peek;
            }

            SpliceEnumerator<TData> copy = new SpliceEnumerator<TData>(this);
            copy.Current = fromIndex;
            for(int i = 0; i < n; ++i, copy.MoveNext())
                peek[i] = (copy.Current);

            return peek;
        }

        /// <summary>
        /// Get all the values between current and the next stop.
        /// </summary>
        public virtual List<int> PeekTo(int index)
        {
            List<int> peek = new List<int>();

            index = Math.Modulus(index, Count);
            int next = Next;
            if(index == Current)
            {
                peek.Add(Current);
                return peek;
            }
            else if(index == next)
            {
                peek.Add(Current);
                peek.Add(next);
                return peek;
            }

            SpliceEnumerator<TData> copy = new SpliceEnumerator<TData>(this);
            for(; copy.Current != index; copy.MoveNext())
                peek.Add(copy.Current);

            //include the stopping value
            peek.Add(copy.Current);

            return peek;
        }

        /// <summary>
        /// Get all the values between the two indices.
        /// </summary>
        public virtual List<int> PeekFromTo(int fromIndex, int toIndex, int direction = 1)
        {
            List<int> peek = new List<int>();

            fromIndex = Math.Modulus(fromIndex, Count);
            toIndex = Math.Modulus(toIndex, Count);

            if(fromIndex == toIndex)
            {
                peek.Add(fromIndex);
                return peek;
            }

            SpliceEnumerator<TData> copy = new SpliceEnumerator<TData>(this);
            copy.Current = fromIndex;
            copy.velocity = System.Math.Sign(direction);
            for(; copy.Current != toIndex; copy.MoveNext())
                peek.Add(copy.Current);

            //include the stopping value
            peek.Add(copy.Current);

            return peek;
        }

        protected virtual void CalculateShouldResetJumpCounters(Dictionary<int, int> counters)
        {
            //is there a jump at this index?
            //have we counted and made every jump in the list?
            //if so, reset the counters
            if(jumps.Count > 0 && jumps.Count == counters.Count)
            {
                resetCountersOnNext = true;
                foreach(var jumpCounter in counters)
                {
                    if(jumpCounter.Value > 0)
                    {
                        resetCountersOnNext = false;
                        break;
                    }
                }
            }
        }

        protected virtual bool TrackJump(int index, Dictionary<int, int> counters)
        {
            
            if(resetCountersOnNext)
            {
                resetCountersOnNext = false;
                counters.Clear();
            }

            bool willJump = false;
            if(!counters.ContainsKey(index))
            {
                //if not, add and initialize it
                counters.Add(index, Splices[index].Value);
            }

            int jumpsRemaining = counters[index];

            if(jumpsRemaining > 0)
            {
                counters[index] = jumpsRemaining - 1;

                willJump = true;

                if(!inReset)
                    InvokeOnSplice(index, Splices[index].Key, counters[index]);
            }
            else
            {
                CalculateShouldResetJumpCounters(counters);
            }

            return willJump;
        }

        protected virtual int GetNextWithoutJump(int start, int distance)
        {
            int next = Math.Modulus((start + distance), (Count));
            return next;
        }

        protected virtual int GetNextWithJump(int next)
        {
            next = Math.Modulus(Splices[next].Key, Count);
            return next;
        }

        protected int CalculateNext(int start, int distance, Dictionary<int, int> counters)
        {
            //not moving? then just return the start
            if(distance == 0)
            {
                return start;
            }

            int next = GetNextWithoutJump(start, distance);
            int absDistance = System.Math.Abs(distance);

            //if the distance is 1, then just see if the potential next has a jump
            if(absDistance == 1)
            {
                if(Splices.ContainsKey(next) )
                {
                    bool willJump = TrackJump(next, counters);

                    if(willJump)
                        return GetNextWithJump(next);
                }
            }
            else
            {
                //check each index that will be passed for a jump
                for(int i = 1; i <= absDistance; ++i)
                {
                    int direction = System.Math.Sign(distance);
                    int temp = GetNextWithoutJump(start, i * direction);
                    if(Splices.ContainsKey(temp))
                    {
                        bool willJump = TrackJump(next, counters);

                        if(willJump)
                            //recursively continue the remaining distance in case there are more jumps
                            return CalculateNext(GetNextWithJump(temp), absDistance - i, counters);
                    }
                }
            }
            
            return next;
        }
        
        protected virtual IEnumerator<int> EnumerateItems()
        {
            for(; ; )
            {
                current = CalculateNext(current, velocity, this.counters);

                yield return current;
            }
        }

        protected virtual void InvokeOnMoveNext()
        {
            if(OnMoveNext != null)
                OnMoveNext.Invoke();
        }

        protected virtual void InvokeOnReset()
        {
            if(OnReset != null)
                OnReset.Invoke();
        }

        protected virtual void InvokeOnSplice(int spliceFrom, int spliceTo, int spliceCountRemaining)
        {            
            if(OnSplice != null)
                OnSplice.Invoke(spliceFrom, spliceTo, spliceCountRemaining);
        }
    }

    [Serializable]
    public class CircularIntBuffer : CircularBuffer<int>
    {
        public CircularIntBuffer(int initialBufferSize = 10, int maxSize = int.MaxValue)
            : base(initialBufferSize, maxSize)
        {}

        public CircularIntBuffer(CircularIntBuffer other)
            :base(other)
        {}
    }

    public static class SpliceEnumeratorExtensions
    {
        public static SpliceEnumerator<TData> GetSpliceEnumerator<TData>(this IList<TData> collection)
        {
            var enumerator = new SpliceEnumerator<TData>((x) => collection[x], () => collection.Count, (x,y) => collection[x] = y);
            return enumerator;
        }

        public static SpliceEnumerator<TData> GetSpliceEnumerator<TData>(this AnObservableCollection<TData> collection)
        {
            var enumerator = collection.GetSpliceEnumerator();
            collection.OnCountChanged += enumerator.Reset;
            return enumerator;
        }
    }
}