namespace Common.Utilities;

/// <summary>
/// Samler alle indstillinger ét sted. Læses fra local.settings.json (Function),
/// launchSettings.json (TestConsole) eller Application settings i Azure.
/// </summary>
public static class AppSettings
{
    public static string OpenAIEndpoint => Get("AZURE_OPENAI_ENDPOINT");
    public static string OpenAIDeployment => Get("AZURE_OPENAI_DEPLOYMENT");

    public static string AspNetCoreEnvironment =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;

    public static bool IsDevelopment => AspNetCoreEnvironment == "Development";

    private static string Get(string name) =>
        Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");
}
