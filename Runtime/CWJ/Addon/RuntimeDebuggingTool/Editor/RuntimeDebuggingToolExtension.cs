#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using CWJ.AccessibleEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CWJ
{
    public static class RuntimeDebuggingToolExtension
    {
        private const string ThisRdtToolName = nameof(RuntimeDebuggingTool);
        private const string PrefabFileName  = "[CWJ." + ThisRdtToolName + "]";

        private static bool _isChecking;

        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            _isChecking = false;
            EditorDevModeChangedHandler.DevModeOrTargetSceneChangedEvent += WhenNeedUpdateRdtObjState;
            EditorSceneManager.sceneSaving += (scene, _) =>
            {
                _isChecking = false;
                WhenNeedUpdateRdtObjState(EditorUserBuildSettings.development, new List<Scene>() { scene });
            };
        }

        private static void WhenNeedUpdateRdtObjState(bool isDevBuild, List<Scene> curActiveBuildScenes)
        {
            if (_isChecking || EditorApplication.isPlayingOrWillChangePlaymode || curActiveBuildScenes.CountSafe() == 0 ||
                !CWJ_EditorEventHelper.IsProjectOpened)
                return;

            _isChecking = true;

            // Debug.Log("Development mode changed: " + isDevBuild);

            // Debug.LogError("curActiveBuildScenesChanged : " + string.Join(", ", curActiveBuildScenes));

            if (UpdateRdtState(isDevBuild, curActiveBuildScenes))
            {
                AssetDatabase.SaveAssets();
                _isChecking = false;
                //AccessibleEditorUtil.ForceRecompile();
            }
        }

        private static void InitRdtObject(GameObject rdtObj, bool objActive)
        {
            bool isModified = false;
            if (rdtObj.name != PrefabFileName)
            {
                rdtObj.name = PrefabFileName;
                isModified = true;
            }

            if (rdtObj.transform.root != rdtObj.transform)
            {
                rdtObj.transform.SetParent(null);
                rdtObj.transform.SetAsFirstSibling();
                isModified = true;
            }

            if (rdtObj.activeSelf != objActive)
            {
                rdtObj.SetActive(objActive);
                isModified = true;
            }

            if (objActive && rdtObj.TryGetComponent<RuntimeDebuggingTool>(out var rdt)
                          && (!rdt.enabled || !rdt.isVisibleOnStart || !rdt.isDebuggingEnabled))
            {
                rdt.isVisibleOnStart = true;
                rdt.isDebuggingEnabled = true;
                rdt.enabled = true;
                isModified = true;
            }

            if (isModified)
            {
                EditorUtility.SetDirty(rdtObj);
                // EditorSceneManager.MarkSceneDirty(rdtObj.scene);
                if (objActive)
                    AccessibleEditorUtil.PingObj(rdtObj);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>isMarked</returns>
        private static bool UpdateRdtState(bool isDevBuild, List<Scene> curActiveBuildScenes)
        {
            var needCreateRdtScenes = new List<Scene>();
            var isChangedScene      = false;
            var rootObjs            = new List<GameObject>();
            foreach (Scene scene in curActiveBuildScenes)
            {
                scene.GetRootGameObjects(rootObjs);
                GameObject rdtGo = rootObjs.FirstOrDefault(g => g && g.name == PrefabFileName);

                RuntimeDebuggingTool rdt = null;
                if (isDevBuild && (!rdtGo || !rdtGo.TryGetComponent<RuntimeDebuggingTool>(out rdt)))
                {
                    if (FindUtil.TryGetChildCompInObjList<RuntimeDebuggingTool>(rootObjs, true, out rdt))
                    {
                        InitRdtObject(rdt.gameObject, true);
                        isChangedScene = true;
                    }
                    else
                    {
                        needCreateRdtScenes.Add(scene);
                    }

                }

                if (rdtGo && (rdtGo.activeSelf != isDevBuild))
                {
                    InitRdtObject(rdtGo, isDevBuild);
                    isChangedScene = true;
                }
            }

            if (needCreateRdtScenes.Count > 0 &&
                ScriptableObjectStore.TryGetStoreFilePath(out string storeCacheFilePath))
            {
                string myRootFolderPath = PathUtil.GetParentDirectory(storeCacheFilePath, 3);
                var    prefabPath = $"{myRootFolderPath}/Addon/{ThisRdtToolName}/__UseThisPrefabs/{PrefabFileName}.prefab";
                var    prefabSrc = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefabSrc)
                {
                    foreach (Scene scene in needCreateRdtScenes)
                    {
                        var newRdtObj = (GameObject)PrefabUtility.InstantiatePrefab(prefabSrc, scene);
                        InitRdtObject(newRdtObj, true);
                    }

                    isChangedScene = true;
                }
            }

            return isChangedScene;
        }
    }
}
#endif
