using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Common.Utilities;

namespace Common.Authentication;

internal static class Authenticator
{
    private static TokenCredential? _credential;
    private static TokenCredential? _graphCredential;
    private static SecretClient? _secretClient;

    internal static TokenCredential GetCredential()
    {
        if (_credential is not null)
        {
            return _credential;
        }

        _credential = AppSettings.IsDevelopment
            ? new DefaultAzureCredential()
            : new ManagedIdentityCredential();

        return _credential;
    }

    internal static async Task<TokenCredential> GetGraphCredentialAsync()
    {
        if (_graphCredential is not null)
        {
            return _graphCredential;
        }

        string clientSecret = await GetSecretAsync(AppSettings.ApiClientSecretName);
        _graphCredential = new ClientSecretCredential(AppSettings.TenantId, AppSettings.ApiClientId, clientSecret);
        return _graphCredential;
    }

    internal static async Task<string> GetSecretAsync(string secretName)
    {
        _secretClient ??= new SecretClient(new Uri(AppSettings.KeyVaultUri), GetCredential());
        KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
        return secret.Value;
    }
}
