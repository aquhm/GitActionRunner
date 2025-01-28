using System.Windows.Controls;
using System.Windows.Navigation;

namespace GitActionRunner.Core.Interfaces;

public interface INavigationService
{
    void NavigateTo<T>() where T : Page;
    void OnNavigated(object sender, NavigationEventArgs e);
}
