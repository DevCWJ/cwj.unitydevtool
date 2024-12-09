
using UnityEngine;
namespace CWJ
{
    [CreateAssetMenu(fileName = "IntValidator", menuName = "CWJ/TMP/IntValidator", order = int.MaxValue)]
    [System.Serializable]
    public class Ipf_IntValidator : TMPro.TMP_InputValidator
    {
        private bool hasMinus = false;
        private int _maxLength = -1;
        public int maxLength
        {
            get
            {
                if (_maxLength <= 0)
                {
                    hasMinus = minValue < 0;
                    _maxLength = Mathf.Max(minValue.ToString().Length, maxValue.ToString().Length);
                }
                return _maxLength;
            }
        }

        public int minValue = -10;
        public int maxValue = 10;

        public override char Validate(ref string text, ref int pos, char ch)
        {
            //if (!Regex.IsMatch(ch.ToString(), @"^[0-9]+$", RegexOptions.IgnoreCase)
            //    || text.LengthSafe() >= maxLength)
            //    return (char)0;
            if (text.LengthSafe() >= maxLength)
            {
                return (char)0;
            }

            string tmp = text == null ? ch.ToString() : text + ch;

            bool isMinus = hasMinus && tmp.Equals("-");

            if (isMinus == false)
            {
                if (!int.TryParse(tmp, out int val) || (val < minValue || val > maxValue))
                    return (char)0;
            }

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
            return (char)0;
        }
    }

}