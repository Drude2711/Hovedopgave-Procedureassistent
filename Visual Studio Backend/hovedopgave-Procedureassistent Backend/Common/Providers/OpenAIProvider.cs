using Azure.AI.OpenAI;
using Common.Authentication;
using Common.Models;
using Common.Utilities;
using OpenAI.Chat;

namespace Common.Providers;


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
