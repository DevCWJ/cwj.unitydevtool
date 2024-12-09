using System;
using System.Collections;
using System.Collections.Generic;

namespace CWJ.Collection
{
    [Serializable]
    public class QueueList<T> : IEnumerable<T>
    {
#if UNITY_EDITOR
        [UnityEngine.SerializeField, Readonly]
#else
        readonly 
#endif
        bool isPreventDuplication;
#if UNITY_EDITOR
        [UnityEngine.SerializeField, Readonly]
#else
        readonly 
#endif
        List<T> list;

        private readonly HashSet<T> hashSet;

        public T this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }

        public QueueList(bool isPreventDuplication = false)
        {
            list = new List<T>();
            this.isPreventDuplication = isPreventDuplication;
            if (isPreventDuplication)
                hashSet = new HashSet<T>();
        }

        public QueueList(int capacity, bool isPreventDuplication = false)
        {
            list = new List<T>(capacity);
            this.isPreventDuplication = isPreventDuplication;
            if (isPreventDuplication)
                hashSet = new HashSet<T>(capacity);
        }

        public int Count => list.Count;

        public bool Enqueue(T item)
        {
            if (isPreventDuplication)
            {
                if (!hashSet.Add(item))
                    return false; // 이미 존재하는 경우 추가하지 않음
            }
            list.Add(item);
            return true;
        }

        public bool TryDequeue(out T result)
        {
            if (!TryPeek(out result))
            {
                return false;
            }

            list.RemoveAt(0);

            if (isPreventDuplication)
                hashSet.Remove(result);
            return true;
        }

        public bool TryPeek(out T result)
        {
            if (list.Count == 0)
            {
                result = default;
                return false;
            }

            result = list[0];
            return true;
        }

        public bool Remove(T item)
        {
            bool removed = list.Remove(item);
            if (removed && isPreventDuplication)
                hashSet.Remove(item);
            return removed;
        }
        public bool RemoveAll(T item)
        {
            if (isPreventDuplication)
            {
                return Remove(item);
            }
            return list.RemoveAll(e => e != null && e.Equals(item)) > 0;
        }
        public void Clear()
        {
            list.Clear();
            if (isPreventDuplication)
                hashSet.Clear();
        }

        public bool Contains(T item)
        {
            return isPreventDuplication ? hashSet.Contains(item) : list.Contains(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            // 큐의 앞쪽부터시작
            for (int i = 0; i < list.Count; ++i)
            {
                yield return list[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}