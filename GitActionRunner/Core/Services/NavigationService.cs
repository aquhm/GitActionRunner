using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using GitActionRunner.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace GitActionRunner.Services
{
    public class NavigationService : INavigationService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Frame _mainFrame;
        private readonly NavigatedEventHandler _navigatedHandler;
        
        private bool _disposed;
        
        public NavigationService(IServiceProvider serviceProvider, Frame mainFrame)
        {
            _serviceProvider = serviceProvider;
            _mainFrame = mainFrame ?? throw new ArgumentNullException(nameof(mainFrame));
            
            _navigatedHandler = (s, e) => OnNavigated(s, e);
            _mainFrame.Navigated += _navigatedHandler;
            
            Log.Debug("NavigationService initialized");
        }

        public void NavigateTo<T>() where T : Page
        {
            if (!_mainFrame.Dispatcher.CheckAccess())
            {
                Log.Debug("Dispatching navigation request to UI thread for {PageType}", typeof(T).Name);
        
                _mainFrame.Dispatcher.BeginInvoke(
                                                  DispatcherPriority.Loaded,
                                                  new Action(() => ExecuteNavigation<T>())
                                                 );
                return;
            }

            ExecuteNavigation<T>();
        }

        private void ExecuteNavigation<T>() where T : Page
        {
            try
            {
                Log.Information("Attempting to navigate to page: {PageType}", typeof(T).Name);
                var page = _serviceProvider.GetRequiredService<T>();
                Log.Debug("Page instance created: {PageType}", typeof(T).Name);

                Log.Debug("Current frame content: {CurrentContent}", _mainFrame.Content?.GetType().Name ?? "null");
                _mainFrame.Content = null;
                Log.Debug("Frame content cleared");

                Log.Debug("Navigating to new page");
                _mainFrame.Navigate(page);
                Log.Information("Successfully navigated to page: {PageType}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Navigation failed to page: {PageType}", typeof(T).Name);
                MessageBox.Show($"화면 전환 중 오류가 발생했습니다: {ex.Message}", 
                                "오류", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
            }
        }

        public async void OnNavigated(object sender, NavigationEventArgs e)
        {
            try 
            {
                Log.Debug("Navigation event received for content: {ContentType}", 
                          e.Content?.GetType().Name ?? "null");
            
                if (e.Content is FrameworkElement element && 
                    element.DataContext is INavigationAware viewModel)
                {
                    Log.Debug("Content DataContext type: {DataContextType}", 
                              element.DataContext.GetType().Name);
                
                    Log.Debug("Calling OnNavigatedTo for viewModel: {ViewModelType}", 
                              viewModel.GetType().Name);

                    try 
                    {
                        await viewModel.OnNavigatedTo();
                        Log.Information("Successfully completed OnNavigatedTo");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to execute OnNavigatedTo");
                        MessageBox.Show($"초기화 중 오류가 발생했습니다: {ex.Message}", 
                                        "오류", 
                                        MessageBoxButton.OK, 
                                        MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Critical error in OnNavigated");
                MessageBox.Show($"치명적인 오류가 발생했습니다: {ex.Message}", 
                                "오류", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
            }
        }

        public void Dispose()
        {
            if (_mainFrame != null)
            {
                _mainFrame.Navigated -= _navigatedHandler;
            }
        }
    }
}