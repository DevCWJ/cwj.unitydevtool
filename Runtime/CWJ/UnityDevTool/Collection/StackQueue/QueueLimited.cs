using System;
using System.Collections;
using System.Collections.Generic;

namespace CWJ.Collection
{
    [Serializable]
    public class QueueLimited<T> : IEnumerable<T>
    {
#if UNITY_EDITOR
        [UnityEngine.SerializeField, Readonly]
#endif
        private readonly int _maxSize;
#if UNITY_EDITOR
        [UnityEngine.SerializeField, Readonly]
#endif
        private int _count;
#if UNITY_EDITOR
        [UnityEngine.SerializeField, Readonly]
#endif
        private int _head;
#if UNITY_EDITOR
        [UnityEngine.SerializeField, Readonly]
#endif
        private int _tail;
#if UNITY_EDITOR
        [UnityEngine.SerializeField, Readonly]
#endif
        private T[] _array;

        public QueueLimited(int maxSize)
        {
            if (maxSize <= 0)
                throw new ArgumentOutOfRangeException("maxSize는 0보다 커야 합니다.");
            _maxSize = maxSize;
            _array = new T[_maxSize];
            _count = 0;
            _head = 0;
            _tail = 0;
        }

        public int Count => _count;
        public int MaxSize => _maxSize;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new IndexOutOfRangeException($"인덱스 {index}는 유효한 범위가 아닙니다. (0 ~ {_count - 1})");
                int actualIndex = (_head + index) % _maxSize;
                return _array[actualIndex];
            }
        }

        public void Clear()
        {
            if (_count > 0)
            {
                if (_head < _tail)
                {
                    Array.Clear(_array, _head, _count);
                }
                else
                {
                    Array.Clear(_array, _head, _maxSize - _head);
                    Array.Clear(_array, 0, _tail);
                }
            }
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        public bool Contains(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            int idx = _head;
            for (int i = 0; i < _count; i++)
            {
                if (comparer.Equals(_array[idx], item))
                    return true;
                idx = (idx + 1) % _maxSize;
            }
            return false;
        }

        public T Peek()
        {
            if (!TryPeek(out var result))
                throw new InvalidOperationException("큐가 비어 있습니다.");
            return result;
        }

        public bool TryPeek(out T result)
        {
            if (_count == 0)
            {
                result = default;
                return false;
            }
            result = _array[_head];
            return true;
        }

        public T Dequeue()
        {
            if (!TryDequeue(out var result))
                throw new InvalidOperationException("큐가 비어 있습니다.");
            return result;
        }

        public bool TryDequeue(out T result)
        {
            if (_count == 0)
            {
                result = default;
                return false;
            }
            result = _array[_head];
            _array[_head] = default;
            _head = (_head + 1) % _maxSize;
            _count--;
            return true;
        }

        public void Enqueue(T item)
        {
            if (_count == _maxSize)
            {
                // 큐가 가득 찼을 때, 가장 오래된 요소를 덮어씁니다.
                _array[_tail] = item;
                _head = (_head + 1) % _maxSize;
                _tail = (_tail + 1) % _maxSize;
            }
            else
            {
                _array[_tail] = item;
                _tail = (_tail + 1) % _maxSize;
                _count++;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            int idx = _head;
            for (int i = 0; i < _count; i++)
            {
                yield return _array[idx];
                idx = (idx + 1) % _maxSize;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}