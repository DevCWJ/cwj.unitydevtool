using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using CWJ.Unity.EditorCor.Editor;
#endif

namespace CWJ
{
    /// <summary>
    /// 실행중인지 인스펙터를 통해 알수있음, 중복실행 안됨(중복실행 시도시 실행중인걸 stop or 실행되지않게 설정가능)
    /// <br/>인스펙터에서 보고싶으면 <see cref="VisualizeFieldAttribute"/> 사용하기.
    /// <br/>(Coroutine != null 은 StartCoroutine이 yield return 에서 넘겨주기전까지 null이기때문에 만든 coroutine helper클래스)
    /// <para/>Runtime 중이 아닐때도 실행가능함
    /// <br/>Runtime 아닐때 실행시키고싶은 Editor전용 <see cref="IEnumerator"/> 에서는 <see cref="YieldInstruction"/> (yield return ~) 을 <see cref="WaitForSecondsRealtime"/> 만 사용하기
    /// </summary>
    [System.Serializable]
    public class CoroutineTracked : IDisposable
    {
        [Readonly] private string name;
        [Readonly] private bool isCantStartWhenAlreadyRunning;

        [Readonly] private bool _isRunning_frameSafely;
        public bool isRunning => _isRunning_frameSafely;

        [Readonly] private bool _isRunning_insideCor;
        public bool isRunningInsideCor => _isRunning_insideCor;

        [Readonly] private MonoBehaviour behaviour;

        private Coroutine wrapperCoroutine;
        private Coroutine doCoroutine;
#if UNITY_EDITOR  
        private Coroutine_Editor editor_wrapperCoroutine;
        private Coroutine_Editor editor_doCoroutine;
#endif

        public CoroutineTracked(string nameForClassify = null, bool? isNotStartWhenAlreadyRunning = null)
        {
            _AllStopCor();
            if (nameForClassify != null)
                name = nameForClassify;
            this.isCantStartWhenAlreadyRunning = (isNotStartWhenAlreadyRunning != null) && isNotStartWhenAlreadyRunning.Value;
        }

        public void SetSettings(MonoBehaviour monoBehaviour, string nameForClassify, bool? isCantStartWhenAlreadyRunning, bool forcedStop = false)
        {
            if (nameForClassify != null)
                name = nameForClassify;
            if (isCantStartWhenAlreadyRunning != null)
                this.isCantStartWhenAlreadyRunning = isCantStartWhenAlreadyRunning.Value;

            if (forcedStop)
                StopCoroutine();

            if (behaviour != monoBehaviour)
            {
                if (!forcedStop && !this.isCantStartWhenAlreadyRunning)
                    StopCoroutine();
                this.behaviour = monoBehaviour;
            }
        }

        IEnumerator Routine(MonoBehaviour monoBehaviour, IEnumerator enumerator)
        {
            _isRunning_insideCor = true;

#if UNITY_EDITOR  
            if (!UnityEditor.EditorApplication.isPlaying)
                yield return (editor_doCoroutine = EditorCoroutineUtil.StartCoroutine(enumerator, monoBehaviour));
            else
#endif
                yield return (doCoroutine = monoBehaviour.StartCoroutine(enumerator));
            doCoroutine = null;
            _isRunning_frameSafely = _isRunning_insideCor = false;
            wrapperCoroutine = null;
#if UNITY_EDITOR  
            editor_wrapperCoroutine = null;
            editor_doCoroutine = null;
#endif
        }

        public bool StartCoroutine(MonoBehaviour obj, IEnumerator enumerator, string nameForClassify = null, bool? isNotStartWhenAlreadyRunning = null)
        {
            SetSettings(obj, nameForClassify, isNotStartWhenAlreadyRunning);
            return StartCoroutine(obj, enumerator);
        }

        public bool StartCoroutine(MonoBehaviour behaviour, IEnumerator enumerator)
        {
            if (_isRunning_frameSafely)
            {
                if ((
#if UNITY_EDITOR  
                   (!UnityEditor.EditorApplication.isPlaying) ? editor_doCoroutine != null : 
#endif
                   doCoroutine != null) && isCantStartWhenAlreadyRunning)
                {
                    //Debug.LogException(new System.InvalidProgramException("I Can't run when it's already running\n" + nameof(IsCanNotStartWhenAlreadyRunning) + " : " + IsCanNotStartWhenAlreadyRunning), behaviour);
                    return false;
                }
                else
                {
                    StopCoroutine();
                }
            }
            if (this.behaviour != behaviour)
                this.behaviour = behaviour;

            _isRunning_frameSafely = true;
            try
            {
#if UNITY_EDITOR  
                if (!UnityEditor.EditorApplication.isPlaying)
                    editor_wrapperCoroutine = EditorCoroutineUtil.StartCoroutine(Routine(behaviour, enumerator), behaviour);
                else
#endif
                    wrapperCoroutine = behaviour.StartCoroutine(Routine(behaviour, enumerator));
            }
            catch
            {
                _isRunning_frameSafely = false;
                StopCoroutine();
            }

            return true;
        }

        bool isStopping = false;
        public IEnumerator WaitForDoRoutineExists(MonoBehaviour monoBehaviour)
        {
            float timeout = 3f;
#if UNITY_EDITOR  
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                float interval = 0.03f;
                var waitInEditor = new WaitForSecondsRealtime(interval);
                do
                {
                    yield return waitInEditor;
                    timeout -= interval;
                }
                while (timeout <= 0 || (_isRunning_frameSafely && editor_doCoroutine == null));
            }
            else
#endif
                yield return new WaitUntilWithTimeout(() => _isRunning_frameSafely && doCoroutine == null, timeout);

            _AllStopCor();
        }

        public void StopCoroutine()
        {
            if ((
#if UNITY_EDITOR  
                (!UnityEditor.EditorApplication.isPlaying) ? editor_doCoroutine == null :
#endif
                doCoroutine == null) && _isRunning_frameSafely)
            {
                if (!isStopping)
                {
                    isStopping = true;
#if UNITY_EDITOR  
                    if (!UnityEditor.EditorApplication.isPlaying)
                        EditorCoroutineUtil.StartCoroutine(WaitForDoRoutineExists(behaviour), behaviour);
                    else
#endif
                        behaviour.StartCoroutine(WaitForDoRoutineExists(behaviour));
                }
            }
            else
            {
                _AllStopCor();
            }
        }

        private void _AllStopCor()
        {
            if (doCoroutine != null) behaviour.StopCoroutine(doCoroutine);
            doCoroutine = null;
            if (wrapperCoroutine != null) behaviour.StopCoroutine(wrapperCoroutine);
            wrapperCoroutine = null;
#if UNITY_EDITOR  
            if (editor_doCoroutine != null) EditorCoroutineUtil.StopCoroutine(editor_doCoroutine);
            editor_doCoroutine = null;
            if (editor_wrapperCoroutine != null) EditorCoroutineUtil.StopCoroutine(editor_wrapperCoroutine);
            editor_wrapperCoroutine = null;
#endif
            _isRunning_frameSafely = _isRunning_insideCor = false;
            isStopping = false;
        }

        public void Dispose()
        {
            StopCoroutine();
            behaviour = null;
        }
    }
}
