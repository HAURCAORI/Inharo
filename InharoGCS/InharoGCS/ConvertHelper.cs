using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace InharoGCS
{
    public class BoolToBrushConverter : BoolToValueConverter<Brush> { }
    public class BoolToStringConverter : BoolToValueConverter<String> { }
    public class BoolToValueConverter<T> : IValueConverter
    {
        public T? FalseValue { get; set; }
        public T? TrueValue { get; set; }

        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return FalseValue;
            else
                return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? value.Equals(TrueValue) : false;
        }
    }

    public class DataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) { return "-"; }
            else if (value is byte[])
                return ConvertHelper.ByteToHexStringWithSpace((byte[])value);
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null) { return "-";  }
            if (value is ushort)
                return ConvertHelper.ToHexString((ushort)value);
            else if (value is ulong)
                return ConvertHelper.ToHexString((ulong)value);
            else 
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal static class SearchChild
    {
        public static T GetChildOfType<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
    internal class ConvertHelper
    {
        public static string ByteToHexString(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }

        public static string ByteToHexStringWithSpace(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 3];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 3] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 3 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
                c[i * 3 + 2] = ' ';
            }
            return new string(c);
        }

        public static byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace(" ", "");
            if (hex.Length % 2 == 1)
            {
                hex = "0" + hex;
            }
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string ToHexString(ushort value)
        {
            return ByteToHexString(BitConverter.GetBytes(value));
        }

        public static string ToHexString(ulong value)
        {
            return ByteToHexString(BitConverter.GetBytes(value));
        }

    }
}
