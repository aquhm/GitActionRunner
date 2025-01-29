using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace GitActionRunner.Controls;

public class CustomToast : Window
{
    private readonly DispatcherTimer _timer;
    private readonly MediaPlayer _soundPlayer;

    public CustomToast(string message, bool isSuccess)
    {
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        SizeToContent = SizeToContent.WidthAndHeight;
        Topmost = true;
        ShowInTaskbar = false;

        // 효과음 재생
        _soundPlayer = new MediaPlayer();
        _soundPlayer.Open(new Uri(isSuccess ? 
            "pack://application:,,,/Sounds/success.wav" : 
            "pack://application:,,,/Sounds/notification.wav"));
        _soundPlayer.Play();

        var border = new Border
        {
            Background = new SolidColorBrush(isSuccess ? 
                Color.FromRgb(40, 167, 69) : 
                Color.FromRgb(220, 53, 69)),
            CornerRadius = new CornerRadius(4),
            Margin = new Thickness(0, 0, 10, 10),
            Padding = new Thickness(15),
            MinWidth = 300,
            MaxWidth = 400,
            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Opacity = 0.5,
                BlurRadius = 10
            }
        };

        var contentStack = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        contentStack.Children.Add(new TextBlock
        {
            Text = isSuccess ? "Success" : "Completed",
            FontWeight = FontWeights.Bold,
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 5),
            Foreground = Brushes.White
        });

        contentStack.Children.Add(new TextBlock
        {
            Text = message,
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap
        });

        border.Child = contentStack;
        Content = border;

        // 진입 애니메이션
        Opacity = 0;
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        BeginAnimation(OpacityProperty, fadeIn);

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _timer.Tick += Timer_Tick;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - ActualWidth - 20;
        Top = workArea.Bottom - ActualHeight - 20;

        _timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        _timer.Stop();
        
        // 퇴장 애니메이션
        var fadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(200)
        };
        fadeOut.Completed += (s, _) => Close();
        BeginAnimation(OpacityProperty, fadeOut);
    }
}