using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

namespace CWJ
{
    using static CWJ.ViewNameDefine;

    public class UIViewManager : CWJ.Singleton.SingletonBehaviour<UIViewManager>
    {
        [Readonly]
        public CWJ.Collection.OLD_DictionaryVisualized<ViewName, UIViewCore> uiViewByNameDict = null;
        public static UIViewCore[] allOfUIView;
        [DrawHeaderAndLine("Top")]
        [SerializeField] Transform topTrf;
        [SerializeField] GameObject logoObj;
        public Button rootHomeBtn;
        public Button rootCloseBtn;
        [DrawHeaderAndLine("Header")]
        [SerializeField] Transform headerTrf;
        [SerializeField] TextMeshProUGUI panelTitleTxt;
        public Button rootBackBtn;

        public void InitRootPanel(UIViewCore viewItem, CompositeDisposable itemEventDisposable,
            Action onClickBack, Action onClickClose, Action onClickHome)
        {
            bool useRootBackBtn = viewItem.useRootBackBtn;
            if (useRootBackBtn)
            {
                rootBackBtn.OnClickAsObservable().Subscribe(_ => onClickBack.Invoke()).AddTo(itemEventDisposable);
                rootBackBtn.gameObject.GetOrAddComponent<EscClickBtn>().enabled = true;
            }

            bool useRootCloseBtn = viewItem.useRootHomeBtnType == UIViewCore.CloseButtonState.Close;
            if (useRootCloseBtn)
            {
                rootCloseBtn.OnClickAsObservable().Subscribe(_ => onClickClose.Invoke()).AddTo(itemEventDisposable);
                rootCloseBtn.gameObject.GetOrAddComponent<EscClickBtn>().enabled = !useRootBackBtn;
            }

            bool useRootHomeBtn = viewItem.useRootHomeBtnType == UIViewCore.CloseButtonState.Home;
            if (useRootHomeBtn)
            {
                rootHomeBtn.OnClickAsObservable().Subscribe(_ => onClickHome.Invoke()).AddTo(itemEventDisposable);
                rootHomeBtn.gameObject.GetOrAddComponent<EscClickBtn>().enabled = !useRootBackBtn;
            }

            rootCloseBtn.gameObject.SetActive(useRootCloseBtn);
            rootHomeBtn.gameObject.SetActive(useRootHomeBtn);
            topTrf.gameObject.SetActive(viewItem.useLogo || useRootHomeBtn || useRootCloseBtn);

            rootBackBtn.gameObject.SetActive(useRootBackBtn);
            bool hasTitle = !string.IsNullOrEmpty(viewItem.displayTitleTxt);
            panelTitleTxt.gameObject.SetActive(hasTitle);
            panelTitleTxt.SetText(hasTitle ? viewItem.displayTitleTxt : string.Empty);
            headerTrf.gameObject.SetActive(useRootBackBtn || hasTitle);

            bool isActiveRootObj = useRootBackBtn || useRootHomeBtn || useRootCloseBtn;

            if (isActiveRootObj)
            {
                int viewItemIndex = viewItem.transform.GetSiblingIndex();
                int rootTrfIndex = topTrf.parent.GetSiblingIndex();
                if (viewItemIndex > rootTrfIndex)
                    topTrf.parent.SetSiblingIndex(viewItemIndex);

                //UiViewManager에서 root버튼관련 이벤트를 모두 할당하려했지만
                //그냥 UIViewCore에서 만들어놓은 virtual 메소드 통해 기본기능을 할당하고,
                //원하는 View에 따라선 override받아 기능을 수정할수있는게 낫다 판단함.
                //Button toHideBtn, customBackBtn;
                //if (useRootBackBtn)
                //{
                //    if (viewItem.useRootHomeBtnType != UIViewCore.CloseButtonState.None)
                //    {
                //        toHideBtn = useRootCloseBtn ? rootCloseBtn : rootHomeBtn;
                //        customBackBtn = rootBackBtn;
                //    }
                //    else
                //    {
                //        if (viewItem.closeButton || viewItem.homeButton)
                //        {
                //            toHideBtn = viewItem.closeButton ? viewItem.closeButton : viewItem.homeButton;
                //            customBackBtn = rootBackBtn;
                //        }
                //        else
                //        {
                //            toHideBtn = rootBackBtn;
                //            customBackBtn = null;
                //        }
                //    }
                //}
                //else
                //{
                //    toHideBtn = (useRootCloseBtn ? rootCloseBtn : rootHomeBtn);
                //    customBackBtn = null;
                //}
                //bool hasCustomBackBtn = customBackBtn != null;
                //if (hasCustomBackBtn)
                //{
                //    customBackBtn.gameObject.GetOrAddComponent<EscClickBtn>().enabled = true;
                //    customBackBtn.OnClickAsObservable().Subscribe(_ => viewItem.BackChildView()).AddTo(itemEventDisposable);
                //}
                //toHideBtn.gameObject.GetOrAddComponent<EscClickBtn>().enabled = !hasCustomBackBtn;
                //toHideBtn.OnClickAsObservable().Subscribe(_ => UIStackManager.BackToLastView()).AddTo(itemEventDisposable);
            }

            topTrf.parent.gameObject.SetActive(isActiveRootObj);
            //logoObj.SetActive(viewItem.useLogo);
        }




