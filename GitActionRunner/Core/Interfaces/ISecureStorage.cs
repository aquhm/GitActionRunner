namespace GitActionRunner.Core.Interfaces;

// Interfaces/ISecureStorage.cs
public interface ISecureStorage
{
    Task<string> GetAsync(string key);
    Task SaveAsync(string key, string value);
    Task RemoveAsync(string key);
}