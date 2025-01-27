using System.Windows.Controls;

namespace GitActionRunner.Core.Interfaces;

public interface INavigationService
{
    void NavigateTo<T>() where T : Page;
}
