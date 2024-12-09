using UnityEngine;
using UnityEngine.UI;

namespace CWJ
{
    [RequireComponent(typeof(Button)), DisallowMultipleComponent]
    public class EscClickBtn : MonoBehaviour
    {
        [GetComponent, SerializeField] Button escBtn;

        private void OnEnable()
        {
            EscClickBtnMngr.RegisterButton(this);
        }
        private void OnDisable()
        {
            if (MonoBehaviourEventHelper.IS_QUIT)
            {
                return;
            }
            EscClickBtnMngr.RemoveButton(this);
        }

        public void InvokeButtonOnClick()
        {
            if (escBtn == null)
                escBtn = GetComponent<Button>();
            Debug.Assert(escBtn, $"{nameof(escBtn)} 없음", this);

            if (escBtn)
                escBtn.onClick?.Invoke();
        }
    }


}