namespace GitActionRunner.Core.Interfaces;

public interface IAuthenticationService
{
    Task<string> GetAccessTokenAsync();
    Task SaveAccessTokenAsync(string token);
    bool IsAuthenticated { get; }
}