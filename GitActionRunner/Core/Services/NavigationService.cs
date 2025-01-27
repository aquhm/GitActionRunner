using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using GitActionRunner.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GitActionRunner.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Frame _mainFrame;
        
        public NavigationService(IServiceProvider serviceProvider, Frame mainFrame)
        {
            _serviceProvider = serviceProvider;
            _mainFrame = mainFrame ?? throw new ArgumentNullException(nameof(mainFrame));
        }

        public void NavigateTo<T>() where T : Page
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => NavigateTo<T>()));
                return;
            }

            try
            {
                var page = _serviceProvider.GetRequiredService<T>();
                _mainFrame.Content = null;
                _mainFrame.Navigate(page);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                MessageBox.Show($"화면 전환 중 오류가 발생했습니다: {ex.Message}", 
                                "오류", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
            }
        }

        private void NavigateInternal<T>() where T : Page
        {
            var page = _serviceProvider.GetRequiredService<T>();
            _mainFrame.Content = null;
            _mainFrame.Navigate(page);
        }
    }
}