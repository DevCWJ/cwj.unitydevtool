#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;

using UnityEditor;

using UnityEngine;
using UnityEngine.UI;
using CWJ.AccessibleEditor;

namespace CWJ
{
    public class UIAutoScaleEditor : MonoBehaviour, CWJ.AccessibleEditor.InspectorHandler.ISelectHandler, CWJ.AccessibleEditor.InspectorHandler.ICompiledHandler
    {
        [SerializeField, GetComponentInParent, ErrorIfNull] CanvasScaler canvasScaler;
        [DrawHeaderAndLine("Resolution Setting")]
        [SerializeField] bool isFigmaConvertedUI;
        [SerializeField] Vector2 sourceResolution = new(360, 800);
        [SerializeField] Vector2 destinationResolution = new(1080, 1920);
        [SerializeField, Range(0, 1)] float matchWidthOrHeight = 0.5f;

        [DrawHeaderAndLine("UI Element Setting")]

        [SerializeField, ShowConditional(nameof(isFigmaConvertedUI))] float imagePixelsPerUnitMultiplier = 1.5f; //or 0.75f

        [SerializeField, ShowConditional(nameof(isFigmaConvertedUI))] bool isVerticalLayoutItemExpandWidth = false;
        [SerializeField, ShowConditional(nameof(isFigmaConvertedUI))] bool isNeedVerticalLayoutContentSizeFitter = true;

        [SerializeField, ShowConditional(nameof(isFigmaConvertedUI))] bool isHorizontalLayoutItemExpandHeight = false;
        [SerializeField, ShowConditional(nameof(isFigmaConvertedUI))] bool isNeedHorizontalLayoutContentSizeFitter = true;
        [DrawHeaderAndLine("실행 종류 두가지")]

        [DrawHeaderAndLine("1. 부모오브젝트 하나만 넣거나")]
        [CWJ.HelpBox(
            "PutOnTargetOfRoot에는 Canvas(최상단)오브젝트 넣을수있음" +
            "\nPanel(배경화면을 가득채울 오브젝트)이 " +
            "\n될 부모오브젝트 또한 가능.\n" +
            "\nCanvas오브젝트 제외한 해당 오브젝트의 직속 자식오브젝트들을" +
            "\nPanel로 취급해 Anchor를 Stretch FULL로 설정함.")]
        [SerializeField] RectTransform PutOnTargetOfRoot;
        [SerializeField, Readonly] RectTransform last_PutOnTargetOfRoot = null;


        [DrawHeaderAndLine("2. 배열에 각각 추가하고 Invoke Button")]
        [CWJ.HelpBox(
            "targetObjs 배열에는 Canvas(최상단)오브젝트 못넣음" +
            "\nPanel(배경화면을 가득채울 오브젝트)이 " +
            "\n될 부모오브젝트들만 추가가능.\n" +
            "\n배열의 오브젝트들을 " +
            "\nPanel로 취급해 Anchor를 Stretch FULL로 설정함.")]
        [SerializeField] RectTransform[] targetObjs;

        private void OnValidate()
        {
            if (PutOnTargetOfRoot != null)
            {
                last_PutOnTargetOfRoot = PutOnTargetOfRoot;
                PutOnTargetOfRoot = null;

                List<RectTransform> children = new();
                if (canvasScaler.transform == last_PutOnTargetOfRoot.transform
                    || last_PutOnTargetOfRoot.GetComponent<Canvas>() != null)
                {
                    foreach (RectTransform child in last_PutOnTargetOfRoot)
                    {
                        children.Add(child);
                    }
                }
                else
                {
                    children.Add(last_PutOnTargetOfRoot);
                }
                targetObjs = children.ToArray();
                if (!StartAutoScaling())
                {
                    last_PutOnTargetOfRoot = null;
                }
            }
        }

        private void Reset()
        {
            Init();
        }

        [InvokeButton]
        Vector2 GetGameViewSize()
        {
            return GameViewUtil.GetCurGameViewSize();
        }

