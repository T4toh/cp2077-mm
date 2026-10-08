using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using NexusMods.UI.Sdk;
using NexusMods.UI.Sdk.Dialog;

namespace NexusMods.App.UI.Notifications;

public class WindowNotificationService : IWindowNotificationService
{
    private WindowNotificationManager? _notificationManager;

    /// <summary>
    /// Lazy initialization, as main window may not available at creation time
    /// Needs to be called on UI thread
    /// </summary>
    private WindowNotificationManager? GetNotificationManager()
    {
        if (_notificationManager != null)
            return _notificationManager;

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: not null } desktopLifetime)
        {
            // Unable to access the main window, so we cannot show notifications
            return null;
        }
        
        // Must be on UI thread to create the WindowNotificationManager
        _notificationManager = new WindowNotificationManager(desktopLifetime.MainWindow)
        {
            Position = NotificationPosition.BottomCenter,
            MaxItems = 4,
        };
            
        return _notificationManager;
    }

    /// <Inheritdoc />
    public void ShowToast(
        string message,
        ToastNotificationVariant type = ToastNotificationVariant.Neutral,
        TimeSpan? expiration = null,
        DialogButtonDefinition[]? buttonDefinitions = null,
        Action<ButtonDefinitionId>? buttonHandler = null)
    {
        DispatcherHelper.EnsureOnUIThread(() =>
            {
                var isNew = _notificationManager is null;
                var manager = GetNotificationManager();
                if (manager == null) return;

                // TODO: Use buttons and handler

                var notification = new Notification(
                    null,
                    message,
                    type switch
                    {
                        ToastNotificationVariant.Neutral => NotificationType.Information,
                        ToastNotificationVariant.Success => NotificationType.Success,
                        ToastNotificationVariant.Failure => NotificationType.Error,
                    },
                    expiration ?? TimeSpan.FromSeconds(type == ToastNotificationVariant.Failure ? 10 : 5));

                // Must be on UI thread to show the notification. A manager created just now has no template yet
                // (it gets one on the next layout pass) and drops whatever it is shown, so the first toast waits
                if (isNew) Dispatcher.UIThread.Post(() => manager.Show(notification), DispatcherPriority.Background);
                else manager.Show(notification);
            }
        );
    }
}


