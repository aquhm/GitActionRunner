using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Navigation;
using GitActionRunner.Core.Interfaces;
using GitActionRunner.ViewModels;
using GitActionRunner.Views;
using Microsoft.Extensions.DependencyInjection;
using NavigationService = GitActionRunner.Services.NavigationService;

namespace GitActionRunner;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;

    public MainWindow(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        
        // Frame 초기화
        MainFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
        
        // App에 Frame 설정
        App.SetMainFrame(MainFrame);
        
        // NavigationService 가져오기
        _navigationService = serviceProvider.GetRequiredService<INavigationService>();
        
        // 초기 화면으로 이동
        _navigationService.NavigateTo<GitHubLoginView>();
    }
}