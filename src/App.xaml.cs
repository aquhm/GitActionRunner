using System.Configuration;
using System.Data;
using System.Windows;
using GitActionRunner.Core.Interfaces;
using GitActionRunner.Core.Models;
using GitActionRunner.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace src;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        ServiceProvider = serviceCollection.BuildServiceProvider();
        base.OnStartup(e);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISecureStorage, SecureStorage>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IGitHubService, GitHubService>();
        
        services.AddSingleton<GitHubLoginViewModel>();
        
        services.AddTransient(typeof(MainWindow));
    }
}