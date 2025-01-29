using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using GitActionRunner.Core;
using GitActionRunner.ViewModels;
using Serilog; // ViewModel namespace 추가

namespace GitActionRunner.Views
{
    public partial class RepositoryListView : Page
    {
        public RepositoryListView(RepositoryListViewModel viewModel)
        {
            Log.Information("Initializing RepositoryListView");
        
            try
            {
                InitializeComponent();
                DataContext = viewModel;
                Log.Information("RepositoryListView initialized successfully");
            
                Loaded += RepositoryListView_Loaded;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error initializing RepositoryListView");
                throw;
            }
        }
        
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPopup.IsOpen = !SettingsPopup.IsOpen;
        }
        
        private void RepositoryListView_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Information("RepositoryListView loaded");
            
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.EnableResize();
            }
        }
    }
}