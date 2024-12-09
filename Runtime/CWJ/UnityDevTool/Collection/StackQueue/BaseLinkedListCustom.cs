using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CWJ.Collection
{
    [System.Serializable]
    public abstract class BaseLinkedListCustom<T> : IEnumerable<T>
    {
#if UNITY_EDITOR
        [UnityEngine.SerializeField, Readonly] protected List<T> itemList;
        [UnityEngine.SerializeField, Readonly]
#endif
        protected bool isPreventDuplication;

        protected readonly LinkedList<T> list;
        protected readonly HashSet<T> set;
        public int Count => list.Count;

        public BaseLinkedListCustom(bool isPreventDuplication)
        {
            list = new();
            this.isPreventDuplication = isPreventDuplication;
            if (isPreventDuplication)
                set = new();
#if UNITY_EDITOR
            itemList = new();
#endif
        }

        public BaseLinkedListCustom(IEnumerable<T> collection, bool isPreventDuplication)
        {
            list = new();
            this.isPreventDuplication = isPreventDuplication;
            if (isPreventDuplication)
                set = new();
#if UNITY_EDITOR
            itemList = new();
#endif
            foreach (var item in collection)
            {
                PutInMethod(item); // 중복 방지 로직을 적용하여 초기화
            }
        }

        protected abstract bool PutInMethod(T item);

        protected bool TryPutOutMethod(out T result)
        {
            if (TryPeek(out result))
            {
                list.RemoveFirst();
                if (isPreventDuplication)
                {
                    set.Remove(result);
                }
#if UNITY_EDITOR
                itemList.RemoveAt(0);
#endif
                return true;
            }
            return false;
        }

        public bool TryPeek(out T result)
        {
            if (list.Count == 0)
            {
                result = default;
                return false;
            }
            result = list.First.Value;
            return true;
        }

        public T Peek()
        {
            if (!TryPeek(out T result))
                throw new InvalidOperationException("리스트가 비어 있습니다.");
            return result;
        }

        public bool Remove(T item)
        {
#if UNITY_EDITOR
            itemList.Remove(item);
#endif
            if (isPreventDuplication)
            {
                if (!set.Remove(item))
                    return false;
                bool removedFromList = list.Remove(item);
#if UNITY_EDITOR
                UnityEngine.Debug.Assert(removedFromList, "HashSet에는 있지만 LinkedList에는 없는 아이템이 발견되었습니다. : " + item.ToString());
#endif
                return removedFromList;
            }
            else
            {
                return list.Remove(item);
            }
        }

        public bool Contains(T value)
        {
            return isPreventDuplication ? set.Contains(value) : list.Contains(value);
        }
        public void Clear()
        {
            list.Clear();
            if (isPreventDuplication)
                set.Clear();
#if UNITY_EDITOR
            itemList.Clear();
#endif
        }
        public int CountPredicate(Predicate<T> predicate)
        {
            if (predicate == null)
            {
                return Count;
            }
            else
            {
                if (isPreventDuplication)
                    return set.Count((e) => predicate(e));
                return list.Count(e => predicate(e));
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in list)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
