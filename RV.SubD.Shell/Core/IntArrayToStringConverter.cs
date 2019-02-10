namespace RV.SubD.Shell.Core
{
    using System;
    using System.Globalization;
    using System.Linq;

    public class IntArrayToStringConverter : ParameterlessConverterBase<IntArrayToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Join(", ", ((int[])value).Select(e => e.ToString()));
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse((string)value, out var result))
            {
                return new[] { result };
            }

            return new[] { 1 };
        }
    }
}
