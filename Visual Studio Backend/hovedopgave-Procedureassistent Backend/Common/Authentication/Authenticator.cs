using Azure.Core;
using Azure.Identity;
using Common.Utilities;

namespace Common.Authentication;

/// <summary>
/// Giver den credential, der bruges mod Azure-tjenester.
/// Lokalt (Development): din egen login fra Visual Studio eller az login.
/// I Azure: Function Appens managed identity. Der bruges ingen API-nøgler.
/// </summary>
internal static class Authenticator
{
    private static TokenCredential? _credential;

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
}
