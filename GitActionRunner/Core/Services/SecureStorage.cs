using System.IO;
using System.Text.Json;
using GitActionRunner.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;
using Serilog;

namespace GitActionRunner.Core.Models;
public class SecureStorage : ISecureStorage
{
    private readonly string _filePath;

    public SecureStorage()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var directory = Path.Combine(appData, "GitActionRunner");
        Directory.CreateDirectory(directory);
        _filePath = Path.Combine(directory, "secure_storage.dat");
        Log.Debug("SecureStorage initialized with path: {FilePath}", _filePath);
    }

    public async Task<string> GetAsync(string key)
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                Log.Debug("Storage file not found at: {FilePath}", _filePath);
                return null;
            }
            
            Log.Debug("Reading secure storage for key: {Key}", key);
            
            var encryptedData = await File.ReadAllBytesAsync(_filePath);
            var decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(decryptedData);
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            var exists = dictionary.TryGetValue(key, out var value);
            Log.Debug("Value {Exists} for key: {Key}", exists ? "found" : "not found", key);
            return value;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to read from secure storage for key: {Key}", key);
            return null;
        }
    }

    public async Task SaveAsync(string key, string value)
    {
        try
        {
            Log.Debug("Saving value for key: {Key}", key);
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
                Log.Debug("Creating new secure storage file");
                dictionary = new Dictionary<string, string>();
            }

            dictionary[key] = value;
            var updatedJson = JsonSerializer.Serialize(dictionary);
            var dataToEncrypt = Encoding.UTF8.GetBytes(updatedJson);
            var encryptedBytes = ProtectedData.Protect(dataToEncrypt, null, DataProtectionScope.CurrentUser);

            await File.WriteAllBytesAsync(_filePath, encryptedBytes);
            Log.Information("Successfully saved value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save to secure storage for key: {Key}", key);
            throw;
        }
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