using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitActionRunner.Core.Interfaces;
using GitActionRunner.Views;

namespace GitActionRunner.ViewModels
{
    public class GitHubLoginViewModel : BaseViewModel
    {
        private readonly IGitHubService _gitHubService;
        private readonly IAuthenticationService _authService;
        private INavigationService _navigationService;
        
        private bool _isConnected;
        private string _userName;
        private string _email;
        private string _connectionStatus;
        private string _accessToken;
        
        public GitHubLoginViewModel(IGitHubService gitHubService, IAuthenticationService authService, INavigationService navigationService)
        {
            _gitHubService = gitHubService;
            _authService = authService;
            _navigationService = navigationService;

            LoginWithTokenCommand = new AsyncRelayCommand(LoginWithToken);
            LogoutCommand = new AsyncRelayCommand(Logout);

            Task.Run(InitializeAsync);
        }

        public bool IsConnected 
        { 
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }
        
        public INavigationService NavigationService
        {
            get => _navigationService;
            set
            {
                _navigationService = value;
            }
        }
        
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }
        
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }
        
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }
        
        public string AccessToken
        {
            get => _accessToken;
            set => SetProperty(ref _accessToken, value);
        }

        public ICommand LoginWithTokenCommand { get; }
        public ICommand LogoutCommand { get; }
        
        private async Task InitializeAsync()
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                var savedToken = await _authService.GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(savedToken))
                {
                    var isAuthenticated = await _gitHubService.AuthenticateAsync(savedToken);
                    if (isAuthenticated)
                    {
                        IsConnected = true;
                        NavigationService.NavigateTo<RepositoryListView>();
                    }
                }
            }, "계정 연동 상태를 확인중입니다...");
        }


        private async Task LoginWithToken()
        {
            try
            {
                if (string.IsNullOrEmpty(AccessToken))
                {
                    ConnectionStatus = "토큰을 입력해주세요";
                    return;
                }

                await ExecuteWithLoadingAsync(async () =>
                {
                    var isAuthenticated = await _gitHubService.AuthenticateAsync(AccessToken);
                    if (isAuthenticated)
                    {
                        await _authService.SaveAccessTokenAsync(AccessToken);
                    
                        IsConnected = true;
                        AccessToken = string.Empty;
                        NavigationService.NavigateTo<RepositoryListView>();
                    }
                    else
                    {
                        ConnectionStatus = "인증 실패: 유효하지 않은 토큰";
                    }
                }, "인증을 진행중입니다...");
            }
            catch (Exception ex)
            {
                ConnectionStatus = "연결 실패";
            }
        }

        private async Task Logout()
        {
            await _authService.SaveAccessTokenAsync(string.Empty);
            IsConnected = false;
            UserName = string.Empty;
            Email = string.Empty;
            AccessToken = string.Empty;
        }
    }
}
