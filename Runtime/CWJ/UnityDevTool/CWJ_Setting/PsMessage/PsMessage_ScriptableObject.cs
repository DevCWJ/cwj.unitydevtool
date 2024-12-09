#if UNITY_EDITOR

namespace CWJ.AccessibleEditor.PsMessage
{
    public enum EMsgState
    {
        None = 0,
        Waiting = 1,
        ShowMsg,
        ShowSetting
    }

    //ScriptableObject 클래스의 이름과 cs파일 이름과 같아야함 (자체자작한 WindowBehaviour때문)
    public sealed class PsMessage_ScriptableObject : Initializable_ScriptableObject
    {
        public override bool IsAutoReset => true;

        public EMsgState psMsgState = EMsgState.ShowSetting;

        public string confirmedMessage = "";

        public string fieldTmpMessage = "";

        public override void OnReset(bool isNeedSave = true)
        {
            psMsgState = EMsgState.ShowSetting;
            fieldTmpMessage = confirmedMessage = "\n'UnityDevTool' is a library developed by 조우정.\n\n1. Do not share to others\n\n2. Do not import to company-owned project\n";
            base.OnReset(isNeedSave);
        }
    }
}

#endif