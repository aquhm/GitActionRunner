using System.Windows;
using System.Windows.Controls;
using DesktopNotifications.Windows;
using GitActionRunner.Converters;
using GitActionRunner.Core.Interfaces;
using GitActionRunner.Core.Models;
using GitActionRunner.Core.Services;
using GitActionRunner.Services;
using GitActionRunner.ViewModels;
using GitActionRunner.Views;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace GitActionRunner
{
    public partial class App : Application
    {
        private static Frame _mainNavigationFrame;
        private static IServiceCollection _services;
        public static IServiceProvider ServiceProvider { get; private set; }
        
        public App()
        {
            // UI Thread의 처리되지 않은 예외 처리
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        
            // Non-UI Thread의 처리되지 않은 예외 처리
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        
            // Task에서 처리되지 않은 예외 처리
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }
    
        public static void SetMainFrame(Frame frame)
        {
            _mainNavigationFrame = frame;
            if (_services != null)
            {
                // 기존 서비스 프로바이더를 재구성
                ServiceProvider = _services.BuildServiceProvider();
            }
        }
        
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "Unhandled exception in UI thread");
            MessageBox.Show($"오류가 발생했습니다: {e.Exception.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Log.Fatal(exception, "Unhandled exception in AppDomain");
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Error(e.Exception, "Unhandled exception in Task");
            e.SetObserved();
        }
    
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LoggerSetup.Configure(LogEventLevel.Verbose);

            NotificationService.Initialize().Wait();
            
            _services = new ServiceCollection();
            ConfigureServices(_services);
            ServiceProvider = _services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Application shutting down");
            base.OnExit(e);
            Log.CloseAndFlush();
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
        
            services.AddSingleton<INavigationService>(provider =>
                                                               new NavigationService(provider, _mainNavigationFrame));
        }
    }
}