        /// <summary>
        /// 추후 Undo 추가하기
        /// </summary>
        /// <param name="column"></param>
        /// <param name="margin"></param>
        [InvokeButton]
        void Editor_UnfoldView(int column, float margin =10)
        {
            var uiViews = FindObjectsOfType<UIViewCore>(true).OrderBy(u => u.viewName.ToInt()).ToArray();
            if (uiViews.Length == 0)
            {
                return;
            }
            if (column <= 0)
                column = 1;
            var rect = uiViews[0].GetComponent<RectTransform>().rect;
            float x = 0, y = 0;
            float w = rect.width + margin;
            float h = -1 * (rect.height + margin);
            int index = 0;
            foreach (var uiView in uiViews)
            {
                x += w;
                if (++index % column == 0)
                {
                    y += h;
                    x = 0;
                }
                uiView.transform.localPosition = new Vector3(x, y, uiView.transform.localPosition.z);
                uiView.gameObject.SetActive(true);
            }
        }

        protected override void _Awake()
        {
            allOfUIView = FindObjectsOfType<UIViewCore>(true);
            uiViewByNameDict = new CWJ.Collection.OLD_DictionaryVisualized<ViewName, UIViewCore>();
            foreach (var item in allOfUIView)
            {
                if (uiViewByNameDict.TryGetValue(item.viewName, out var overlapView))
                {
                    Debug.LogError($"{nameof(ViewName)} 겹침!! {item.GetType().Name} <-> {uiViewByNameDict[item.viewName].GetType().Name}");
#if UNITY_EDITOR
                    UnityEditor.Selection.objects = new GameObject[2] { overlapView.gameObject, item.gameObject };
                    Debug.Break();
#endif
                    return;
                }
                uiViewByNameDict.Add(item.viewName, item);
                item.transform.localPosition = Vector3.zero;
                if (item.backButton == rootBackBtn)
                {
                    item.backButton = null;
                    item.useRootBackBtn = true;
                }
                if(item.closeButton == rootCloseBtn)
                {
                    item.closeButton = null;
                    item.useRootHomeBtnType = UIViewCore.CloseButtonState.Close;
                }
                if(item.homeButton == rootHomeBtn)
                {
                    item.homeButton = null;
                    item.useRootHomeBtnType = UIViewCore.CloseButtonState.Home;
                }
                item.Init();
                item.gameObject.SetActive(false);
                item.InitChildView();
                //item.Hide();
                if (item.backButton != null)
                {
                    if (item.closeButton != null)
                    {
                        _SubscribeHideBtn(item.closeButton, false); //둘다 있을땐 backButton을 특별한 목적의 버튼이라 생각하고, closeButton에만 창닫기를 넣음
                        item.backButton.gameObject.GetOrAddComponent<EscClickBtn>();
                    }
                    else if (item.useRootHomeBtnType != UIViewCore.CloseButtonState.Close) //closeButton는 없는 view
                    {
                        _SubscribeHideBtn(item.backButton, true);
                    }
                }
                _SubscribeHomeBtn(item.homeButton);
            }

            _SubscribeHomeBtn(rootHomeBtn);

            void _SubscribeHideBtn(Button backBtn, bool withReactEscInput)
            {
                if (backBtn == null) return;
                if (withReactEscInput)
                    backBtn.gameObject.GetOrAddComponent<EscClickBtn>();
                backBtn.OnClickAsObservable().Subscribe(_ =>
                {
                    UIStackManager.BackToLastView();
                });
            }
            void _SubscribeHomeBtn(Button homeBtn)
            {
                if (homeBtn == null) return;
                homeBtn.OnClickAsObservable().Subscribe(_ =>
                {
                    UIStackManager.GoBackToRootView();
                });
            }

            UIStackManager.SetRootAndAllHide(RootName_BeforeLogin);
        }

        //public void Danger_SetRoot(UIViewCore viewItem)
        //{
        //    if (CurRootView &&
        //        CurRootView.gameObject.TryGetComponent<BackToQuit>(out var btq))
        //    {
        //        btq.enabled = false;
        //    }
        //    viewItem.gameObject.GetOrAddComponent<BackToQuit>().enabled = true;
        //    CurRootView = viewItem;
        //    UIStackManager.GoTo(viewItem);
        //    Debug.Log("Root View : " + viewItem.gameObject.name, viewItem);
        //}

        ///// <summary>
        ///// 설정된 rootView외의 다른 ViewCore는 비활성화
        ///// </summary>
        ///// <param name="rootViewName"></param>
        //public void SetRootAndAllHide(ViewName rootViewName)
        //{
        //    // _UIViewCoreStack에 있는 모든 오브젝트 Hide불러준후 
        //    // _SetRoot(rootViewName) 필요 
        //    foreach (var uiView in uiViewByNameDict.Values)
        //    {   
        //        if (uiView.viewName.Equals(rootViewName))
        //            Danger_SetRoot(uiView);
        //        else
        //            uiView.Hide();
        //    }
        //}
    }
}