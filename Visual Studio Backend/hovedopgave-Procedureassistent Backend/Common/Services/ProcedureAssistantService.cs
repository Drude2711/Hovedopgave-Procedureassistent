using Common.Models;
using Common.Providers;
using Common.Utilities;
using Microsoft.Extensions.Logging;

namespace Common.Services;

internal class ProcedureAssistantService
{
    private const string NotFoundAnswer = "Det ved jeg ikke. Jeg kan ikke finde svaret i virksomhedens procedurer.";
    private const int MaxHistoryTurns = 6;

    private static ProcedureDocument? _document;

    private readonly ILogger _logger;
    private readonly OpenAIProvider _openAIProvider;
    private readonly GraphProvider _graphProvider;

    public ProcedureAssistantService(ILogger logger)
    {
        _logger = logger;
        _openAIProvider = new OpenAIProvider();
        _graphProvider = new GraphProvider();
    }

    internal async Task<AskResponse> AskAsync(string question, List<ChatTurn>? history)
    {
        _document ??= await _graphProvider.GetDocumentAsync(AppSettings.TestDocument);
        _logger.LogInformation("Bruger dokument {Name} ({Length} tegn)", _document.Name, _document.Text.Length);

        List<ChatTurn> recentHistory = (history ?? new List<ChatTurn>())
            .Where(t => !string.IsNullOrWhiteSpace(t.Text))
            .TakeLast(MaxHistoryTurns)
            .ToList();

        string systemPrompt =
            "Du er en procedureassistent for virksomheden. " +
            "Svar kun ud fra dokumentet nedenfor, og brug aldrig din egen generelle viden. " +
            $"Hvis svaret ikke står i dokumentet, så svar præcis: \"{NotFoundAnswer}\" " +
            "Svar kort og præcist, og svar altid på dansk.\n\n" +
            $"Dokument: {_document.Name}\n---\n{_document.Text}\n---";

        _logger.LogInformation("Sender spørgsmål til modellen med {Count} tidligere beskeder: {Question}",
            recentHistory.Count, question);

        string answer = await _openAIProvider.GetAnswerAsync(systemPrompt, recentHistory, question);

        _logger.LogInformation("Svar modtaget fra modellen.");

        bool notFound = answer.Trim().StartsWith("Det ved jeg ikke");

        return new AskResponse
        {
            Answer = answer,
            Sources = notFound
                ? new List<Source>()
                : new List<Source> { new() { Title = _document.Name, Url = _document.Url } }
        };
    }
}
