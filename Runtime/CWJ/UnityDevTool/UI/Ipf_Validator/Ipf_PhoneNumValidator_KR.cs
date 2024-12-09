using UnityEngine;
using TMPro;
using CWJ;

[CreateAssetMenu(fileName = "PhoneNumValidator_KR", menuName = "CWJ/TMP/PhoneNumValidator_KR", order = 1)]
public class Ipf_PhoneNumValidator_KR : TMP_InputValidator
{
    public override char Validate(ref string text, ref int pos, char ch)
    {
        // 입력된 문자가 숫자인지 확인
        if (char.IsDigit(ch))
        {
            // 입력 가능한 최대 길이 13자(010-1234-5678)
            if (text.LengthSafe() >= 13)
            {
                return (char)0;
            }

            // 숫자를 입력할 때 하이픈이 필요한 위치에 하이픈 추가
            if (text.Length == 3 || text.Length == 8)
            {
                text += "-"; // 하이픈 추가
                pos += 1; // 포지션을 한 칸 증가
            }
            string tmp = text == null ? ch.ToString() : text + ch;
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WebGLPlayer:
                    text = tmp;
                    pos += 1;
                    break;

                default:
                    return ch;
            }
        }

        // 숫자가 아닌 경우 입력 차단
        return (char)0;
    }
}