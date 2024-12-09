using System.Collections.Generic;
using System;
using System.Collections;

namespace CWJ.Collection
{
    [Serializable]
    public class StackLimited<T> : IEnumerable<T>
    {
#if UNITY_EDITOR
        [UnityEngine.SerializeField, Readonly]
#endif
        private int _maxSize;
#if UNITY_EDITOR
        [UnityEngine.SerializeField, Readonly]
#endif
        private int _count;
#if UNITY_EDITOR
        [UnityEngine.SerializeField, Readonly]
#endif
        private T[] _array;

        public StackLimited(int maxSize)
        {
            if (maxSize == 0)
                throw new ArgumentOutOfRangeException("maxSize는 0보다 커야 합니다.");
            _maxSize = maxSize;
            _array = new T[_maxSize];
            _count = 0;
        }
        public int Count { get => _count; }
        public int MaxSize { get => _maxSize; }

        public T this[int index]
        {
            get
            {
                if (index < 0)
                    throw new Exception($"Index {index} 범위 벗어남. (0 ~ )");
                else if (index >= _maxSize)
                    throw new Exception($"Index {index} 범위 벗어남. ( ~ {_maxSize - 1})");
                //else if (index >= Count)
                //    throw new IndexOutOfRangeException($"Index {index} 범위 벗어남. ( ~ {Count - 1})");
                return _array[index];
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = _count - 1; i >= 0; i--)
            {
                yield return _array[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public void Clear()
        {
            Array.Clear(_array, 0, _maxSize);
            _count = 0;
        }
        public bool Contains(T item)
        {
            if (item == null)
            {
                return _array.IsExists(x => x == null);
            }
            var equalityComparer = EqualityComparer<T>.Default;
            for (int i = _count - 1; i >= 0; --i)
            {
                if (equalityComparer.Equals(_array[i], item))
                    return true;
            }

            return false;
        }

        public T Peek()
        {
            if (!TryPeek(out var result))
                throw new InvalidOperationException("스택이 비어 있습니다.");
            return result;
        }

        public bool TryPeek(out T result)
        {
            if (_count == 0)
            {
                result = default;
                return false;
            }
            result = _array[_count - 1];
            return true;
        }

        public T Pop()
        {
            if (!TryPop(out var result))
                throw new InvalidOperationException("스택이 비어 있습니다.");
            return result;
        }

        public bool TryPop(out T result)
        {
            if (!TryPeek(out result))
            {
                return false;
            }
            --_count;
            _array[_count] = default;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns>inside limit Size</returns>
        public bool Push(T item)
        {
            if (_count < _maxSize)
            {
                _array[_count] = item;
                ++_count;
                return true;
            }
            else
            {
                Array.Copy(_array, 1, _array, 0, _maxSize - 1);
                _array[_maxSize - 1] = default;
                _array[_count - 1] = item;
                return false;
            }
        }

        //[Serializable]
        //public struct Enumerator : IEnumerator<T>
        //{
        //    private readonly StackLimited<T> _stack;
        //    private int _index;

        //    public T Current => _stack._array[_index];
        //    object IEnumerator.Current => Current;

        //    internal Enumerator(StackLimited<T> stack)
        //    {
        //        _stack = stack;
        //        _index = stack.Count;
        //    }

        //    public void Dispose() { }

        //    public bool MoveNext()
        //    {
        //        _index--;
        //        return _index >= 0;
        //    }

        //    public void Reset()
        //    {
        //        _index = _stack.Count;
        //    }
        //}
    }
}