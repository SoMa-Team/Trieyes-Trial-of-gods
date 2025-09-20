using System;
namespace Utils
{
    public class Parser
    {
        public static bool TryParseIntOrPercent(string s, out int value, out bool isPercent)
        {
            value = 0;
            isPercent = false;
            if (string.IsNullOrWhiteSpace(s)) return false;

            s = s.Trim();
            if (s.EndsWith("%"))
            {
                isPercent = true;
                s = s.Substring(0, s.Length - 1).Trim();
            }

            // 필요하면 천단위 구분자/공백 제거 등 추가
            // s = s.Replace(",", "");

            if (int.TryParse(s, System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out var v))
            {
                value = v;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 퍼센트 허용: 실패 시 예외를 던지는 버전(기존 ParseStrToInt 대체용)
        /// </summary>
        public static int ParseStrToInt(string s)
        {
            if (!TryParseIntOrPercent(s, out var v, out var p))
                throw new FormatException($"Invalid numeric/percent: '{s}'");
            return v;
        }
    }
}