using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UTools
{
    public static class UNumericExtensions
    {
        public static float Map(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            if (Mathf.Approximately(fromMin, fromMax))
            {
                Debug.LogWarning("Cannot map a value from a zero-length range.");
                return toMin;
            }

            float normalizedValue = Mathf.InverseLerp(fromMin, fromMax, value);
            return Mathf.Lerp(toMin, toMax, normalizedValue);
        }
    }

    public static class UStringExtensions
    {
        private static readonly Regex UserNamePattern = new(@"^[a-zA-Z0-9_][A-Za-z0-9_]*$", RegexOptions.Compiled);
        private static readonly Regex ChineseTextPattern = new(@"^[\u4e00-\u9fa5]*$", RegexOptions.Compiled);

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNotNullOrEmpty(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static bool CheckUserName(this string input)
        {
            return !string.IsNullOrEmpty(input) && UserNamePattern.IsMatch(input);
        }

        public static string TrimLength(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || maxLength <= 0)
            {
                return string.Empty;
            }

            if (value.Length <= maxLength)
            {
                return value;
            }

            if (maxLength <= 3)
            {
                return value.Substring(0, maxLength);
            }

            return $"{value.Substring(0, maxLength - 3)}...";
        }

        public static bool CheckStrChinese(this string input)
        {
            return input != null && ChineseTextPattern.IsMatch(input);
        }

        public static bool IsIPAddress(this string input)
        {
            return IPAddress.TryParse(input?.Trim(), out IPAddress address)
                && address.AddressFamily == AddressFamily.InterNetwork;
        }

        public static string ToBase64String(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }
    }

    public static class UTimeExtensions
    {
        private const string ShortTimeFormat = @"mm\:ss";
        private const string LongTimeFormat = @"hh\:mm\:ss";
        private const string ShortChineseTimeFormat = "mm\\\u5206ss\\\u79D2";
        private const string LongChineseTimeFormat = "hh\\\u5C0F\u65F6mm\\\u5206ss\\\u79D2";

        public static string ToTimeString(this int seconds, bool useChinese = false, string format = null)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            string resolvedFormat = format ?? ResolveTimeFormat(time, useChinese);
            return time.ToString(resolvedFormat);
        }

        public static string ToHhMmSsString(this TimeSpan timeSpan, bool useChinese = false)
        {
            return timeSpan.ToString(ResolveTimeFormat(timeSpan, useChinese));
        }

        public static bool TryCalculateTimeSpan(this string startTimeText, string endTimeText, out TimeSpan timeSpan)
        {
            if (DateTime.TryParse(startTimeText, out DateTime startTime)
                && DateTime.TryParse(endTimeText, out DateTime endTime))
            {
                timeSpan = endTime - startTime;
                return true;
            }

            timeSpan = TimeSpan.Zero;
            return false;
        }

        public static TimeSpan CalculateTimeSpan(this string startTimeText, string endTimeText)
        {
            return startTimeText.TryCalculateTimeSpan(endTimeText, out TimeSpan timeSpan)
                ? timeSpan
                : TimeSpan.Zero;
        }

        private static string ResolveTimeFormat(TimeSpan timeSpan, bool useChinese)
        {
            if (timeSpan.TotalHours >= 1d)
            {
                return useChinese ? LongChineseTimeFormat : LongTimeFormat;
            }

            return useChinese ? ShortChineseTimeFormat : ShortTimeFormat;
        }
    }

    public static class UEnumExtensions
    {
        public static string ToLocalizedString(this Enum value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            System.Reflection.FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = field == null
                ? null
                : Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

            return attribute?.Description ?? value.ToString();
        }
    }

    public static class UColorExtensions
    {
        public static Color32 ConvertColorToColor32(this Color color)
        {
            return color;
        }

        public static Color ConvertColor32ToColor(this Color32 color)
        {
            return color;
        }
    }
}
