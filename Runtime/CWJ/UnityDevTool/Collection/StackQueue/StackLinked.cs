using System;
using System.Collections;
using System.Collections.Generic;

namespace CWJ.Collection
{
    /// <summary>
    /// <see langword="Remove()"/>, 중복방지 옵션이 있는 Stack입니다.
    /// <br/> 인스펙터에 배열보임.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class StackLinked<T> : BaseLinkedListCustom<T>
    {
        public StackLinked(bool isPreventDuplication = false) : base(isPreventDuplication) { }

        public StackLinked(IEnumerable<T> collection, bool isPreventDuplication = false) : base(collection, isPreventDuplication) { }


        public bool Push(T item)
        {
            if (isPreventDuplication && !set.Add(item))
                return false;

            list.AddFirst(item);
#if UNITY_EDITOR
            itemList.Insert(0, item);
#endif
            return true;
        }

        public bool TryPop(out T result)
        {
            return TryPutOutMethod(out result);
        }

        protected override bool PutInMethod(T item) => Push(item);
    }
}
