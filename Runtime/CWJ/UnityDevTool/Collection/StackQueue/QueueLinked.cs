
using System;
using System.Collections;
using System.Collections.Generic;

namespace CWJ.Collection
{
    /// <summary>
    /// <see langword="Remove()"/>, 중복방지 옵션이 있는 Queue입니다.
    /// <br/> 인스펙터에 배열보임.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class QueueLinked<T> : BaseLinkedListCustom<T>
    {
        public QueueLinked(bool isPreventDuplication = false) : base(isPreventDuplication) { }

        public QueueLinked(IEnumerable<T> collection, bool isPreventDuplication = false) : base(collection, isPreventDuplication) { }


        public bool Enqueue(T item)
        {
            if (isPreventDuplication && !set.Add(item))
                return false;

            list.AddLast(item);
#if UNITY_EDITOR
            itemList.Add(item);
#endif
            return true;
        }

        public bool TryDequeue(out T result)
        {
            return TryPutOutMethod(out result);
        }

        protected override bool PutInMethod(T item) => Enqueue(item);
    }
}