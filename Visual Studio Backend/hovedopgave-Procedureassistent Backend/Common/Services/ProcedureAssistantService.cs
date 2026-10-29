using Common.Models;
using Common.Providers;
using Microsoft.Extensions.Logging;

namespace Common.Services;

/// <summary>
/// Forretningslogikken: tager et spørgsmål og returnerer et svar med kilder.
/// Senere: søg i Azure AI Search, byg prompt med fundne afsnit, tjek adgang.
/// </summary>
internal class ProcedureAssistantService
{
    private const string SystemPrompt =
        "Du er en procedureassistent for virksomheden. " +
        "Svar kort og præcist, og svar altid på dansk.";

    // Kun de seneste beskeder sendes med, så tokenforbrug og svartid ikke vokser uendeligt
    private const int MaxHistoryTurns = 6;

    private readonly ILogger _logger;
    private readonly OpenAIProvider _openAIProvider;

    public ProcedureAssistantService(ILogger logger)
    {
        _logger = logger;
        _openAIProvider = new OpenAIProvider();
    }

    internal async Task<AskResponse> AskAsync(string question, List<ChatTurn>? history)
    {
        List<ChatTurn> recentHistory = (history ?? new List<ChatTurn>())
            .Where(t => !string.IsNullOrWhiteSpace(t.Text))
            .TakeLast(MaxHistoryTurns)
            .ToList();

        _logger.LogInformation("Sender spørgsmål til modellen med {Count} tidligere beskeder: {Question}",
            recentHistory.Count, question);

        string answer = await _openAIProvider.GetAnswerAsync(SystemPrompt, recentHistory, question);

        _logger.LogInformation("Svar modtaget fra modellen.");

        return new AskResponse
        {
            Answer = answer,
            Sources = new List<Source>() // kilder kommer, når dokumenterne er med (trin 6)
        };
    }
}
