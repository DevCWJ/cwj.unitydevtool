## BUG

1. __Unity2019의 UGUI 컴포넌트들과 Unity2020의 UGUI컴포넌트들은 서로 호환이 안됨 (Unity버그지만 내 컴포넌트에 영향이 가므로 Bug처리)__
    - Unity2020에서 PolygonChartManager 생성에 문제가 있음
2. PolygonChartManager 생성할때 아무 Canvas에나 만들어짐 (DontDestroyOnLoad의 Canvas는 피하도록 수정)

## TODO (importance priority)

0. VisualizeProperty, VisualizeField 를 통해 그려질때 isFindAllBaseClass가 true 일 경우 변수이름앞에 base. / this. 로 구분

1. __CWJ_Inspector_Core.cs 완성__
    - CWJ_Inspector_Core_Cache, FieldCache 완성
        - 이는 외형을 직접 그려주는 Attribute가 아닌 ReadonlyAttribute_Editor, HideInConditionalAttribute_Editor, SyncValueAttribute_Editor, OnValueChangedAttribute_Editor,
          등과 같은 상태 attribute의 경우엔 PropertyDrawer를 없애고 FieldCache에서 실시간으로 상태를 판단해주게함
    - DrawPropAction에 미리 그릴 함수를 캐싱
        - 직접 그려주는 어트리뷰트의 경우엔 해당 PropertyDrawer를 가져와서 캐시해놓고 사용하도록 수정하기.
          그렇게 최종적으로는 EditorGUI_CWJ.PropertyField_New를 없에는게 목표임.
          더 나아가서는 CWJInspector_VisualizeField, CWJInspector_VisualizeProperty 또한 비슷한방식을 거쳐서 그려지도록
          (현재의 CWJInspector_BodyAndFoldout.PropCache 와 기능비슷함)
    - Array/List, Class/Struct의 경우 Element를 그릴때 FieldInfo로 그리는데 SerializedProperty를 갖고있는 경우 element부터는 SerializedProperty가 그리게하자...

2. ~~__CWJInspector_ElementAbstract.isForciblyGetMembers 기능 완성__~~
    - ~~설명: 사용자가 CWJInspector_VisualizeProperty foldout을 열었을때 이미 isForciblyGetMembers을 활성화 한 경우가 아니라면 가장 밑에 isForciblyGetMembers 버튼이 나오고
      그 버튼을 누를경우 해당 클래스의 모든 property를 보여줌~~
    - __*완료*__

3. __CWJInspector_ElementAbstract 들 Dispose 구현__
    - isForciblyGetMembers 기능때문에 컴파일되지않더라도 CWJInspector_ElementAbstract들이 Dispose되어야 하는 경우가 생김

4. __WarningThatNonSerialized 제거__
    - 2까지 완료되면 isForciblyGetMembers를 활성화 할 때에만 경고 팝업 띄우면 될듯

5. __RuntimeLogSetting 완성__
    - CWJ Special Foldout Group에서는 DebugSetting foldout을 그냥 버튼식으로 보여주면 될듯
      define 으로 빌드에 영향을 주는 DebugSettingWindow와 컴파일시키지 않아도 Log활성화 여부를 선택적으로 설정할수있는 RuntimeLogSetting 두가지를 Foldout에 버튼으로 보여주면되고
      완성시켜야할것은 RuntimeLogSetting.
      RuntimeLogSetting안에서 log를 ignore시킬 Component 타입 혹은 특정 오브젝트를 추가/삭제/관리 할수있으며
      추가되어있는 Comp타입 혹은 오브젝트의 인스펙터에선 log가 ignore되어있음을 확인할수있음 (고도화요소로는 Hierachy에서 ignore시킬 오브젝트를 관리할수있게(visibable이랑 selectable 결정할수있는거처럼))

6. __VrDevTool UnityEngine.XR로 전환__

7. __Custom Transform GUI__
    - position, rotation, scale 각 Vector3 값을 복사 / 붙여넣기 할수있게

8. ~~Multiple Objects Edit : Array/List~~
    - ~~20.1.0에서 복수선택시 inspector multiple edit이 가능하게끔 수정했으나
      복수선택후 drag and drop으로 값을 추가 할 시 선택된 오브젝트의 배열들이 모두 같은값이 되는 문제를 해결해야함~~
      __*아 ㅋㅋ 유니티 builtIn array drawer에도 똑같은 현상 발생됨 그대로 놔둬도 될듯*__

9. Array, List Drawer for SerializedProperty
    - while(index > arrayProp.arraySize)
      arrayProp.InsertArrayElementAtIndex(arrayProp.arraySize);
      while(index < arrayProp.arraySize)
      arrayProp.DeleteArrayElementAtIndex(arrayProp.arraySize-1);
      와
      SerializedProperty elemProp = arrayProp.GetArrayElementAtIndex(i);
      EditorGUI.PropertyField(position, elemProp, GUIContent.none);
      를 이용해서 Array CWJ_Inspector_BodyAndFoldout 241에 SerializedProperty용 array/Listdarwer 만들기

10. DrawClassOrStructType도 Undo지원하도록 수정


11. HideConditional 이렇게
    ```
    public enum EConditionOperator
    {
        And,
        Or
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HideIfAttribute : ShowIfAttributeBase
    {
        public HideIfAttribute(string condition) : base(condition)
        {
            Inverted = true;
        }

        public HideIfAttribute(EConditionOperator conditionOperator, params string[] conditions)
            : base(conditionOperator, conditions)
        {
            Inverted = true;
        }

        public HideIfAttribute(string enumName, object enumValue)
            : base(enumName, enumValue as Enum)
        {
            Inverted = true;
        }
    }
    ```
