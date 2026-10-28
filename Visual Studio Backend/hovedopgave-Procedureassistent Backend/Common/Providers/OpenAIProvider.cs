using Azure.AI.OpenAI;
using Common.Authentication;
using Common.Utilities;
using OpenAI.Chat;

namespace Common.Providers;

/// <summary>
/// Al kommunikation med sprogmodellen i Azure AI Foundry.
/// </summary>
internal class OpenAIProvider
{
    // Oprettes én gang og genbruges på tværs af kald
    private static ChatClient? _chatClient;

    private static ChatClient GetChatClient()
    {
        if (_chatClient is not null)
        {
            return _chatClient;
        }

        var client = new AzureOpenAIClient(new Uri(AppSettings.OpenAIEndpoint), Authenticator.GetCredential());
        _chatClient = client.GetChatClient(AppSettings.OpenAIDeployment);
        return _chatClient;
    }

    internal async Task<string> GetAnswerAsync(string systemPrompt, string userMessage)
    {
        ChatCompletion completion = await GetChatClient().CompleteChatAsync(
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userMessage));

        return completion.Content[0].Text;
    }
}
