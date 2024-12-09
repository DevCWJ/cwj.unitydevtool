
using UnityEngine;
using CWJ.Collection;
using System.Collections.Generic;
using System.Linq;

namespace CWJ
{
    using static CWJ.ViewNameDefine;
    public partial class UIStackManager
    {
        [VisualizeField]
        static StackLinked<UIViewCore> _ViewCoreStack = new(isPreventDuplication: true);
#if UNITY_EDITOR
        internal static StackLinked<UIViewCore> Editor_uiViewStack => _ViewCoreStack;
#endif
        // UIViewCore 전용 스택을 이용하여 CurrentView 구현
        public static UIViewCore CurrentView => _ViewCoreStack.TryPeek(out var view) ? view : null;

        private static bool CanPop
        {
            get
            {
                if (CurRootView)
                    return _ViewCoreStack.Count > 1;
                else
                    return _ViewCoreStack.Count > 0;
            }
        }
        public static UIViewCore CurRootView { get; private set; } = null;
        public static UIViewCore RootView_beforeLogin { get; set; }
        public static UIViewCore RootView_afterLogin { get; set; }
        public static void SetRootAndAllHide(UIViewCore newRootView)
        {
            if (CurrentView)
                CurrentView.Hide();

            if (CurRootView &&
                CurRootView.gameObject.TryGetComponent<BackToQuit>(out var btq))
            {
                btq.enabled = false;
            }
            CurRootView = null;

            // 모든 UIViewCore 숨김
            foreach (var uiView in UIViewManager.Instance.uiViewByNameDict.Values)
            {
                HideView(uiView);
            }

            _ViewCoreStack.Clear();

            // RootView 설정
            newRootView.gameObject.GetOrAddComponent<BackToQuit>().enabled = true;
            CurRootView = newRootView;
            CurRootView.Show();

            Debug.Log("Root View : " + newRootView.gameObject.name, newRootView);
        }


        public static void SetRootAndAllHide(ViewName rootViewName)
        {
            if (!UIViewManager.Instance.uiViewByNameDict.TryGetValue(rootViewName, out UIViewCore rootView) || !rootView)
            {
                Debug.LogError($"Cannot find the RootView [{rootViewName}]. Check the viewName Please.");
                return;
            }

            SetRootAndAllHide(rootView);
        }

        public static bool ShowView(UIViewCore uiView)
        {
            if (!uiView)
            {
                return false;
            }

            // UIViewCore인 경우 _UIViewCoreStack에 추가
            if (_ViewCoreStack.Push(uiView))
            {
                Debug.Log("Show >".SetColor(Color.green) + uiView.name, uiView);
            }
            else
            {
                if (uiView == CurRootView)
                {
                    return Show(CurRootView.transform, false);
                }
                _ViewCoreStack.Remove(uiView);
                _ViewCoreStack.Push(uiView);
            }

            // 기존 _ObjStack 로직 유지
            return Show(uiView.transform, false);
        }

        public static bool HideView(UIViewCore uIView)
        {
            if (!uIView)
            {
                return false;
            }

            // UIViewCore인 경우 _UIViewCoreStack에서 제거
            if (uIView != CurRootView)
                _ViewCoreStack.Remove(uIView);

            return Hide(uIView.transform, false);
        }

        public static UIViewCore GoToView(ViewName viewName, bool stackOpen = false)
        {
            if (!UIViewManager.Instance.uiViewByNameDict.TryGetValue(viewName, out UIViewCore nextView) || !nextView)
            {
                Debug.LogError($"Cannot find the viewName [{viewName}]. Check the viewName Please.");
                return CurrentView;
            }
            Debug.Log("GoTo > " + viewName.ToString_Fast());
            return GoToView(nextView, stackOpen);
        }

        public static UIViewCore GoToView(UIViewCore nextView, bool stackOpen = false)
        {
            if (!nextView)
            {
                Debug.LogError($"Cannot find the view");
                return CurrentView;
            }
            var lastView = CurrentView;
            if (lastView)
            {
                lastView.Hide();
                if (stackOpen)
                {
                    _ViewCoreStack.Push(lastView);
                    _ObjStack.Push(lastView.transform);
                }
            }
            nextView.Show();

            return nextView;
        }


        public static UIViewCore BackToLastView()
        {
            Debug.LogError("BackToLastView");
            if (!CanPop)
            {
                Debug.LogError("You try to pop the RootView!");
                return null;
            }
            var lastView = CurrentView;
            if (!lastView)
            {
                Debug.LogError("No current view to go back from.");
                return null;
            }

            //현재 View Hide
            lastView.Hide();

            //현재View닫은후 새로운 currentView
            var previousView = CurrentView;

            if (previousView)
            {
                previousView.Show();
                return previousView;
            }
            else
            {
                Debug.LogError("No previous view found.");
                return null;
            }
        }

        /// <summary>
        /// Pop until (CurrentView.viewName == viewName) Or (CurrentView.isRootView)
        /// <br/><br/>
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns>Last Popped View</returns>
        public static UIViewCore BackToView(ViewName viewName)
        {
            if (!CanPop)
            {
                Debug.LogError("You try to pop the RootView!");
                return null;
            }
            if (CurrentView)
                CurrentView.Hide();

            UIViewCore lastView = null;
            while (CanPop && _ViewCoreStack.TryPop(out lastView))
            {
                if (lastView.viewName == viewName)
                {
                    break;
                }
                HideView(lastView);
            }

            if (lastView)
            {
                lastView.Show();
                return lastView;
            }
            else
            {
                return GoToView(viewName);
            }
        }

        public static void GoBackToRootView()
        {
            if (!CanPop)
            {
                Debug.LogError("You try to pop the RootView!");
                if (CurRootView)
                    CurRootView.Show();
                return;
            }
            if (CurrentView)
                CurrentView.Hide();

            while (CanPop && _ViewCoreStack.TryPop(out var lastPopView))
            {
                HideView(lastPopView);
            }

            if (CurRootView)
                CurRootView.Show();
        }

    }
}
