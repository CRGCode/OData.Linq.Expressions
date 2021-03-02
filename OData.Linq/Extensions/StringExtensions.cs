﻿using System.Linq;

namespace OData.Linq.Extensions
{
    static class StringExtensions
    {
        public static bool IsAllUpperCase(this string str)
        {
            return !str.Any(char.IsLower);
        }

        public static string NullIfWhitespace(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? null : str;
        }

        public static string OrDefault(this string str, string defaultValue)
        {
            return str ?? defaultValue;
        }

        public static string EnsureStartsWith(this string source, string value)
        {
            return (source == null || source.StartsWith(value)) ? source : value + source;
        }
    }
}
