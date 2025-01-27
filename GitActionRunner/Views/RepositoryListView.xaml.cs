using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using GitActionRunner.Core;
using GitActionRunner.ViewModels;  // ViewModel namespace 추가

namespace GitActionRunner.Views
{
    public partial class RepositoryListView : Page
    {
        public RepositoryListView(RepositoryListViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}