        [InvokeButton]
        public static void SetGameViewSize(Vector2 size)
        {
            GameViewUtil.SetGameViewSize(size, "Custom Temp");
            //var gameViewType = System.Type.GetType("UnityEditor.GameView, UnityEditor");
            //var gameViewWindow = EditorWindow.GetWindow(gameViewType);

            //if (gameViewType == null)
            //{
            //    Debug.LogError("Failed to get GameView type.");
            //    return;
            //}

            //var setCustomResolutionMethod = gameViewType.GetMethod("SetCustomResolution", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            //if (setCustomResolutionMethod == null)
            //{
            //    var methods = gameViewType.GetMethods();
            //    foreach (var method in methods)
            //    {
            //        Debug.Log(method.Name);
            //    }
            //    Debug.LogError("Failed to get SetCustomResolution method.");
            //    return;
            //}

            //setCustomResolutionMethod.Invoke(gameViewWindow, new object[] { size, "Custom Size" });
            //gameViewWindow.Repaint();
            //https://github.com/Unity-Technologies/UnityCsReference/tree/master
            //위에 git링크보고 하고있엇느넫 Unity 6000.0.22f1 버전인듯 ㅅㅂ
        }




        void Init(bool hasLog = true)
        {
            this.hideFlags = HideFlags.DontSaveInBuild;

            sourceResolution = GetGameViewSize();
            if (hasLog)
                Debug.Log($"현재 UI 기준 해상도(GameView) : {sourceResolution.x}x{sourceResolution.y}");

            if (canvasScaler == null)
            {
                canvasScaler = GetComponentInParent<CanvasScaler>(true);
            }
            if (canvasScaler != null)
            {
                if (canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize ||
                    canvasScaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
                {
                    if (hasLog)
                    {
                        CWJ.AccessibleEditor.DisplayDialogUtil.DisplayDialog<UIAutoScaleEditor>("CanvasScaler의 설정이 올바르지 않습니다." +
                        "\nuiScaleMode는 ScaleWithScreenSize여야 하고, screenMatchMode는 MatchWidthOrHeight여야 합니다.", logObj: this, isError: true);
                    }
                    return;
                }

                destinationResolution = canvasScaler.referenceResolution;
                matchWidthOrHeight = canvasScaler.matchWidthOrHeight;
                if (hasLog)
                    Debug.Log($"현재 목표 해상도(CanvasScaler): {destinationResolution.x}x{destinationResolution.y}");
            }
        }

        static int UndoCurGroupIndex;
        static RectTransform CurScalingTarget;

        [InvokeButton]
        public bool StartAutoScaling()
        {
            if (targetObjs.Length == 0 || targetObjs[0] == null)
            {
                CWJ.AccessibleEditor.DisplayDialogUtil.DisplayDialog<UIAutoScaleEditor>("Target Parents가 지정되지 않았음.", logObj: this, isError: true);
                return false;
            }

            Init();

            if (canvasScaler == null)
            {
                CWJ.AccessibleEditor.DisplayDialogUtil.DisplayDialog<UIAutoScaleEditor>("Canvas Scaler 가 없었음.\nCanvas에 CanvasScaler 추가후 다시 시도해야함.", logObj: this, isError: true);
                return false;
            }

            if (!canvasScaler.enabled)
            {
                CWJ.AccessibleEditor.DisplayDialogUtil.DisplayDialog<UIAutoScaleEditor>("Canvas Scaler가 비활성화되어 있습니다.\nCanvas Scaler를 활성화한 후 다시 시도해야함.", logObj: this, isError: false);
                canvasScaler.enabled = true;
                AssetDatabase.Refresh();
                return false;
            }

            if (isFigmaConvertedUI)
            {
                var panelSize = targetObjs[0].sizeDelta;
                if(panelSize.x!= sourceResolution.x ||
                    panelSize.y!= sourceResolution.y)
                {
                    string gameViewSizeStr = sourceResolution.ToStringByDetailed();
                    string panelSizeStr = panelSize.ToStringByDetailed();
                    if (CWJ.AccessibleEditor.DisplayDialogUtil.DisplayDialog<UIAutoScaleEditor>(
                        $"현재 UI Panel 크기 :       {panelSizeStr}\n" +
                        $"현재 GameView 해상도 : {gameViewSizeStr}\n" +
                        $"\n\n둘중 어느것이 UI 작업 기준 해상도였나요?", logObj: this, isError: false
                        , ok: panelSizeStr, cancel: gameViewSizeStr))
                    {
                        sourceResolution = panelSize;
                        SetGameViewSize(sourceResolution);
                    }
                }
            }

            if (sourceResolution.x >= destinationResolution.x || sourceResolution.y >= destinationResolution.y)
            {
                CWJ.AccessibleEditor.DisplayDialogUtil.DisplayDialog<UIAutoScaleEditor>("CanvasScaler - Reference Resolution \n또는 GameView 해상도 확인하기", logObj: this, isError: true);
                return false;
            }

            if(!CWJ.AccessibleEditor.DisplayDialogUtil.DisplayDialog<UIAutoScaleEditor>(
                $"Figma Converter로 만들어진 UI? : {isFigmaConvertedUI}\n" +
                $"UI작업 기준 해상도: {sourceResolution.ToStringByDetailed()}\n" +
                $"변환 목표 해상도: {destinationResolution.ToStringByDetailed()}\n" +
                $"Match Width ---({matchWidthOrHeight})--- Height\n" +
                $"\n\nUI 자동 스케일링 작업 시작할까요?", logObj: this, isError: false, ok:"Ok", cancel: "Cancel"))
            {
                return false;
            }

            float scaleMultiplier = GetScaleMultiplier();
            string taskName = "UI AutoScaling By CWJ." + nameof(UIAutoScaleEditor);

            // Undo 그룹 시작
            UndoCurGroupIndex = Undo.GetCurrentGroup();
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(taskName);
            string errorStr = null;
            try
            {
                for (int i = 0; i < targetObjs.Length; i++)
                {
                    var rootTrf = targetObjs[i];
                    if (rootTrf == null)
                    {
                        continue;
                    }
                    if (isFigmaConvertedUI && (rootTrf.transform == canvasScaler.transform || !rootTrf.GetComponent<Canvas>()))
                    {
                        if (rootTrf.GetAnchor() == RectAnchor.STRETCH_FULL)
                        {
                            continue;
                        }
                        rootTrf.SetAnchor(RectAnchor.STRETCH_FULL, setPosition: false);
                    }
                    ScaleUIElements(rootTrf, scaleMultiplier);
                }
            }
            catch (Exception e)
            {
                errorStr = e.ToString();
                Debug.LogError($"[{taskName}] {CurScalingTarget.name}를 작업하던중에 오류가 있어서 실행전으로 되돌립니다\n" + errorStr, CurScalingTarget);
            }
            finally
            {
                Undo.CollapseUndoOperations(UndoCurGroupIndex);
                string displayMsg;
                bool hasError = errorStr != null;
                    // Undo 그룹 마무리
                if (hasError)
                {
                    Undo.PerformUndo();
                    displayMsg = $"UI 자동 스케일링 작업 실패.\n오류로 중지됨. 재시도바람.";
                }
                else
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    displayMsg = $"UI 자동 스케일링 작업 성공!";
                }
                CWJ.AccessibleEditor.DisplayDialogUtil.DisplayDialog<UIAutoScaleEditor>(displayMsg, isError: hasError, logObj: hasError ? CurScalingTarget : this);
                UndoCurGroupIndex = -1;
                CurScalingTarget = null;
            }
            return true;
        }

