using System;

namespace CSharpExtensions.Core
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);

        public static short ToInt16(this string input) => Convert.ToInt16(input);

        public static int ToInt32(this string input) => Convert.ToInt32(input);

        public static long ToInt64(this string input) => Convert.ToInt64(input);

        public static bool ToBoolean(this string input) => Convert.ToBoolean(input);
    }
}
