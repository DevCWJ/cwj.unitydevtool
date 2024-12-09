#if UNITY_EDITOR

using System.Reflection;
using UnityEditor;

using UnityEngine;
using System;

namespace CWJ.AccessibleEditor
{

    public static class GameViewUtil
    {
        static object gameViewSizesInstance;
        static MethodInfo getGroup;

        static GameViewUtil()
        {
            // Get the GameViewSizes instance using reflection
            var sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var instanceProp = singleType.GetProperty("instance");
            getGroup = sizesType.GetMethod("GetGroup");
            gameViewSizesInstance = instanceProp.GetValue(null, null);
        }

        public enum GameViewSizeType
        {
            AspectRatio, FixedResolution
        }

        public static Vector2 GetCurGameViewSize()
        {
            var gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            System.Reflection.MethodInfo GetSizeOfMainGameView = gameViewType.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
            return (Vector2)Res;
        }

        public static void SetGameViewSize(Vector2 size)
        {
            SetGameViewSize(size, "Custom Size");
        }

        public static void SetGameViewSize(Vector2 size, string text,
            GameViewSizeType viewSizeType = GameViewSizeType.FixedResolution, GameViewSizeGroupType sizeGroupType = GameViewSizeGroupType.Standalone)
        {
            Vector2Int sizeInt = size.ToVector2Int();
            int width = sizeInt.x;
            int height = sizeInt.y;
            int idx = FindSize(sizeGroupType, width, height);
            if (idx < 0)
            {
                AddCustomSize(viewSizeType, sizeGroupType, width, height, text);
                idx = FindSize(sizeGroupType, width, height);
            }
            //Debug.LogError("?"+idx);
            SetSize(idx + 8); //0부터 10넣어보기. 내생각엔 모든 sizeGroupType의 뷰사이즈가 포함된 배열의 index를 찾아야할듯함
        }

        public static void AddCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height, string text)
        {
            var group = GetGroup(sizeGroupType);
            var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize");
            var gvsType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
            var gameViewSizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
            var ctor = gvsType.GetConstructor(new Type[] { gameViewSizeType, typeof(int), typeof(int), typeof(string) });
            var newSize = ctor.Invoke(new object[] { (int)viewSizeType, width, height, text });
            addCustomSize.Invoke(group, new object[] { newSize });
        }

        public static int FindSize(GameViewSizeGroupType sizeGroupType, int width, int height)
        {
            var group = GetGroup(sizeGroupType);
            var groupType = group.GetType();
            var getBuiltinCount = groupType.GetMethod("GetBuiltinCount");
            var getCustomCount = groupType.GetMethod("GetCustomCount");
            int customCount = (int)getCustomCount.Invoke(group, null);
            int builtinCount = (int)getBuiltinCount.Invoke(group, null);
            Debug.LogError("b " + builtinCount + "/ c " + customCount);
            int sizesCount = builtinCount + customCount;
            var getGameViewSize = groupType.GetMethod("GetGameViewSize");
            var gvsType = getGameViewSize.ReturnType;
            var widthProp = gvsType.GetProperty("width");
            var heightProp = gvsType.GetProperty("height");
            var indexValue = new object[1];
            for (int i = 0; i < sizesCount; i++)
            {
                indexValue[0] = i;
                var size = getGameViewSize.Invoke(group, indexValue);
                int sizeWidth = (int)widthProp.GetValue(size, null);
                int sizeHeight = (int)heightProp.GetValue(size, null);
                if (sizeWidth == width && sizeHeight == height)
                    return i;
            }
            return -1;
        }

        public static void SetSize(int index)
        {
            var gvWndType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var gvWnd = EditorWindow.GetWindow(gvWndType);
            selectedSizeIndexProp.SetValue(gvWnd, index, null);
            gvWnd.Repaint();
        }

        static object GetGroup(GameViewSizeGroupType type)
        {
            return getGroup.Invoke(gameViewSizesInstance, new object[] { (int)type });
        }
    }
} 
#endif