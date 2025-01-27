using GitActionRunner.Core.Interfaces;

namespace GitActionRunner.Core.Models;

// Services/AuthenticationService.cs
public class AuthenticationService : IAuthenticationService
{
    private string _token;
    private readonly ISecureStorage _secureStorage;

    public AuthenticationService(ISecureStorage secureStorage)
    {
        _secureStorage = secureStorage;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public async Task<string> GetAccessTokenAsync()
    {
        if (string.IsNullOrEmpty(_token))
        {
            _token = await _secureStorage.GetAsync("github_token");
        }
        return _token;
    }

    public async Task SaveAccessTokenAsync(string token)
    {
        _token = token;
        await _secureStorage.SaveAsync("github_token", token);
    }
}