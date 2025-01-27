using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using GitActionRunner.Core;
using GitActionRunner.ViewModels;  // ViewModel namespace 추가

namespace GitActionRunner.Views
{
    public partial class GitHubLoginView : Page
    {
        // public GitHubLoginView()
        // {
        //     InitializeComponent();
        //     DataContext = App.ServiceProvider.GetService<GitHubLoginViewModel>();
        // }
        
        public GitHubLoginView(GitHubLoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}