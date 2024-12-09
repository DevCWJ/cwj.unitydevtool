#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using System.Linq;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace CWJ.AccessibleEditor
{
    public static class EditorDevModeChangedHandler
    {
        public static event Action<bool, List<Scene>> DevModeOrTargetSceneChangedEvent;

        static bool _prevDevModeEnabled;
        public static event Action<bool> OnDevelopBuildChanged;
        public static event Action<List<Scene>> OnActiveBuildScenesChanged;

        [InitializeOnLoadMethod]
        static void InitOnLoad()
        {
            if (BuildEventSystem.IsBuilding) return;

            _prevActiveBuildSceneList = AreAnyLoadedScenesIncludedInBuild(out var curActiveBuildSceneList) ? curActiveBuildSceneList.Select(s => s.path).ToArray() : null;

            EditorSceneManager.sceneSaving += OnSaveScene;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorBuildSettings.sceneListChanged += OnBuildSceneListChanged;

            _prevDevModeEnabled = EditorUserBuildSettings.development;
            EditorApplication.update += CheckDevelopmentMode;
        }

        private static void OnSaveScene(Scene scene, string path)
        {
            CheckActiveBuildScenesChanged();
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if ( mode != OpenSceneMode.Single)
            {
                CheckActiveBuildScenesChanged();
            }
        }

        static void CheckDevelopmentMode()
        {
            if (BuildEventSystem.IsBuilding) return;

            bool currentValue = EditorUserBuildSettings.development;
            if (currentValue != _prevDevModeEnabled)
            {
                _prevDevModeEnabled = currentValue;
                DebugLogUtil.PrintLog("Development mode change detected: " + currentValue);

                OnDevelopBuildChanged?.Invoke(currentValue);
                AreAnyLoadedScenesIncludedInBuild(out var curActiveBuildSceneList);
                DevModeOrTargetSceneChangedEvent?.Invoke(currentValue, curActiveBuildSceneList);
            }
        }

        private static void OnActiveSceneChanged(Scene curScene, Scene nextScene)
        {
            // Debug.LogError(curScene.path + " -> " + nextScene.path);
            CheckActiveBuildScenesChanged();
        }

        private static HashSet<string> _buildSceneListAllPaths;
        private static void OnBuildSceneListChanged()
        {
            _buildSceneListAllPaths = new HashSet<string>(EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes));
            CheckActiveBuildScenesChanged();
        }

        private static string[] _prevActiveBuildSceneList;
        private static void CheckActiveBuildScenesChanged()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            AreAnyLoadedScenesIncludedInBuild(out var curActiveBuildSceneList);
            if (curActiveBuildSceneList.Count != _prevActiveBuildSceneList.LengthSafe()
                || !ArrayUtil.ArrayEqualsTwoTypeList(curActiveBuildSceneList, _prevActiveBuildSceneList, (s, p) => s.path == p))
            {
                _prevActiveBuildSceneList = curActiveBuildSceneList.Select(s => s.path).ToArray();
                OnActiveBuildScenesChanged?.Invoke(curActiveBuildSceneList);
                DevModeOrTargetSceneChangedEvent?.Invoke(EditorUserBuildSettings.development, curActiveBuildSceneList);
            }
        }


        /// <summary>
        /// 현재 로드된 모든 씬 중 하나 이상이 Build Settings에 포함되어 있는지 확인합니다.
        /// Build Settings 씬 목록이 비어있다면, 현재 로드된 모든 씬을 포함된 것으로 간주합니다.
        /// </summary>
        /// <param name="curActiveBuildSceneList">Build Settings에 포함된 씬들의 목록</param>
        /// <returns>현재 로드된 씬 중 하나 이상이 Build Settings에 포함되어 있는지 여부</returns>
        private static bool AreAnyLoadedScenesIncludedInBuild(out List<Scene> curActiveBuildSceneList)
        {
            _buildSceneListAllPaths ??= new HashSet<string>(EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes));
            bool hasBuildSceneElem = _buildSceneListAllPaths.Count > 0;

            int loadedSceneCount = SceneManager.sceneCount;
            curActiveBuildSceneList = new List<Scene>(capacity: loadedSceneCount);

            for (int i = 0; i < loadedSceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    continue;
                }
                if (hasBuildSceneElem && !_buildSceneListAllPaths.Contains(scene.path))
                {
                    continue;
                }

                curActiveBuildSceneList.Add(scene);
            }


            if (curActiveBuildSceneList.Count == 0)
            {
                return false;
            }
            return true;
        }
    }
}

#endif
