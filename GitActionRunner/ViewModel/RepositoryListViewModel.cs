﻿using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using GitActionRunner.Core.Interfaces;
using GitActionRunner.Core.Models;
using GitActionRunner.Views;
using System.Windows;
using GitActionRunner.Controls;
using GitActionRunner.Core.Services;
using Microsoft.Toolkit.Uwp.Notifications;
// using Microsoft.Windows.AppNotifications;
// using Microsoft.Windows.AppNotifications.Builder;
using Serilog;


namespace GitActionRunner.ViewModels
{
    public class RepositoryListViewModel : BaseViewModel, INavigationAware
    {
        private readonly IGitHubService _gitHubService;
        private readonly IAuthenticationService _authService;
        private readonly INavigationService _navigationService;
        private ObservableCollection<Repository> _repositories;
        private ConcurrentDictionary<string, WorkflowRun> _activeRuns = new();
        private Repository _selectedRepository;
        
        private ObservableCollection<string> _branches;
        private string _selectedBranch;
        private WorkflowRun _selectedWorkflow;
        private int _workflowCount;
        
        
        private string _userName;
        
        public ICommand RunWorkflowCommand { get; }
        public ICommand LogoutCommand { get; }
        
        public ObservableCollection<string> Branches
        {
            get => _branches;
            set => SetProperty(ref _branches, value);
        }
    
        public string SelectedBranch
        {
            get => _selectedBranch;
            set => SetProperty(ref _selectedBranch, value);
        }
        
        public WorkflowRun SelectedWorkflow
        {
            get => _selectedWorkflow;
            set => SetProperty(ref _selectedWorkflow, value);
        }
        
        public int WorkflowCount
        {
            get => _workflowCount;
            set => SetProperty(ref _workflowCount, value);
        }

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }
        
        public ObservableCollection<Repository> Repositories
        {
            get => _repositories;
            set => SetProperty(ref _repositories, value);
        }
        
        public Repository SelectedRepository
        {
            get => _selectedRepository;
            set
            {
                if (SetProperty(ref _selectedRepository, value))
                {
                    LoadWorkflows(value);
                }
            }
        }
        
        public ObservableCollection<WorkflowRun> Workflows { get; private set; }
        
        public RepositoryListViewModel(
                IGitHubService gitHubService, 
                IAuthenticationService authService,
                INavigationService navigationService)
        {
            _gitHubService = gitHubService;
            _authService = authService;
            _navigationService = navigationService;

            Repositories = new ObservableCollection<Repository>();
            Workflows = new ObservableCollection<WorkflowRun>();
            Branches = new ObservableCollection<string>();
        
            LogoutCommand = new AsyncRelayCommand(ExecuteLogout);
            RunWorkflowCommand = new AsyncRelayCommand<WorkflowRun>(ExecuteWorkflow, canExecute: (workflow) => workflow != null);
        }
        
