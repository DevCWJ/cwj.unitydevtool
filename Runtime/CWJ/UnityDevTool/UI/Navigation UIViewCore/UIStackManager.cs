using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using CWJ.Collection;

namespace CWJ
{
    public static class UIStackExtension
    {
        public static void SetActiveByBackMngr<T>(this T obj, bool isActive) where T : Component
        {
            if (isActive)
                UIStackManager.Show(obj.transform);
            else
                UIStackManager.Hide(obj.transform);
        }
    }


    public partial class UIStackManager : CWJ.Singleton.SingletonBehaviour<UIStackManager>
    {

        [VisualizeField] static StackLinked<Transform> _ObjStack = new(isPreventDuplication: true);

        public static int ExistsCount(Predicate<Transform> predicate)
        {
            return _ObjStack.CountPredicate(predicate);
        }

        public void HideElems(Predicate<Transform> predicate)
        {
            _ObjStack.Where(o => o != null && predicate(o)).Do(e => Hide(e));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trf"></param>
        /// <returns>false : 기존에 있엇음</returns>
        [InvokeButton]
        public static bool Show(Transform trf, bool checkUIViewChild = true)
        {
            if (!trf)
            {
                return false;
            }
            if (checkUIViewChild && trf.parent.GetComponentInParent<UIViewCore>())
            {
                Debug.LogError("Show Error (InnerView는 UIViewCore통해서만 관리되어야함 > " + trf.name, trf);
                return false;
            }
            Debug.Log("Show > ".SetColor(Color.green) + trf.name, trf);
            if (!trf.gameObject.activeSelf)
                trf.gameObject.SetActive(true);

            if (_ObjStack.Push(trf))
            {
                return true;
            }
            _ObjStack.Remove(trf);
            _ObjStack.Push(trf);
            return false;
        }

        /// <summary>
        /// 켜졌던적 없으면 false
        /// </summary>
        /// <param name="trf"></param>
        /// <param name="checkUIViewChild"></param>
        /// <returns></returns>
        [InvokeButton]
        public static bool Hide(Transform trf, bool checkUIViewChild = true)
        {
            if (!trf)
            {
                return false;
            }
            if (checkUIViewChild && trf.parent.GetComponentInParent<UIViewCore>())
            {
                Debug.LogError("Hide Error (InnerView는 UIViewCore통해서만 관리되어야함 > " + trf.name, trf);
                return false;
            }
            Debug.Log("Hide > ".SetColor(Color.red) + trf.name, trf);

            if (trf.gameObject.activeSelf)
                trf.gameObject.SetActive(false);

            return _ObjStack.Remove(trf);
        }

        //public bool TryPeek(out Transform lastObj)
        //{
        //    return _ObjStack.TryPeek(out lastObj);
        //}

        //public bool TryPop( out Transform lastObj)
        //{
        //    return _ObjStack.TryPop(out lastObj);
        //}

        

        //public void OnlyTargetActive(bool isActive, params Transform[] targets)
        //{
        //    _ObjStack.Where(item => item != null && targets.Contains(item)).Do(t => t.gameObject.SetActive(isActive));
        //}


        /// <summary>
        /// Target까지 Hide. 없으면 Show
        /// </summary>
        /// <param name="target"></param>
        //public void BackWhileTarget(Transform target, bool isHideWithTarget = true)
        //{
        //    while (_ObjStack.TryPeek(out var peekObj))
        //    {
        //        if (peekObj == target)
        //        {
        //            if (isHideWithTarget)
        //                Hide(target);
        //            return;
        //        }
        //        Hide(peekObj);
        //    }
        //    if (!isHideWithTarget)
        //        Show(target);
        //    else
        //        Hide(target);
        //}

        //public void TargetActive(bool isActive, params Transform[] targets)
        //{
        //    int targetLen = targets == null ? 0 : targets.Length;
        //    int listLen = _ObjStack.Count;

        //    int inspectionCompeteNumber = 0;

        //    for (var i = 0; i < listLen; ++i)
        //    {
        //        bool isFind = false;

        //        for (var j = inspectionCompeteNumber; j < targetLen; ++j)
        //        {
        //            if ((_ObjStack[i] == targets[j]))
        //            {
        //                (targets[j], targets[inspectionCompeteNumber]) = (targets[inspectionCompeteNumber], targets[j]);

        //                ++inspectionCompeteNumber;

        //                isFind = true;
        //                continue;
        //            }
        //        }

        //        bool isActiveInspection = (targetLen == 0 || isFind) ? isActive : !isActive;

        //        _ObjStack[i].gameObject.SetActive(isActiveInspection);
        //    }
        //}
    }

}