namespace Common.Utilities;


public static class AppSettings {
    public static string OpenAIEndpoint => Get("AZURE_OPENAI_ENDPOINT");
    public static string OpenAIDeployment => Get("AZURE_OPENAI_DEPLOYMENT");

    // Login (Entra ID) – app-registreringen "Procedureassistent"
    public static string TenantId => Get("AZURE_TENANT_ID");
    public static string ApiClientId => Get("API_CLIENT_ID");
    public static string ApiAppIdUri => Get("API_APP_ID_URI");

  
    public static bool AuthEnabled =>
        string.Equals(Environment.GetEnvironmentVariable("AUTH_ENABLED"), "true", StringComparison.OrdinalIgnoreCase);

    public static string AspNetCoreEnvironment =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;

    public static bool IsDevelopment => AspNetCoreEnvironment == "Development";

    private static string Get(string name) =>
        Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");
}