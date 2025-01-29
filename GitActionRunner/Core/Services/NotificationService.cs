using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using DesktopNotifications;
using DesktopNotifications.Windows;

namespace GitActionRunner.Core.Services;

public class NotificationService
{
    private static INotificationManager _notificationManager;
    private static INotificationManager CreateManager()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsNotificationManager();
        }

        throw new PlatformNotSupportedException();
    }

    public static async Task Initialize()
    {
        if (_notificationManager == default)
        {
            _notificationManager = CreateManager();
            await _notificationManager.Initialize();

            _notificationManager.NotificationActivated += ManagerOnNotificationActivated;
            _notificationManager.NotificationDismissed += ManagerOnNotificationDismissed;
        }
    }

    public static async Task ShowToast(string title, string message)
    {
        try
        {
            if (_notificationManager == null)
            {
                throw new InvalidOperationException("Notification manager not initialized.");
            }

            var notification = new Notification
            {
                Title = title,
                Body = message
            };

            await _notificationManager.ShowNotification(notification);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Toast Error: {ex.Message}");
            MessageBox.Show($"Failed to show toast: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private static void ManagerOnNotificationDismissed(object? sender, NotificationDismissedEventArgs e)
    {
        Console.WriteLine($"Notification dismissed: {e.Reason}");
    }

    private static void ManagerOnNotificationActivated(object? sender, NotificationActivatedEventArgs e)
    {
        Console.WriteLine($"Notification activated: {e.ActionId}");
    }

}