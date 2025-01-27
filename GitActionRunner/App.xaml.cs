using System.Windows;
using System.Windows.Controls;
using GitActionRunner.Core.Interfaces;
using GitActionRunner.Core.Models;
using GitActionRunner.Core.Services;
using GitActionRunner.Services;
using GitActionRunner.ViewModels;
using GitActionRunner.Views;
using Microsoft.Extensions.DependencyInjection;

namespace GitActionRunner
{
    public partial class App : Application
    {
        private static Frame _mainNavigationFrame;
        private static IServiceCollection _services;
        public static IServiceProvider ServiceProvider { get; private set; }
    
        public static void SetMainFrame(Frame frame)
        {
            _mainNavigationFrame = frame;
            if (_services != null)
            {
                // 기존 서비스 프로바이더를 재구성
                ServiceProvider = _services.BuildServiceProvider();
            }
        }
    
        protected override void OnStartup(StartupEventArgs e)
        {
            _services = new ServiceCollection();
            ConfigureServices(_services);
            ServiceProvider = _services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Core Services
            services.AddSingleton<ISecureStorage, SecureStorage>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IGitHubService, GitHubService>();
        
            // ViewModels
            services.AddSingleton<GitHubLoginViewModel>();
            services.AddSingleton<RepositoryListViewModel>();
        
            // Views
            services.AddTransient<MainWindow>();
            services.AddTransient<GitHubLoginView>();
            services.AddTransient<RepositoryListView>();
        
            // Navigation Service를 singleton으로 등록
            services.AddSingleton<INavigationService>(provider =>
                                                              new NavigationService(provider, _mainNavigationFrame));
        }
    }
}