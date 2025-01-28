using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using GitActionRunner.Core.Interfaces;
using GitActionRunner.Views;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace GitActionRunner;


public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;

    public MainWindow(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();
        
        // Frame 초기화
        MainFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
        
        // App에 Frame 설정
        App.SetMainFrame(MainFrame);
        
        MainFrame.Navigated += MainFrame_Navigated;
        
        // Frame 설정 후 NavigationService 가져오기
        var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
        
        // 초기 화면으로 이동
        navigationService.NavigateTo<GitHubLoginView>();
    }
    
    private void MainFrame_Navigated(object sender, NavigationEventArgs e)
    {
        if (e.Content is GitHubLoginView)
        {
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Width = 400;
            Height = 300;
        }
        else if (e.Content is RepositoryListView)
        {
            ResizeMode = ResizeMode.CanResize;
            SizeToContent = SizeToContent.Manual;
            Width = 780;
            Height = 500;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
    
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }
        else
            DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized 
                ? WindowState.Normal 
                : WindowState.Maximized;
    }
    
    private void Window_ContentRendered(object sender, EventArgs e)
    {
        Log.Information("Window content rendered");
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}