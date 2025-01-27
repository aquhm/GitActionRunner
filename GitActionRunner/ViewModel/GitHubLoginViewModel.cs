using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitActionRunner.Core.Interfaces;
using GitActionRunner.Views;

namespace GitActionRunner.ViewModels
{
    public class GitHubLoginViewModel : ObservableObject
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
        
        public bool IsNotConnected => !IsConnected;
        
        public INavigationService NavigationService
        {
            get => _navigationService;
            set
            {
                _navigationService = value;
                // NavigationService가 설정된 후 초기화 작업이 필요한 경우 여기서 수행
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
            // 저장된 토큰이 있는지 확인
            var savedToken = await _authService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(savedToken))
            {
                var isAuthenticated = await _gitHubService.AuthenticateAsync(savedToken);
                if (isAuthenticated)
                {
                    IsConnected = true;
                    await LoadUserInfo();
                    UpdateConnectionStatus();
                    NavigationService.NavigateTo<RepositoryListView>();
                }
            }
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

                var isAuthenticated = await _gitHubService.AuthenticateAsync(AccessToken);
                if (isAuthenticated)
                {
                    await _authService.SaveAccessTokenAsync(AccessToken);
                    IsConnected = true;
                    await LoadUserInfo();
                    UpdateConnectionStatus();
                    NavigationService.NavigateTo<RepositoryListView>();
                }
                else
                {
                    ConnectionStatus = "인증 실패: 유효하지 않은 토큰";
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus = "연결 실패";
            }
        }

        private async Task Logout()
        {
            await _authService.SaveAccessTokenAsync(null);
            IsConnected = false;
            UserName = null;
            Email = null;
            UpdateConnectionStatus();
        }

        private async Task LoadUserInfo()
        {
            if (IsConnected)
            {
                // GitHub API로 사용자 정보 로드
                ConnectionStatus = "연결됨";
            }
        }

        private void UpdateConnectionStatus()
        {
            ConnectionStatus = IsConnected ? "연결됨" : "연결되지 않음";
        }
    }
    
    
    
}
