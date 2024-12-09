using System;
using System.Collections;
using System.Collections.Generic;


using UnityEngine;

namespace CWJ.SmartSenior
{
    [SelectionBase]
    public class GaugePointerMover : MonoBehaviour, CWJ.AccessibleEditor.InspectorHandler.IOnGUIHandler
    {
        [HelpBox("이 컴포넌트의 위치는 gaugeBarRectTrf에 있는것이 적절합니다")]
        [SerializeField] private RectTransform gaugeBarRectTrf;
        [SerializeField] private RectTransform pointerRectTrf;
        [DrawHeaderAndLine("Setting")]
        [HelpBox("설정값 혹은 관련 UI크기는 gaugeBarRectTrf 오브젝트가 켜져있을때 수정해주세요.")]
        [SerializeField] private float leftPadding = 10f; // 좌측 패딩
        [SerializeField] private float rightPadding = 10f; // 우측 패딩


#if UNITY_EDITOR
        [SerializeField, Readonly, ErrorIfNull] private bool isConfigured = false;
        [SerializeField, HideInInspector] RectTransform gaugeBarRectTrf_cache;
        [SerializeField, HideInInspector] float rightPadding_cache;
        [SerializeField, Range(0,100)] float editor_testValue;
        [SerializeField, HideInInspector] float editor_testValue_cache;

        private void OnValidate()
        {
            if (rightPadding_cache != rightPadding || !gaugeBarRectTrf_cache || !gaugeBarRectTrf || gaugeBarRectTrf != gaugeBarRectTrf_cache)
            {
                bool last = isConfigured;
                isConfigured = false;
                if (last != isConfigured)
                    CWJ.AccessibleEditor.EditorSetDirty.SetObjectDirty(this);
            }
        }

        public void CWJEditor_OnGUI()
        {
            if (!IsUIObjActivate())
            {
                return;
            }
            DetectNeedChangeMinMax();
            if (pointerRectTrf && pointerRectTrf.GetAnchor() != RectAnchor.MIDDLE_LEFT)
            {
                pointerRectTrf.SetAnchor(RectAnchor.MIDDLE_LEFT, setPivot: false, setPosition: false);
                CWJ.AccessibleEditor.EditorSetDirty.SetObjectDirty(pointerRectTrf);
            }
            if (isConfigured && IsUIObjActivate())
            {
                if (editor_testValue != editor_testValue_cache)
                {
                    editor_testValue_cache = editor_testValue;
                    _UpdatePointer(editor_testValue);
                }
            }
        }

        bool IsUIObjActivate()
        {
            return gaugeBarRectTrf && gaugeBarRectTrf.gameObject.activeInHierarchy;
        }
        void DetectNeedChangeMinMax()
        {
            if ((minX - leftPadding).RoundToFloatingValue() != 0
                || (maxX - (gaugeBarRectTrf.rect.width - rightPadding)).RoundToFloatingValue() != 0)
            {
                isConfigured = false;
                CWJ.AccessibleEditor.EditorSetDirty.SetObjectDirty(this);
            }
        }

        [InvokeButton]
        private bool AutoConfigureSetting()
        {
            if (IsUIObjActivate())
            {
                var min = minX; var max = maxX;
                rightPadding_cache = rightPadding;
                UpdateMinMax();
                gaugeBarRectTrf_cache = gaugeBarRectTrf;
                isConfigured = true;
                CWJ.AccessibleEditor.EditorSetDirty.SetObjectDirty(this);
                //UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                return true;
            }
            else
            {
                Debug.LogError("gaugeBarRectTrf 오브젝트가 없거나 꺼져있습니다. 오브젝트를 켠 상태로 시도해주세요", gameObject);
                return false;
            }

        }

        [InvokeButton]
        void Test_SetRandomValue()
        {
            if (isConfigured && IsUIObjActivate())
            {
                System.Random rng = new System.Random();
                editor_testValue_cache = editor_testValue = rng.Next(100 + 1);
                _UpdatePointer(editor_testValue);
            }
        }
#endif

        [HelpBox("하단 [Invoke Buttons] 에서 AutoConfigureSetting() 눌러주고 씬저장")]
        [DrawHeaderAndLine("Cache")]
        [SerializeField, Readonly] private double minX;
        [SerializeField, Readonly] private double maxX;
        private void UpdateMinMax()
        {
            minX = leftPadding;
            maxX = gaugeBarRectTrf.rect.width - rightPadding;
        }

        bool isShown;
        [NonSerialized] float waitingValue = -1;
        private void OnEnable()
        {
            isShown = true;
            if (waitingValue >= 0)
            {
                var v = waitingValue;
                waitingValue = -1;

                ThreadDispatcher.EnqueueDelayed(
                    () =>
                    {
                        UpdateMinMax();
                        _UpdatePointer(v);
                    }, ThreadDispatcher.GetLateUpdateDelayTime());
            }
        }

        private void OnDisable()
        {
            isShown = false;
        }


        [NonSerialized] double lastValue = -1;

        public void UpdatePointer(double value)
        {
            if (!isShown)
            {
                waitingValue = (float)value;
                return;
            }
            if (lastValue == value)
            {
                return;
            }
            _UpdatePointer(value);
        }

        static readonly Vector2 LeftPivot = new(0, 0.5f);
        static readonly Vector2 CenterPivot = new(0.5f, 0.5f);
        static readonly Vector2 RightPivot = new(1, 0.5f);

        void _UpdatePointer(double value)
        {
            lastValue = value;
            // Value는 0 ~ 100 범위의 값이어야 함
            if (value <= 0)
            {
                value = 0;
                pointerRectTrf.SetPivot(LeftPivot);
            }
            else if (value >= 100)
            {
                value = 100;
                pointerRectTrf.SetPivot(RightPivot);
            }
            else
            {
                pointerRectTrf.SetPivot(CenterPivot);
            }

            double targetX = Mathf.Lerp((float)minX,(float) maxX, (float)value / 100f);

            // 포인터 위치 업데이트
            Vector2 anchoredPosition = pointerRectTrf.anchoredPosition;
            anchoredPosition.x = (float)targetX;
            pointerRectTrf.anchoredPosition = anchoredPosition;
        }


    }
}
