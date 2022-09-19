using System.Collections.Generic;

namespace nv
{
    public partial class JumpList<T>
    {
        public class Iterator
        {
            JumpList<T> _jumpList;

            bool _forwardIteration;

            int _currentIndex = 0;

            Dictionary<int, int> _jumpCounters = new Dictionary<int, int>();

            bool _resetCountersOnNext = false;

            void CreateIterator(int at_index, bool direction)
            {
                _jumpCounters.Clear();
                _forwardIteration = direction;
                _currentIndex = at_index;
            }

            public Iterator(JumpList<T> jump_list, int start_index = 0, bool forward_iteration = true)
            {
                _jumpList = jump_list;
                CreateIterator(start_index, forward_iteration);
            }

            public Iterator(Iterator other)
            {
                _jumpList = other._jumpList;
                _forwardIteration = other._forwardIteration;
                _jumpCounters = new Dictionary<int, int>(other._jumpCounters);
                _currentIndex = other._currentIndex;
                _resetCountersOnNext = other._resetCountersOnNext;
            }

            public JumpList<T> CurrentList
            {
                get
                {
                    return _jumpList;
                }
            }

            public bool ForwardIteration
            {
                set
                {
                    _forwardIteration = value;
                }
                get
                {
                    return _forwardIteration;
                }
            }

            public int CurrentIndex
            {
                get
                {
                    return _currentIndex;
                }
                set
                {
                    _currentIndex = value;
                }
            }

            public T CurrentValue
            {
                get
                {
                    return _jumpList[CurrentIndex];
                }
                set
                {
                    _jumpList[CurrentIndex] = value;
                }
            }

            public Element CurrentElement
            {
                get
                {
                    return new Element { value = CurrentValue, index = CurrentIndex };
                }
            }

            /// <summary>
            /// Get the N values from the sequence, starting with the current. Does not move this iterator.
            /// </summary>
            /// <param name="n">number of items to get</param>
            /// <returns>list of the next n items, starting with the current</returns>
            public List<T> Peek(int n)
            {
                List<T> next = new List<T>();
                if(n <= 0)
                    return next;

                Iterator copy = new Iterator(this);
                for(int i = 0; i < n; ++i, copy.Next())
                    next.Add(copy.CurrentValue);

                return next;
            }

            /// <summary>
            /// Get the N indices from the sequence, starting with the current. Does not move this iterator.
            /// </summary>
            /// <param name="n">number of indices to get</param>
            /// <returns>list of the next n indices, starting with the current</returns>
            public List<int> PeekIndices(int n)
            {
                List<int> next = new List<int>();
                if(n <= 0)
                    return next;

                Iterator copy = new Iterator(this);
                for(int i = 0; i < n; ++i, copy.Next())
                    next.Add(copy.CurrentIndex);

                return next;
            }

            /// <summary>
            /// Get the N elements from the sequence, starting with the current. Does not move this iterator.
            /// </summary>
            /// <param name="n">number of indices to get</param>
            /// <returns>list of the next n elements, starting with the current</returns>
            public List<Element> PeekElements(int n)
            {
                List<Element> next = new List<Element>();
                if(n <= 0)
                    return next;

                Iterator copy = new Iterator(this);
                for(int i = 0; i < n; ++i, copy.Next())
                    next.Add(copy.CurrentElement);

                return next;
            }

            public T MoveNext()
            {
                Next();
                return _jumpList[CurrentIndex];
            }

            public void ClearJumpHistory()
            {
                CreateIterator(CurrentIndex, _forwardIteration);
            }

            protected void Next()
            {
                if(_resetCountersOnNext)
                {
                    _resetCountersOnNext = false;
                    _jumpCounters.Clear();
                }

                //is there a jump at this index?
                if(!_jumpList.HasJump(CurrentIndex))
                {
                    CurrentIndex = _jumpList.NextIndex(CurrentIndex, ForwardIteration);
                    return;
                }

                //have we counted and made every jump in the list?
                //if so, reset the counters
                if(_jumpList.GetNumJumps() > 0 && _jumpList.GetNumJumps() == _jumpCounters.Count)
                {
                    bool resetCounters = true;
                    foreach(var jumpCounter in _jumpCounters)
                    {
                        if(jumpCounter.Value > 0)
                        {
                            resetCounters = false;
                            break;
                        }
                    }
                    if(resetCounters)
                        _resetCountersOnNext = true;
                }

                //do we have a counter tracking the jumps yet?
                if(!_jumpCounters.ContainsKey(CurrentIndex))
                {
                    //if not, add and initialize it
                    _jumpCounters.Add(CurrentIndex, _jumpList._jumps[CurrentIndex].count);
                }

                if(_jumpCounters[CurrentIndex] > 0)
                {
                    _jumpCounters[CurrentIndex] = _jumpCounters[CurrentIndex] - 1;

                    if(_jumpList._jumps[CurrentIndex].callback != null)
                        _jumpList._jumps[CurrentIndex].callback(CurrentIndex, _jumpList._jumps[CurrentIndex].jump, _jumpCounters[CurrentIndex]);

                    CurrentIndex = _jumpList._jumps[CurrentIndex].jump;
                }
                else
                {
                    CurrentIndex = _jumpList.NextIndex(CurrentIndex, ForwardIteration);
                }
            }
        }
    }
}