        private async Task InitializeAsync()
        {
            try
            {
                Log.Debug("Clearing collections");
                Repositories?.Clear();
                Workflows?.Clear();
                Branches?.Clear();
        
                await ExecuteWithLoadingAsync(async () =>
                {
                    await LoadUserInfo();
                    await LoadRepositories();
                }, "Loading repository information...");
        
                Log.Information("InitializeAsync completed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in InitializeAsync");
                MessageBox.Show($"A critical error occurred during initialization: {ex.Message}", 
                                "오류", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                _navigationService.NavigateTo<GitHubLoginView>(); // 로그인 화면으로 복귀
            }
        }
        
        private async Task LoadUserInfo()
        {
            Log.Information("Loading user info");
            try
            {
                var user = await _gitHubService.GetCurrentUser();
                UserName = user?.Name ?? user?.Login;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading user info: {ex}");
                MessageBox.Show("Failed to load user information", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteLogout()
        {
            try
            {
                await _authService.SaveAccessTokenAsync(null);
                Repositories?.Clear();
                Workflows?.Clear();
                _navigationService.NavigateTo<GitHubLoginView>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during logout: {ex.Message}", 
                                "오류", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
            }
        }
        
        private async Task LoadRepositories()
        {
            Log.Information("Loading repositories");
            try
            {
                var repos = await _gitHubService.GetRepositoriesAsync();
            
                // UI 스레드에서 컬렉션 업데이트
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Repositories = new ObservableCollection<Repository>(repos);
                    OnPropertyChanged(nameof(Repositories));
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading repositories: {ex}");
                MessageBox.Show("Failed to load repositories", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async Task LoadWorkflows(Repository repository)
        {
            if (repository == null)
            {
                Workflows.Clear();
                Branches.Clear();
                return;
            }
            
            try
            {
                await ExecuteWithLoadingAsync(async () =>
                {
                    var branches = await _gitHubService.GetBranchesAsync(repository.Owner, repository.Name);
                    Branches = new ObservableCollection<string>(branches);
                    SelectedBranch = branches.FirstOrDefault();
                
                    var workflows = await _gitHubService.GetWorkflowRunsAsync(repository.Owner, repository.Name);
                    Workflows.Clear();
                    foreach (var workflow in workflows)
                    {
                        var status = await _gitHubService.GetWorkflowRunStatusAsync(repository.Owner, repository.Name, workflow.Id);
                        workflow.Status = $"{status.Status} ({status.Conclusion ?? "pending"})";
                        workflow.Conclusion = status.Conclusion;
                        
                        Workflows.Add(workflow);
                    }
                }, "Loading workflow information...");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading repository data: {ex}");
                MessageBox.Show("Failed to load repository data", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteWorkflow(WorkflowRun workflow)
        {
            if (workflow == null || SelectedRepository == null || string.IsNullOrEmpty(SelectedBranch))
            {
                MessageBox.Show("Please select a branch first", "Warning",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var result = await _gitHubService.TriggerWorkflowAsync(
                                                                      SelectedRepository.Owner,
                                                                      SelectedRepository.Name,
                                                                      workflow.Id,
                                                                      SelectedBranch);

                if (result != null)
                {
                    workflow.RunId = result.RunId;
                    workflow.Status = "queue";
                    _activeRuns.TryAdd(result.RunId, workflow);
                    StartStatusPolling(result.RunId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start workflow: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void StartStatusPolling(string runId)
        {
            while (_activeRuns.TryGetValue(runId, out var workflow))
            {
                try
                {
                    var status = await _gitHubService.GetWorkflowRunStatusAsync(
                                                                                SelectedRepository.Owner,
                                                                                SelectedRepository.Name,
                                                                                runId);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (!string.IsNullOrEmpty(status.Conclusion))
                        {
                            _activeRuns.TryRemove(runId, out _);
                            workflow.Status = string.Empty;
                            ShowToast($"Workflow '{workflow.Name}' completed: {status.Conclusion}", 
                                      status.Conclusion.ToLower() == "success");
                        }
                        else
                        {
                            workflow.Status = status.Status;
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error polling workflow status: {ex}");
                }

                await Task.Delay(5000);
            }
        }
        
        
        
        private async void ShowToast(string message, bool isSuccess)
        {
            try
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    await NotificationService.ShowToast(isSuccess ? "Success" : "Error", message);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Toast Error: {ex.Message}");
                MessageBox.Show(message, 
                                isSuccess ? "Success" : "Error",
                                MessageBoxButton.OK, 
                                isSuccess ? MessageBoxImage.Information : MessageBoxImage.Warning
                               );
            }
        }

        private void UpdateWorkflowCount()
        {
            WorkflowCount = Workflows.Count;
        }

        public async Task OnNavigatedTo()
        {
            Log.Information("RepositoryListViewModel.OnNavigatedTo called");
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    try 
                    {
                        Log.Debug("Starting InitializeAsync");
                        await InitializeAsync();
                        Log.Information("InitializeAsync completed successfully");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error in InitializeAsync");
                        MessageBox.Show($"An error occurred while loading data: {ex.Message}", 
                                        "오류", 
                                        MessageBoxButton.OK, 
                                        MessageBoxImage.Error);
                        _navigationService.NavigateTo<GitHubLoginView>();
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in OnNavigatedTo");
                MessageBox.Show($"An error occurred during screen transition: {ex.Message}", 
                                "오류", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Error);
                _navigationService.NavigateTo<GitHubLoginView>();
            }
        }
    }
}

