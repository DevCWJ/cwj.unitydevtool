using DeadMosquito.AndroidGoodies;
using UnityEngine;


namespace CWJ
{
    public static class AndroidHelper
    {
#if CWJ_DEVELOPMENT_BUILD || UNITY_EDITOR
        static readonly string ToastTag = ("[ShowToast] ").SetSize(17, true);
#endif
        public static AndroidToastGUI _AndroidToastGUI = null;
        [System.Diagnostics.Conditional("UNITY_ANDROID")]
        public static void ShowToast(string message, AGUIMisc.ToastLength length = AGUIMisc.ToastLength.Short)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            AGUIMisc.ShowToast(message, length);
#if CWJ_DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.Log(ToastTag + message);
#endif
#if CWJ_DEVELOPMENT_BUILD || UNITY_EDITOR
            if (Application.isEditor && Application.isPlaying)
            {
                if (!_AndroidToastGUI)
                {
                    _AndroidToastGUI = GameObject.FindObjectOfType<AndroidToastGUI>();
                    if (!_AndroidToastGUI)
                    {
                        var newO = new GameObject();
                        _AndroidToastGUI = newO.AddComponent<AndroidToastGUI>();
                    }
                }
                _AndroidToastGUI.SetGuiMsg(message, (length.ToInt() + 1 * 2));
            }
#endif
        }


    }
}
