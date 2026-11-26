using System;
using System.Globalization;

namespace Game.Common
{
    public static class NumberFormatter
    {
        private static readonly string[] Suffixes = { "", "만", "억", "조", "경", "해", "자", "양" };
        private const float DiffNum = 10000;

        public static string ToCompact(double value, int decimalPlaces = 4)
        {
            if (value == 0)
                return "0";

            if (value <= DiffNum)
            {
                return value.ToString("F0");
            }

            int suffixIndex = 0;

            while (Math.Abs(value) >= DiffNum && suffixIndex < Suffixes.Length - 1)
            {
                value /= DiffNum;
                suffixIndex++;
            }

            // 소수점 자리수 적용
            string format = "F" + decimalPlaces;
            return value.ToString(format, CultureInfo.InvariantCulture).Replace(".", Suffixes[suffixIndex]) + Suffixes[suffixIndex - 1];
        }
    }
}
