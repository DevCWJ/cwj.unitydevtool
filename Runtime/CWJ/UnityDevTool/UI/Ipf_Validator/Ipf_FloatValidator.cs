using UnityEngine;

namespace CWJ
{

    [CreateAssetMenu(fileName = "FloatValidator", menuName = "CWJ/TMP/FloatValidator", order = int.MaxValue)]
    [System.Serializable]
    public class Ipf_FloatValidator : TMPro.TMP_InputValidator
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
                    if (floatLength <= 0)
                        floatLength = 1;
                    string minStr = StringUtil.ConvertToDecimalLength(minValue, -1, floatLength);
                    string maxStr = StringUtil.ConvertToDecimalLength(maxValue, -1, floatLength);
                    string maxLengthStr = (minStr.Length > maxStr.Length) ? minStr : maxStr;
                    _maxLength = maxLengthStr.Length;
                }
                return _maxLength;
            }
        }


        public float minValue = -360f;
        public float maxValue = 360f;
        public int floatLength = 2;

        //e : [+-]?(\d+([.]\d*)?([eE][+-]?\d+)?|[.]\d+([eE][+-]?\d+)?)
        //non e: [+-]?(\d+([.]\d*)?(e[+-]?\d+)?|[.]\d+(e[+-]?\d+)?)
        public override char Validate(ref string text, ref int pos, char ch)
        {
            //if (!Regex.IsMatch(ch.ToString(), @"[+-]?(\d+([.]\d*)?(e[+-]?\d+)?|[.]\d+(e[+-]?\d+)?)", RegexOptions.IgnoreCase)
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
                bool isDotWithIntFront = ch.Equals('.') && int.TryParse(text, out int frontInt);
                if (isDotWithIntFront == false)
                {
                    if (!float.TryParse(tmp, out float val) || (val < minValue || val > maxValue))
                        return (char)0;
                }
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