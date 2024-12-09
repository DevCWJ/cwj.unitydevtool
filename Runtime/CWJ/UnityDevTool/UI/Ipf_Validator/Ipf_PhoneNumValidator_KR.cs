using UnityEngine;
using TMPro;
using CWJ;

[CreateAssetMenu(fileName = "PhoneNumValidator_KR", menuName = "CWJ/TMP/PhoneNumValidator_KR", order = 1)]
public class Ipf_PhoneNumValidator_KR : TMP_InputValidator
{
    public override char Validate(ref string text, ref int pos, char ch)
    {
        // �Էµ� ���ڰ� �������� Ȯ��
        if (char.IsDigit(ch))
        {
            // �Է� ������ �ִ� ���� 13��(010-1234-5678)
            if (text.LengthSafe() >= 13)
            {
                return (char)0;
            }

            // ���ڸ� �Է��� �� �������� �ʿ��� ��ġ�� ������ �߰�
            if (text.Length == 3 || text.Length == 8)
            {
                text += "-"; // ������ �߰�
                pos += 1; // �������� �� ĭ ����
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

        // ���ڰ� �ƴ� ��� �Է� ����
        return (char)0;
    }
}