using CommunityToolkit.Mvvm.ComponentModel;

namespace GitActionRunner.ViewModels;

public abstract class BaseViewModel : ObservableObject
{
    private bool _isLoading;
    private string _loadingMessage;

    public bool IsLoading
    {
        get => _isLoading;
        protected set => SetProperty(ref _isLoading, value);
    }

    public string LoadingMessage
    {
        get => _loadingMessage;
        protected set => SetProperty(ref _loadingMessage, value);
    }

    public async Task ExecuteWithLoadingAsync(Func<Task> action, string loadingMessage = "Loading data...")
    {
        try
        {
            LoadingMessage = loadingMessage;
            IsLoading = true;
            await action();
        }
        finally
        {
            IsLoading = false;
            LoadingMessage = string.Empty;
        }
    }
}