        float GetScaleMultiplier()
        {
            float widthScale = sourceResolution.x / destinationResolution.x;
            float heightScale = sourceResolution.y / destinationResolution.y;

            float scaleFactor = matchWidthOrHeight == 0.5f ?
                Mathf.Sqrt(widthScale * heightScale) :
                Mathf.Pow(widthScale, 1 - matchWidthOrHeight) * Mathf.Pow(heightScale, matchWidthOrHeight);

            float scaleMultiplier = 1 / scaleFactor;
            return scaleMultiplier;
        }



        private void ScaleUIElements(RectTransform parent, float multiplier)
        {
            Queue<RectTransform> rectTrfQueue = new();
            rectTrfQueue.Enqueue(parent);

            // 총 처리할 오브젝트 수 계산
            int totalObjects = GetTotalChildCount(parent);
            int processedObjects = 0;
            int lastProgress = -1;

            string progressBarTitle = $"CWJ.{nameof(UIAutoScaleEditor)} - UI Auto Scaling";
            EditorUtility.DisplayProgressBar(progressBarTitle, $"target : ", 0f);
            try
            {
                while (rectTrfQueue.TryDequeue(out CurScalingTarget))
                {
                    processedObjects++;
                    int currentProgress = (int)((float)processedObjects / totalObjects * 100);

                    if (currentProgress != lastProgress)
                    {
                        lastProgress = currentProgress;
                        EditorUtility.DisplayProgressBar(progressBarTitle, $"target : {CurScalingTarget.name}", (float)currentProgress / 100);
                    }

                    // 자식들을 큐에 추가
                    foreach (RectTransform child in CurScalingTarget)
                    {
                        if (child != null)
                            rectTrfQueue.Enqueue(child);
                    }

                    if (CurScalingTarget.transform == canvasScaler.transform || CurScalingTarget.GetComponent<Canvas>() != null)
                    {
                        continue;
                    }

                    // Undo 기록
                    Undo.RecordObject(CurScalingTarget, "Scale UI Element");

                    // AnchoredPosition과 SizeDelta 스케일링
                    CurScalingTarget.anchoredPosition *= multiplier;
                    CurScalingTarget.sizeDelta *= multiplier;

                    EditorUtility.SetDirty(CurScalingTarget);

                    if (ScalingTmpText(CurScalingTarget, multiplier))
                    { }
                    else if (AutoSettingImage(CurScalingTarget))
                    { }

                    if (ScalingHorizontalGroup(CurScalingTarget, multiplier))
                    { }
                    else if (ScalingVerticalGroup(CurScalingTarget, multiplier))
                    { }
                    else if (ScalingGridGroup(CurScalingTarget, multiplier))
                    { }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                // 진행률 창 닫기
                EditorUtility.ClearProgressBar();
            }
        }


        private int GetTotalChildCount(Transform parent)
        {
            return parent.GetComponentsInChildren<RectTransform>(true).Length;
        }

        void SetPadding(LayoutGroup lg, float multiplier)
        {
            lg.padding.left = Mathf.RoundToInt(lg.padding.left * multiplier);
            lg.padding.right = Mathf.RoundToInt(lg.padding.right * multiplier);
            lg.padding.top = Mathf.RoundToInt(lg.padding.top * multiplier);
            lg.padding.bottom = Mathf.RoundToInt(lg.padding.bottom * multiplier);
        }

        bool _ScalingComponentCore<T>(RectTransform rectTrf, Action<T> scalingAct)
            where T : Component
        {
            if (rectTrf.TryGetComponent(out T comp))
            {
                Undo.RecordObject(comp, "Scale " + typeof(T).Name);

                scalingAct.Invoke(comp);

                EditorUtility.SetDirty(comp);
                return true;
            }
            return false;
        }

        bool ScalingTmpText(RectTransform rectTrf, float multiplier)
        {
            return _ScalingComponentCore<TextMeshProUGUI>(rectTrf, (txt) =>
            {
                bool backup = txt.enabled;
                txt.enabled = false;
                if (txt.enableAutoSizing)
                {
                    txt.fontSizeMin *= multiplier;
                    txt.fontSizeMax *= multiplier;
                }
                else
                {
                    txt.fontSize *= multiplier;
                }
                txt.margin *= multiplier;
                txt.enabled = backup;
            });
        }

        bool ScalingVerticalGroup(RectTransform rectTrf, float multiplier)
        {
            return _ScalingComponentCore<VerticalLayoutGroup>(rectTrf, (vlg) =>
            {
                bool backup = vlg.enabled;
                vlg.enabled = false;
                SetPadding(vlg, multiplier);
                vlg.spacing *= multiplier;
                if (isFigmaConvertedUI)
                {
                    if (rectTrf.GetComponentInParent<LayoutElement>() == null)
                    { //레이어그룹 자식에 있는 LayoutGroup에는 적용안함 (자식오브젝트에선 모양유지가 중요할테니)
                        if (isNeedVerticalLayoutContentSizeFitter)
                        {
                            var sizeFitter = vlg.gameObject.GetOrAddComponent<ContentSizeFitter>();
                            if (sizeFitter.verticalFit != ContentSizeFitter.FitMode.PreferredSize)
                                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                            if (!sizeFitter.enabled) sizeFitter.enabled = true;
                        }
                        if (isVerticalLayoutItemExpandWidth)
                        {
                            vlg.childControlHeight = false;
                            vlg.childForceExpandHeight = false;
                            vlg.childControlWidth = true;
                            vlg.childForceExpandWidth = true;
                        }
                    }

                }

                vlg.enabled = backup;
            });
        }

        bool ScalingHorizontalGroup(RectTransform rectTrf, float multiplier)
        {
            return _ScalingComponentCore<HorizontalLayoutGroup>(rectTrf, (hlg) =>
            {
                bool backup = hlg.enabled;
                hlg.enabled = false;
                SetPadding(hlg, multiplier);
                hlg.spacing *= multiplier;
                if (isFigmaConvertedUI)
                {
                    if (rectTrf.GetComponentInParent<LayoutElement>() == null)
                    { //레이어그룹 자식에 있는 LayoutGroup에는 적용안함 (자식오브젝트에선 모양유지가 중요할테니)
                        if (isNeedHorizontalLayoutContentSizeFitter)
                        {
                            var sizeFitter = hlg.gameObject.GetOrAddComponent<ContentSizeFitter>();
                            if (sizeFitter.horizontalFit != ContentSizeFitter.FitMode.PreferredSize)
                                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                            if (!sizeFitter.enabled) sizeFitter.enabled = true;
                        }

                        if (isHorizontalLayoutItemExpandHeight)
                        {
                            hlg.childControlHeight = true;
                            hlg.childForceExpandHeight = true;
                            hlg.childControlWidth = false;
                            hlg.childForceExpandWidth = false;
                        }
                    }
                }
                hlg.enabled = backup;
            });
        }

        bool ScalingGridGroup(RectTransform rectTrf, float multiplier)
        {
            return _ScalingComponentCore<GridLayoutGroup>(rectTrf, (glg) =>
            {
                bool backup = glg.enabled;
                glg.enabled = false;
                SetPadding(glg, multiplier);
                glg.spacing *= multiplier;
                if (isFigmaConvertedUI)
                {
                    if (rectTrf.GetComponentInParent<LayoutElement>() == null)
                    { //레이어그룹 자식에 있는 LayoutGroup에는 적용안함 (자식오브젝트에선 모양유지가 중요할테니)
                        if (isNeedHorizontalLayoutContentSizeFitter || isNeedVerticalLayoutContentSizeFitter)
                        {
                            var sizeFitter = glg.gameObject.GetOrAddComponent<ContentSizeFitter>();
                            if (isNeedHorizontalLayoutContentSizeFitter)
                            {
                                if (sizeFitter.horizontalFit != ContentSizeFitter.FitMode.PreferredSize)
                                    sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                            }
                            if (isNeedVerticalLayoutContentSizeFitter)
                            {
                                if (sizeFitter.verticalFit != ContentSizeFitter.FitMode.PreferredSize)
                                    sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                            }
                            if (!sizeFitter.enabled) sizeFitter.enabled = true;
                        }
                    }
                }
                glg.enabled = backup;
            });
        }

        bool AutoSettingImage(RectTransform rectTrf/*, float multiplier*/)
        {
            return _ScalingComponentCore<Image>(rectTrf, (img) =>
            {
                bool backup = img.enabled;
                img.enabled = false;
                bool isExpandable = rectTrf.GetAnchor().ToInt() >= 9 /*|| rectTrf.name.Equals("bg") || rectTrf.name.Equals("background")*/;
                if (!isExpandable && isFigmaConvertedUI)
                {
                    LayoutGroup gp = null;
                    if (isVerticalLayoutItemExpandWidth)
                        gp = rectTrf.GetComponentInParent<VerticalLayoutGroup>();
                    if (gp == null && isHorizontalLayoutItemExpandHeight)
                        gp = rectTrf.GetComponentInParent<HorizontalLayoutGroup>();
                    isExpandable = gp != null;
                }
                if (isExpandable && img.type != Image.Type.Sliced)
                {
                    img.type = Image.Type.Sliced;
                    img.pixelsPerUnitMultiplier = imagePixelsPerUnitMultiplier;
                }
                img.enabled = backup;
            });
        }


        public void CWJEditor_OnSelect(MonoBehaviour target)
        {
            Init();
        }

        public void CWJEditor_OnCompile()
        {
            Init(false);
        }
    }
}
#endif
