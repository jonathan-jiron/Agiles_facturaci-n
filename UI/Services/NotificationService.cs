namespace UI.Services;

public enum NotificationType
{
    Success,
    Error,
    Warning,
    Info
}

public class NotificationMessage
{
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class NotificationService
{
    public event Action<NotificationMessage>? OnNotification;

    public void ShowSuccess(string message)
    {
        OnNotification?.Invoke(new NotificationMessage
        {
            Message = message,
            Type = NotificationType.Success
        });
    }

    public void ShowError(string message)
    {
        OnNotification?.Invoke(new NotificationMessage
        {
            Message = message,
            Type = NotificationType.Error
        });
    }

    public void ShowWarning(string message)
    {
        OnNotification?.Invoke(new NotificationMessage
        {
            Message = message,
            Type = NotificationType.Warning
        });
    }

    public void ShowInfo(string message)
    {
        OnNotification?.Invoke(new NotificationMessage
        {
            Message = message,
            Type = NotificationType.Info
        });
    }
}
