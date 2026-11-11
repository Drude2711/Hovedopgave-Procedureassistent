using Common.Models;
using Common.Providers;
using Microsoft.Extensions.Logging;

namespace Common.Services;

internal class ProcedureAssistantService
{
    private const string NotFoundAnswer = "Det ved jeg ikke. Jeg kan ikke finde svaret i virksomhedens procedurer.";
    private const string SourcesPrefix = "Kilder:";
    private const int MaxHistoryTurns = 6;

    private static List<ProcedureDocument>? _documents;

    private readonly ILogger _logger;
    private readonly OpenAIProvider _openAIProvider;
    private readonly StorageProvider _storageProvider;

    public ProcedureAssistantService(ILogger logger)
    {
        _logger = logger;
        _openAIProvider = new OpenAIProvider();
        _storageProvider = new StorageProvider();
    }

    internal async Task<AskResponse> AskAsync(string question, List<ChatTurn>? history)
    {
        _documents ??= await _storageProvider.GetDocumentsAsync();
        _logger.LogInformation("Bruger {Count} dokumenter ({Length} tegn)",
            _documents.Count, _documents.Sum(d => d.Text.Length));

        List<ChatTurn> recentHistory = (history ?? new List<ChatTurn>())
            .Where(t => !string.IsNullOrWhiteSpace(t.Text))
            .TakeLast(MaxHistoryTurns)
            .ToList();

        string documentText = string.Join("\n\n", _documents.Select(d =>
            $"Dokument: {d.Name}\n---\n{d.Text}\n---"));

        string systemPrompt =
            "Du er en procedureassistent for virksomheden. " +
            "Svar kun ud fra dokumenterne nedenfor, og brug aldrig din egen generelle viden. " +
            $"Hvis svaret ikke står i dokumenterne, så svar præcis: \"{NotFoundAnswer}\" " +
            "Svar kort og præcist, og svar altid på dansk. " +
            "Hvis flere dokumenter siger noget forskelligt om det samme, så brug det nyeste ud fra version og dato i dokumentnavnet, " +
            "og nævn kort, at der findes en ældre version. " +
            $"Når du har fundet svaret, så slut med en linje, der starter med \"{SourcesPrefix}\" " +
            "efterfulgt af navnene på de dokumenter, du har brugt, adskilt med semikolon.\n\n" +
            documentText;

        _logger.LogInformation("Sender spørgsmål til modellen med {Count} tidligere beskeder: {Question}",
            recentHistory.Count, question);

        string reply = await _openAIProvider.GetAnswerAsync(systemPrompt, recentHistory, question);

        _logger.LogInformation("Svar modtaget fra modellen.");

        (string answer, List<string> usedNames) = SplitSources(reply);

        bool notFound = answer.StartsWith("Det ved jeg ikke");

        List<Source> sources = notFound
            ? new List<Source>()
            : _documents
                .Where(d => usedNames.Contains(d.Name, StringComparer.OrdinalIgnoreCase))
                .Select(d => new Source { Title = d.Name, Url = string.IsNullOrEmpty(d.Url) ? null : d.Url })
                .ToList();

        return new AskResponse { Answer = answer, Sources = sources };
    }

    private static (string Answer, List<string> Names) SplitSources(string reply)
    {
        List<string> lines = reply.Trim().Split('\n').ToList();
        int index = lines.FindLastIndex(l => l.TrimStart().StartsWith(SourcesPrefix, StringComparison.OrdinalIgnoreCase));

        if (index < 0)
        {
            return (reply.Trim(), new List<string>());
        }

        List<string> names = lines[index].Trim()[SourcesPrefix.Length..]
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        string answer = string.Join('\n', lines.Take(index)).Trim();
        return (answer, names);
    }
}
