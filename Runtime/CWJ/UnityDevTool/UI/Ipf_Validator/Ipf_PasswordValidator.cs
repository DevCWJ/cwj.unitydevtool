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

        // ����� Ư������ ��� (�ʿ信 ���� ���� ����)
        private readonly string allowedSpecialChars = "^$*.[]{}()?\"!@#%&/\\,><':;|_~`=+-";
        public override char Validate(ref string text, ref int pos, char ch)
        {
            // ���� �ؽ�Ʈ ���̰� �ִ� ���̿� �����ߴ��� Ȯ��
            if (text.LengthSafe() >= 13)
            {
                return (char)0;
            }

            // ���� ���ڱ����� Ȯ��
            if (IsAllowedCharacter(ch))
            {
                // �Է��� ����ϰ� �ؽ�Ʈ�� �߰�
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
        /// �Էµ� ���ڰ� ���� ���ڱ��� ���ϴ��� Ȯ��
        /// </summary>
        /// <param name="c">�Էµ� ����</param>
        /// <returns>���Ǹ� true, �׷��� ������ false</returns>
        private bool IsAllowedCharacter(char c)
        {
            // ���� �빮��
            if (c >= 'A' && c <= 'Z') return true;
            // ���� �ҹ���
            if (c >= 'a' && c <= 'z') return true;
            // ����
            if (c >= '0' && c <= '9') return true;
            // ���� Ư������
            if (allowedSpecialChars.Contains(c)) return true;

            // �ѱ��̳� �ٸ� ���ڴ� ������� ����
            return false;
        }
    }
}
