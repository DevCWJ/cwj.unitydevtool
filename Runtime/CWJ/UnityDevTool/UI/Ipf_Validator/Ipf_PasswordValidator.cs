using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CWJ
{
    [CreateAssetMenu(fileName = "PasswordValidator", menuName = "CWJ/TMP/PasswordValidator", order = int.MaxValue)]
    [System.Serializable]
    public class Ipf_PasswordValidator : TMPro.TMP_InputValidator
    {
        [Header("Password Settings")]
        public int maxLength = 23;

        // 허용할 특수문자 목록 (필요에 따라 수정 가능)
        private readonly string allowedSpecialChars = "^$*.[]{}()?\"!@#%&/\\,><':;|_~`=+-";
        public override char Validate(ref string text, ref int pos, char ch)
        {
            // 현재 텍스트 길이가 최대 길이에 도달했는지 확인
            if (text.LengthSafe() >= 13)
            {
                return (char)0;
            }

            // 허용된 문자군인지 확인
            if (IsAllowedCharacter(ch))
            {
                // 입력을 허용하고 텍스트에 추가
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
            return (char)0;
        }

        /// <summary>
        /// 입력된 문자가 허용된 문자군에 속하는지 확인
        /// </summary>
        /// <param name="c">입력된 문자</param>
        /// <returns>허용되면 true, 그렇지 않으면 false</returns>
        private bool IsAllowedCharacter(char c)
        {
            // 영어 대문자
            if (c >= 'A' && c <= 'Z') return true;
            // 영어 소문자
            if (c >= 'a' && c <= 'z') return true;
            // 숫자
            if (c >= '0' && c <= '9') return true;
            // 허용된 특수문자
            if (allowedSpecialChars.Contains(c)) return true;

            // 한글이나 다른 문자는 허용하지 않음
            return false;
        }
    }
}
