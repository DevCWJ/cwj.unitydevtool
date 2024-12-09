#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Linq;

namespace CWJ
{
    using static ViewNameDefine;

    [CustomEditor(typeof(UIViewManager))]
    public class UINavigationEditor : Editor
    {
        UIViewManager uiNavigation;
        ViewName viewName;
        //GUIContent icon;

        private void OnEnable()
        {
            if (target == null || serializedObject == null)
            {
                return;
            }
            //icon = EditorGUIUtility.IconContent("UINavigation");
            uiNavigation = serializedObject.targetObject as UIViewManager;
        }

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null)
            {
                return;
            }
            bool isPlaying = EditorApplication.isPlaying;
            bool backupEnabled = GUI.enabled;

            GUILayout.BeginVertical(GUI.skin.box);
            //GUILayout.Label(icon);
            //GUILayout.Label($"{nameof(CWJ)}/{nameof(UIViewManager)}", EditorStyles.boldLabel/*, GUILayout.ExpandHeight(true)*/);
            //GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"{nameof(CWJ)}/{nameof(UIStackManager)}" + " editor", EditorStyles.boldLabel);

            // Show Stacks
            foreach (var view in UIStackManager.Editor_uiViewStack)
            {
                GUILayout.BeginHorizontal();
                if(UIStackManager.CurRootView == null)
                GUILayout.Label((view == UIStackManager.CurRootView ? "(ROOT) " : string.Empty) + view.name, GUILayout.ExpandWidth(true));
                GUI.enabled = false;
                EditorGUILayout.ObjectField(view, typeof(GameObject), false, GUILayout.Width(150));
                GUI.enabled = backupEnabled;
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            // Show Pop, Push Button
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Methods", EditorStyles.boldLabel);
            GUI.enabled = isPlaying;

            if (GUILayout.Button("Back", GUILayout.ExpandWidth(true)))
            {
                UIStackManager.BackToLastView();
            }
            GUILayout.BeginHorizontal();
            bool pushButtonClicked = GUILayout.Button("GoToView", GUILayout.Width(100));

            viewName = (ViewName)EditorGUILayout.EnumPopup(viewName);
            if (pushButtonClicked)
            {
                UIStackManager.GoToView(viewName);
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            if (isPlaying)
                Repaint();

            GUILayout.EndVertical();

            GUI.enabled = !isPlaying;

            base.OnInspectorGUI();
        }
    }
}

#endif