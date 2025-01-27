using System.Windows;
using System.Windows.Controls;

namespace GitActionRunner.Controls;

public class PlaceholderTextBox : TextBox
{
    public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(PlaceholderTextBox));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
}