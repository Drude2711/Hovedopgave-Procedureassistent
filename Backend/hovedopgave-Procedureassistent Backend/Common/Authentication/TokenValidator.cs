using System.Security.Claims;
using Common.Utilities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Common.Authentication;


public static class TokenValidator {
    private static ConfigurationManager<OpenIdConnectConfiguration>? _configManager;

    public static async Task<ClaimsIdentity?> ValidateAsync(string? authorizationHeader)
    {
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            return null;
        }

        string token = authorizationHeader["Bearer ".Length..];
        string tenantId = AppSettings.TenantId;

        // Henter Microsofts offentlige signeringsnøgler (caches automatisk)
        _configManager ??= new ConfigurationManager<OpenIdConnectConfiguration>(
            $"https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever());

        OpenIdConnectConfiguration config = await _configManager.GetConfigurationAsync(CancellationToken.None);

        var parameters = new TokenValidationParameters
        {
            
            ValidIssuers = new[]
            {
                $"https://login.microsoftonline.com/{tenantId}/v2.0",
                $"https://sts.windows.net/{tenantId}/"
            },
            // ... og være udstedt til dette API
            ValidAudiences = new[] { AppSettings.ApiClientId, AppSettings.ApiAppIdUri },
            IssuerSigningKeys = config.SigningKeys,
            ValidateLifetime = true
        };

        TokenValidationResult result = await new JsonWebTokenHandler().ValidateTokenAsync(token, parameters);
        return result.IsValid ? result.ClaimsIdentity : null;
    }
}