using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using System;
using System.Collections.Generic;

namespace CWJ
{
    using static CWJ.ViewNameDefine;

    [System.Serializable, DisallowMultipleComponent]
    public abstract class UIViewCore : CWJ.MonoBehaviourCWJ_LazyOnEnable
    {
        [SerializeField, Readonly] protected ViewName _viewName = 0;
        [SerializeField, Readonly] protected bool isInit = false;
        public abstract ViewName viewName { get; }

        public static UIViewCore Get(ViewName viewName)
        {
            if (!UIViewManager.Instance.uiViewByNameDict.TryGetValue(viewName, out var view))
            {
                return null;
            }
            return view;
        }
        public static T Get<T>() where T : UIViewCore
        {
            Type t = typeof(T);
            var items = UIViewManager.allOfUIView
#if UNITY_EDITOR
                .FindAll
#else
                .FirstOrDefault
#endif
                (o => o.GetType().Equals(t));

#if UNITY_EDITOR
            if (items.Length > 1)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    Debug.LogError("UIView가 중복??? " + i, items[i]);
                }
            }
            return (T)items[0];
#else
            return (T)items;
#endif
        }


        public string displayTitleTxt = null;
        public bool useLogo = false;
        public bool useRootBackBtn = false;
        public CloseButtonState useRootHomeBtnType = CloseButtonState.None;
        protected bool isHidenAsValidRoute, isShownAsValidRoute;
        public enum CloseButtonState
        {
            None,
            Close,
            Home
        }

        public Button backButton = null;
        public Button closeButton = null;
        public Button homeButton = null;

#if UNITY_EDITOR
        protected virtual void Reset()
        {
            UpdateObjName();
        }

        protected virtual void OnValidate()
        {
            if (viewName != _viewName)
            {
                _viewName = viewName;
                UpdateObjName();
            }
        }

        protected void UpdateObjName()
        {
            this.gameObject.name = $"View - {viewName.ToString()}";
        }
