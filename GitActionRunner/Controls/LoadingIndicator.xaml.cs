// /Controls/LoadingIndicator.xaml.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GitActionRunner.Controls
{
    public partial class LoadingIndicator : UserControl
    {
        public static readonly DependencyProperty LoadingMessageProperty =
                DependencyProperty.Register("LoadingMessage", typeof(string), 
                                            typeof(LoadingIndicator), 
                                            new PropertyMetadata("Loading..."));

        public static readonly DependencyProperty BackgroundProperty =
                DependencyProperty.Register("Background", typeof(Brush), 
                                            typeof(LoadingIndicator), 
                                            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x25, 0x25, 0x47))));

        public string LoadingMessage
        {
            get => (string)GetValue(LoadingMessageProperty);
            set => SetValue(LoadingMessageProperty, value);
        }

        public new Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public LoadingIndicator()
        {
            InitializeComponent();
        }
    }
}