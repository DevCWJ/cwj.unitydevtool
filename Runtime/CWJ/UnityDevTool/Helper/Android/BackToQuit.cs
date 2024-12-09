using UnityEngine;

namespace CWJ
{
    public class BackToQuit : MonoBehaviour
    {
        private bool isPreparedToQuit = false;
        [SerializeField] float quitCommandTime = 2;
        private void OnEnable()
        {
            ResetQuitFlag();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (PopupHelper.isLoadingEnabled)
                {
                    return;
                }
                if (!isPreparedToQuit)
                {
                    isPreparedToQuit = true;
                    AndroidHelper.ShowToast("Press Back Again to Exit App");
                    this.Invoke(nameof(ResetQuitFlag), quitCommandTime);
                }
                else
                {
                    Debug.Log("Quit");
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                }
            }
        }

        private void ResetQuitFlag()
        {
            isPreparedToQuit = false;
        }
    }
}
