using System;
using System.Text;
using System.Collections.Generic;

namespace CWJ
{
    public static class ByteUtil
    {
        public static byte[] ConvertHexStringToByteArray(string hexString, char separator = ' ')
        {
            if (string.IsNullOrWhiteSpace(hexString))
            {
                throw new ArgumentException("Input string is null or empty.");
            }

            // 공백을 기준으로 문자열 분리
            string[] hexValues = hexString.Split(separator);

            // 16진수 문자열을 byte 배열로 변환
            byte[] byteArray = hexValues.ConvertAll(hex => byte.Parse(hex, System.Globalization.NumberStyles.HexNumber));

            return byteArray;
        }

        public static string ToHexStrWithLine(IList<byte> bytes, int byteLength = -1, bool toLowerCase = false, char separator = ' ')
        {
            return StringUtil.ToHexStrWithLine(bytes, byteLength, toLowerCase, separator);
        }

        public static byte[] GetBytesWithLength(this Encoding encoding, string str, out int byteLength)
        {
            return GetBytesWithLength(encoding, str, str.Length, out byteLength);
        }

        public static byte[] GetBytesWithLength(this Encoding encoding, string str, int strLength, out int byteLength)
        {
            byteLength = 0;
            if (str == null)
            {
                return null;
            }

            int byteCount = encoding.GetByteCount(str, 0, strLength);
            byte[] bytes = new byte[byteCount];
            byteLength = encoding.GetBytes(str, 0, strLength, bytes, 0);
            return bytes;
        }

        public static bool SplitInBytes(this byte[] src, int srcLength, byte[] foundBytes, int foundByteLength, out byte[] splitRightBytes, out int splitRightLength)
        {
            splitRightBytes = null;
            splitRightLength = 0;

            if (src == null || foundBytes == null || srcLength == 0 || foundByteLength == 0 || foundByteLength > srcLength)
            {
                return false;
            }

            int index = IndexOfInBytes(src, srcLength, foundBytes, foundByteLength);

            if (index == -1)
            {
                return false;
            }

            splitRightLength = srcLength - index - foundByteLength;
            splitRightBytes = new byte[splitRightLength];

            Array.Copy(src, index + foundByteLength, splitRightBytes, 0, splitRightLength);

            return true;
        }

        public static bool SplitInBytes(this byte[] src, int srcLength, byte[] foundBytes, int foundByteLength, out byte[] leftBytes, out int leftLength, out byte[] rightBytes, out int rightLength)
        {
            leftBytes = null;
            rightBytes = null;
            leftLength = 0; rightLength = 0;

            if (src == null || foundBytes == null || srcLength == 0 || foundByteLength == 0 || foundByteLength > srcLength)
            {
                return false;
            }

            int index = IndexOfInBytes(src, srcLength, foundBytes, foundByteLength);

            if (index == -1)
            {
                leftBytes = src;
                return false;
            }

            leftLength = index;
            leftBytes = new byte[leftLength];

            rightLength = srcLength - index - foundByteLength;
            rightBytes = new byte[rightLength];

            Array.Copy(src, 0, leftBytes, 0, leftLength);
            Array.Copy(src, index + foundByteLength, rightBytes, 0, rightLength);
            return true;
        }

        public static bool SplitLastInBytes(this byte[] src, int srcLength, byte[] foundBytes, int foundByteLength, out byte[] rightBytes, out int rightLength)
        {
            rightBytes = null;
            rightLength = 0;
            if (src == null || foundBytes == null || srcLength == 0 || foundByteLength == 0 || foundByteLength > srcLength)
            {
                return false;
            }

            int index = LastIndexOfInBytes(src, srcLength, foundBytes, foundByteLength);

            if (index == -1)
            {
                return false;
            }

            rightLength = srcLength - index - foundByteLength;
            rightBytes = new byte[rightLength];

            Array.Copy(src, index + foundByteLength, rightBytes, 0, rightLength);
            return true;
        }

        public static bool SplitLastInBytes(this byte[] src, int srcLength, byte[] foundBytes, int foundByteLength, out byte[] leftBytes, out int leftLength, out byte[] rightBytes, out int rightLength)
        {
            leftBytes = null;
            rightBytes = null;
            leftLength = 0; rightLength = 0;
            if (src == null || foundBytes == null || srcLength == 0 || foundByteLength == 0 || foundByteLength > srcLength)
            {
                return false;
            }

            int index = LastIndexOfInBytes(src, srcLength, foundBytes, foundByteLength);

            if (index == -1)
            {
                leftBytes = src;
                return false;
            }

            leftLength = index;
            leftBytes = new byte[index];
            rightLength = srcLength - index - foundByteLength;
            rightBytes = new byte[srcLength - index - foundByteLength];

            Array.Copy(src, 0, leftBytes, 0, leftLength);
            Array.Copy(src, index + foundByteLength, rightBytes, 0, rightLength);
            return true;
        }

        public static byte[] RemoveFromEnd(this byte[] src, int srcLength, byte[] removeBytes, int removeByteLength, out int resultLength)
        {
            resultLength = -1;
            if (src == null || removeBytes == null || srcLength == 0 || removeByteLength == 0 || removeByteLength > srcLength)
            {
                return src;
            }


            int index = LastIndexOfInBytes(src, srcLength, removeBytes, removeByteLength);

            if (index == -1)
            {
                return src;
            }

            resultLength = index;
            byte[] result = new byte[resultLength];
            Array.Copy(src, 0, result, 0, resultLength);

            return result;
        }

        public static int LastIndexOfInBytes(this byte[] src, int srcLength, byte[] foundBytes,  int foundBLength)
        {
            for (int i = srcLength - foundBLength; i >= 0; i--)
            {
                bool found = true;
                for (int j = 0; j < foundBLength; j++)
                {
                    if (src[i + j] != foundBytes[j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    return i;
                }
            }

            return -1;
        }

        public static int IndexOfInBytes(this byte[] src, int srcLength, byte[] foundBytes, int foundBLength)
        {
            int limit = srcLength - foundBLength;

            for (int i = 0; i <= limit; i++)
            {
                bool found = true;
                for (int j = 0; j < foundBLength; j++)
                {
                    if (src[i + j] != foundBytes[j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    return i;
                }
            }

            return -1;
        }

        public static byte[] ReplaceBytes(byte[] src, int srcLength, byte[] search, int searchLength, byte[] replace, int replaceLength)
        {
            if (replace == null) return src;
            int index = FindIndexBytesInBytes(src, srcLength, search, searchLength);
            if (index < 0) return src;
            byte[] dst = new byte[srcLength - searchLength + replaceLength];
            Buffer.BlockCopy(src, 0, dst, 0, index);
            Buffer.BlockCopy(replace, 0, dst, index, replaceLength);
            Buffer.BlockCopy(src, index + searchLength, dst, index + replaceLength, srcLength - (index + searchLength));
            return dst;
        }

        public static int FindIndexBytesInBytes(byte[] src, int srcLenght, byte[] find, int findLength)
        {
            if (src == null || find == null || srcLenght == 0 || findLength == 0 || findLength > srcLenght) return -1;
            int endIndex = srcLenght - findLength + 1;
            int foundSuccessIndex = findLength - 1;
            byte findStartPoint = find[0];
            for (int i = 0; i < endIndex; i++)
            {
                if (src[i] == findStartPoint)
                {
                    for (int j = 1; j < findLength; j++)
                    {
                        if (src[i + j] != find[j]) break;
                        if (j == foundSuccessIndex) return i;
                    }
                }
            }
            return -1;
        }
    }
}
