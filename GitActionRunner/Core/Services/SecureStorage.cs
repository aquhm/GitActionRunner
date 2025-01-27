using System.IO;
using System.Text.Json;
using GitActionRunner.Core.Interfaces;

namespace GitActionRunner.Core.Models;

// Services/SecureStorage.cs
using System.Security.Cryptography;
using System.Text;

public class SecureStorage : ISecureStorage
{
    private readonly string _filePath;

    public SecureStorage()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var directory = Path.Combine(appData, "GitActionRunner");
        Directory.CreateDirectory(directory);
        _filePath = Path.Combine(directory, "secure_storage.dat");
    }

    public async Task<string> GetAsync(string key)
    {
        try
        {
            if (!File.Exists(_filePath))
                return null;

            var encryptedData = await File.ReadAllBytesAsync(_filePath);
            var decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(decryptedData);
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            return dictionary.TryGetValue(key, out var value) ? value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveAsync(string key, string value)
    {
        Dictionary<string, string> dictionary;

        if (File.Exists(_filePath))
        {
            var encryptedData = await File.ReadAllBytesAsync(_filePath);
            var decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(decryptedData);
            dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        else
        {
            dictionary = new Dictionary<string, string>();
        }

        dictionary[key] = value;

        var updatedJson = JsonSerializer.Serialize(dictionary);
        var dataToEncrypt = Encoding.UTF8.GetBytes(updatedJson);
        var encryptedBytes = ProtectedData.Protect(dataToEncrypt, null, DataProtectionScope.CurrentUser);

        await File.WriteAllBytesAsync(_filePath, encryptedBytes);
    }

    public async Task RemoveAsync(string key)
    {
        if (!File.Exists(_filePath))
            return;

        var encryptedData = await File.ReadAllBytesAsync(_filePath);
        var decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
        var json = Encoding.UTF8.GetString(decryptedData);
        var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        if (dictionary.Remove(key))
        {
            var updatedJson = JsonSerializer.Serialize(dictionary);
            var dataToEncrypt = Encoding.UTF8.GetBytes(updatedJson);
            var encryptedBytes = ProtectedData.Protect(dataToEncrypt, null, DataProtectionScope.CurrentUser);
            await File.WriteAllBytesAsync(_filePath, encryptedBytes);
        }
    }
}