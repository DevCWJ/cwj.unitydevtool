
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using DeadMosquito.AndroidGoodies;
namespace CWJ
{
    /// <summary> 안드로이드 토스트 메시지 표시 싱글톤. 메인씬에 미리 두고 Start이후에 부를것.</summary>
    public class AndroidToastGUI : MonoBehaviour
    {

        /// <summary> 안드로이드 토스트 메시지 표시하기 </summary>
        [System.Diagnostics.Conditional("UNITY_ANDROID")]
        [InvokeButton]
        void Editor_ShowToastTest(string message, AGUIMisc.ToastLength length = AGUIMisc.ToastLength.Short)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
#if CWJ_DEVELOPMENT_BUILD || UNITY_EDITOR
            if (Application.isEditor)
            {
                Debug.Log(message);
                editorGuiTime = (length.ToInt() + 1 * 2);
                editorGuiMessage = message;
                return;
            }
#endif

        }

#if CWJ_DEVELOPMENT_BUILD || UNITY_EDITOR
        public void SetGuiMsg(string guiMsg, float guiTime)
        {
            editorGuiMessage = guiMsg;
            editorGuiTime = guiTime;
            this.enabled = true;
        }
        float editorGuiTime = 0f;
        string editorGuiMessage;
        [SerializeField] int fontSize = 30;
        private GUIStyle toastStyle;
        private void OnValidate()
        {
            if (toastStyle != null)
            {
                toastStyle.fontSize = fontSize;
            }

        }
        private void OnGUI()
        {
            if (editorGuiTime == 0f) return;

            float width = Screen.width * 0.7f;
            float height = Screen.height * 0.08f;
            Rect rect = new Rect((Screen.width - width) * 0.5f, Screen.height * 0.8f, width, height);

            if (toastStyle == null)
            {
                toastStyle = new GUIStyle(GUI.skin.box);
                toastStyle.fontSize = fontSize;
                toastStyle.fontStyle = FontStyle.Bold;
                toastStyle.alignment = TextAnchor.MiddleCenter;
                toastStyle.normal.textColor = Color.white;
            }

            GUI.Box(rect, editorGuiMessage, toastStyle);
        }
        private void Update()
        {
            if (editorGuiTime > 0f)
            {
                editorGuiTime -= Time.unscaledDeltaTime;
            }
            else
            {
                editorGuiTime = 0;
                this.enabled = false;
            }
        }
#endif



    }
}