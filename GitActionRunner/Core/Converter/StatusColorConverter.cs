using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GitActionRunner.Converters;

public class StatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var status = value?.ToString().ToLower();
        if (string.IsNullOrEmpty(status)) return Brushes.Gray;

        return status switch
        {
                "in_progress" => new SolidColorBrush(Color.FromRgb(255, 215, 0)),  // 골드
                "queued" => new SolidColorBrush(Color.FromRgb(255, 191, 0)),  // 진한 황색
                "success" => Brushes.Green,
                "failure" => Brushes.Red,
                "cancelled" => Brushes.Orange,
                _ => Brushes.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}