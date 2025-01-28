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

        public static readonly DependencyProperty ShowBackgroundProperty =
                DependencyProperty.Register("ShowBackground", typeof(bool), 
                                            typeof(LoadingIndicator), 
                                            new PropertyMetadata(false));

        public string LoadingMessage
        {
            get => (string)GetValue(LoadingMessageProperty);
            set => SetValue(LoadingMessageProperty, value);
        }

        public bool ShowBackground
        {
            get => (bool)GetValue(ShowBackgroundProperty);
            set => SetValue(ShowBackgroundProperty, value);
        }

        public LoadingIndicator()
        {
            InitializeComponent();
        }
    }
}