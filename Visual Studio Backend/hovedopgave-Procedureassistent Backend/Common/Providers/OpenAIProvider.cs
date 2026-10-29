using Azure.AI.OpenAI;
using Common.Authentication;
using Common.Models;
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

    /// <summary>
    /// Sender system-prompt, tidligere beskeder og det nye spørgsmål til modellen.
    /// </summary>
    internal async Task<string> GetAnswerAsync(string systemPrompt, IEnumerable<ChatTurn> history, string question)
    {
        var messages = new List<ChatMessage> { new SystemChatMessage(systemPrompt) };

        foreach (ChatTurn turn in history)
        {
            if (turn.Role == "assistant")
            {
                messages.Add(new AssistantChatMessage(turn.Text));
            }
            else
            {
                messages.Add(new UserChatMessage(turn.Text));
            }
        }

        messages.Add(new UserChatMessage(question));

        ChatCompletion completion = await GetChatClient().CompleteChatAsync(messages);
        return completion.Content[0].Text;
    }
}