#endif

        //protected void Awake()
        //{
        //    Init();
        //}



        public void Init()
        {
            if (isInit)
            {
                return;
            }
            isInit = true;
            if (_viewName != viewName)
            {
                Debug.LogError("종료하고 viewName을 확인해주세요", gameObject);
#if UNITY_EDITOR
                UnityEditor.Selection.activeGameObject = gameObject;
                CWJ.AccessibleEditor.AccessibleEditorUtil.PingObj(gameObject);
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }

            if (_viewName == RootName_BeforeLogin)
                UIStackManager.RootView_beforeLogin = this;
            else if (_viewName == RootName_AfterLogin)
                UIStackManager.RootView_afterLogin = this;
            else
            {
                if (backButton == null && closeButton == null && homeButton == null
                    && !useRootBackBtn && useRootHomeBtnType == CloseButtonState.None)
                {
                    Debug.LogError("?? dont have hide button !!", gameObject);
                    Debug.Break();
                    return;
                }
            }

            if (HasChildView)
            {
                childViewStacks = new();
            }

            if (backButton)
            {
                backButton.OnClickAsObservable().Subscribe(_ => OnClickBackBtn());
                if (useRootBackBtn) useRootBackBtn = false;
                backButton.gameObject.GetOrAddComponent<EscClickBtn>().enabled = true;
            }

            bool needEscClickComp = !backButton && !useRootBackBtn;
            if (closeButton)
            {
                closeButton.OnClickAsObservable().Subscribe(_ => OnClickCloseBtn());
                if (useRootHomeBtnType == CloseButtonState.Close) useRootHomeBtnType = CloseButtonState.None;
                if (needEscClickComp)
                    closeButton.gameObject.GetOrAddComponent<EscClickBtn>().enabled = true;
                else if (closeButton.gameObject.TryGetComponent<EscClickBtn>(out var escClick))
                    escClick.enabled = false;
            }
            if (homeButton)
            {
                if (closeButton)
                {
                    Debug.LogError("Home이랑 Close버튼 둘다있음!", gameObject);
                    Debug.Break();
                }
                homeButton.OnClickAsObservable().Subscribe(_ => OnClickHomeBtn());
                if (useRootHomeBtnType == CloseButtonState.Home) useRootHomeBtnType = CloseButtonState.None;
                if (needEscClickComp)
                    homeButton.gameObject.GetOrAddComponent<EscClickBtn>().enabled = true;
                else if (homeButton.gameObject.TryGetComponent<EscClickBtn>(out var escClick))
                    escClick.enabled = false;
            }
            _Init();
        }

        /// <summary>
        /// Start 대신 쓰면됨
        /// </summary>
        protected abstract void _Init();

        /// <summary>
        /// Start 대신 _Init쓰기.
        /// </summary>
        protected sealed override void _Start() { }




        readonly CompositeDisposable _eventDisposable = new();
        bool _isSubscribed = false;


        /// <summary>
        /// OnEnable 대신 OnShown 쓰기.
        /// </summary>
        protected sealed override void _OnEnable()
        {
            if (isShownAsValidRoute)
            {
                UIViewManager.Instance.InitRootPanel(this, _eventDisposable
                    , OnClickBackBtn, OnClickCloseBtn, OnClickHomeBtn);
                InitChildView();
                _isSubscribed = true;
                OnShow();
            }
        }

        protected virtual void WhenBeforeShow() { }
        protected void OnDisable()
        {
            if (MonoBehaviourEventHelper.IS_QUIT)
            {
                return;
            }
            if (isHidenAsValidRoute)
            {
                Unsubscribe();
                InitChildView();
                OnHide();
            }
            isHidenAsValidRoute = false;
            isShownAsValidRoute = false;
        }
        void Unsubscribe()
        {
            if (_isSubscribed)
            {
                _isSubscribed = false;
                _eventDisposable.Clear();
            }
        }

        [InvokeButton]
        public bool Show()
        {
            try
            {
                WhenBeforeShow();
            }
            catch
            {
                return false;
            }
            isShownAsValidRoute = true;
            if (gameObject.activeInHierarchy)
            {
                _OnEnable();
            }
            return UIStackManager.ShowView(this);
        }
        protected abstract void OnShow();



        protected virtual void WhenBeforeHide() { }

        /// <summary>
        /// UIStackManager말곤 직접부르는일 없도록하기
        /// <br/> Hide() -> UIStackManager.HideView( )쓰기
        /// </summary>
        [InvokeButton]
        public bool Hide()
        {
            try
            {
                WhenBeforeHide();
            }
            catch
            {
                return false;
            }

            isHidenAsValidRoute = true;
            if (!gameObject.activeInHierarchy)
            {
                OnDisable();
            }
            return UIStackManager.HideView(this);
        }
        protected abstract void OnHide();


        protected virtual void OnClickBackBtn()
        {
            Debug.LogError(viewName.ToString() + " OnClickRootBackBtn");
            if (HasChildView)
            {
                if (childViewStacks.Count <= 1)
                {
                    UIStackManager.BackToLastView();
                }
                else
                {
                    BackChildView();
                }
                return;
            }
            UIStackManager.BackToLastView();
        }
        protected virtual void OnClickCloseBtn()
        {
            // Debug.LogError(viewName.ToString() + " OnClickCloseBtn");
            UIStackManager.BackToLastView();
        }
        protected virtual void OnClickHomeBtn()
        {
            // Debug.LogError(viewName.ToString() + " OnClickHomeBtn");
            UIStackManager.GoBackToRootView();
        }

        public bool HasChildView => (FirstChildView);
        protected virtual Transform FirstChildView => null;
        [SerializeField, ShowConditional(nameof(HasChildView))] protected Transform curChildView;

        private Stack<Transform> childViewStacks;

        public virtual void InitChildView()
        {
            if (!HasChildView) return;

            // 모든 InnerView를 비활성화
            foreach (Transform child in FirstChildView.parent)
            {
                if (child != FirstChildView)
                {
                    child.gameObject.SetActive(false);
                }
            }
            childViewStacks.Clear();
            // 첫 번째 InnerView 활성화 및 스택에 추가
            curChildView = FirstChildView;
            curChildView.gameObject.SetActive(true);
            childViewStacks.Push(curChildView);

            if (backButton)
                backButton.gameObject.SetActive(false);
        }

        public virtual void BackChildView()
        {
            if (!HasChildView || childViewStacks.Count <= 1)
            {
                // 첫 번째 InnerView이면 더 이상 뒤로 갈 수 없음
                return;
            }

            // 현재 InnerView 비활성화 및 스택에서 제거
            Transform lastInnerView = childViewStacks.Pop();
            lastInnerView.gameObject.SetActive(false);

            // 이전 InnerView 활성화
            curChildView = childViewStacks.Peek();
            curChildView.gameObject.SetActive(true);

            if (backButton)
                backButton.gameObject.SetActive(childViewStacks.Count > 1);
        }

        protected virtual void OpenInnerView(Transform openInner)
        {
            if (!HasChildView) return;

            if (curChildView != null)
                curChildView.gameObject.SetActive(false);

            curChildView = openInner;
            openInner.gameObject.SetActive(true);
            childViewStacks.Push(openInner);
            if (backButton)
                backButton.gameObject.SetActive(FirstChildView != openInner);
        }

        protected virtual void CloseInnerView(Transform closeInner)
        {
            if (!HasChildView || !childViewStacks.Contains(closeInner))
            {
                return;
            }
            // 스택에서 해당 InnerView 제거
            Stack<Transform> tempStack = new Stack<Transform>();
            while (childViewStacks.Peek() != closeInner)
            {
                Transform tempView = childViewStacks.Pop();
                if (tempView)
                {
                    tempView.gameObject.SetActive(false);
                    tempStack.Push(tempView);
                }
            }

            // 닫을 InnerView 비활성화 및 제거
            curChildView.gameObject.SetActive(false);
            childViewStacks.Pop();

            // 이전 InnerView 복원
            if (childViewStacks.Count > 0)
            {
                curChildView = childViewStacks.Peek();
                curChildView.gameObject.SetActive(true);
            }
            else
            {
                curChildView = FirstChildView;
                curChildView.gameObject.SetActive(true);
            }

            // 임시 스택의 뷰들을 다시 스택에 추가
            while (tempStack.Count > 0)
            {
                childViewStacks.Push(tempStack.Pop());
            }

            if (backButton)
                backButton.gameObject.SetActive(childViewStacks.Count > 1);
        }

        protected override void OnDispose()
        {

        }
    }
}
