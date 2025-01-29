using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GitActionRunner.Converters;

public class StatusVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var status = value?.ToString().ToLower();
        if (string.IsNullOrEmpty(status) || status.Equals("queued (pending)"))
            return Visibility.Collapsed;

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}