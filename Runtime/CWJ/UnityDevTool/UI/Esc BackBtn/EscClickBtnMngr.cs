using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UniRx;
using UnityEngine;
namespace CWJ
{
#if UNITY_EDITOR
    public class EscClickBtnMngr : CWJ.Singleton.SingletonBehaviour<EscClickBtnMngr>, CWJ.Singleton.IDontPrecreatedInScene
#else
    public static class EscClickBtnMngr
#endif
    {
        static bool isInit = false;
#if UNITY_EDITOR
        [VisualizeField]
#endif
        private static CWJ.Collection.StackLinked<EscClickBtn> _KeyControlledBtnStack;

        private static bool OnClickEsc_Aos(long _) { return Input.GetKeyUp(KeyCode.Escape); }
        private static bool OnClickEsc_PC(long _) { return Input.GetKeyDown(KeyCode.Escape); }

        private static void Init()
        {
            if (isInit)
            {
                return;
            }
            isInit = true;
            _KeyControlledBtnStack = new();

            var o = Observable.EveryUpdate()
                .TakeUntil(Observable.OnceApplicationQuit());

            o = ((Application.platform == RuntimePlatform.Android) ? o.Where(OnClickEsc_Aos)
                : o.Where(OnClickEsc_PC).Throttle(TimeSpan.FromSeconds(0.15f)));

            o.Subscribe(OnTriggerButton);

            //Observable.EveryUpdate()
            ////.ObserveOnMainThread()
            //.TakeUntil(Observable.OnceApplicationQuit())
            //.Where(_ => Input.GetKey(KeyCode.Escape))
            //.Throttle(TimeSpan.FromSeconds(0.15f))
            //.Subscribe(OnTriggerButton);
        }


        public static void RegisterButton(EscClickBtn b)
        {
            Init();
            _KeyControlledBtnStack.Push(b);
        }
        public static void RemoveButton(EscClickBtn b)
        {
            _KeyControlledBtnStack?.Remove(b);
        }
        private static void OnTriggerButton(long _)
        {
            if (_KeyControlledBtnStack.TryPeek(out var btn)
                && btn && !PopupHelper.isLoadingEnabled)
            {
                btn.InvokeButtonOnClick();
            }
        }
    }

}
