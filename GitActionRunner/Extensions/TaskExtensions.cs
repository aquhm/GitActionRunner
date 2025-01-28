using GitActionRunner.ViewModels;

namespace GitActionRunner.Extensions;

public static class TaskExtensions
{
    public static async Task WithLoading(this Task task, BaseViewModel viewModel, string loadingMessage)
    {
        await viewModel.ExecuteWithLoadingAsync(() => task, loadingMessage);
    }
}