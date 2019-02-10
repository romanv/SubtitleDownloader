namespace RV.SubD.Shell.Core
{
    using System;
    using System.Globalization;
    using System.Windows;

    public class BoolToVisibility : ConverterBase<BoolToVisibility>
    {
        public bool IsInverted { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var val = (bool)value;

            if (IsInverted)
            {
                return val ? Visibility.Collapsed : Visibility.Visible;
            }

            return val ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
