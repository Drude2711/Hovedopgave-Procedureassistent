using Common.Models;
using Common.Providers;
using Microsoft.Extensions.Logging;

namespace Common.Services;

internal class ProcedureAssistantService
{
    private const string NotFoundAnswer = "Det ved jeg ikke. Jeg kan ikke finde svaret i virksomhedens procedurer.";
    private const string SourcesPrefix = "Kilder:";
    private const int MaxHistoryTurns = 6;

    private readonly ILogger _logger;
    private readonly OpenAIProvider _openAIProvider;
    private readonly SearchProvider _searchProvider;

    public ProcedureAssistantService(ILogger logger)
    {
        _logger = logger;
        _openAIProvider = new OpenAIProvider();
        _searchProvider = new SearchProvider();
    }

    internal async Task<AskResponse> AskAsync(string question, List<ChatTurn>? history)
    {
        List<ChatTurn> recentHistory = (history ?? new List<ChatTurn>())
            .Where(t => !string.IsNullOrWhiteSpace(t.Text))
            .TakeLast(MaxHistoryTurns)
            .ToList();

        string? previousQuestion = recentHistory.LastOrDefault(t => t.Role == "user")?.Text;
        string searchQuery = previousQuestion is null ? question : $"{previousQuestion} {question}";

        List<ProcedureDocument> chunks = await _searchProvider.SearchAsync(searchQuery);
        _logger.LogInformation("Søgningen fandt {Count} tekststykker fra: {Names}",
            chunks.Count, string.Join(", ", chunks.Select(c => c.Name).Distinct()));

        if (chunks.Count == 0)
        {
            return new AskResponse { Answer = NotFoundAnswer, Sources = new List<Source>() };
        }

        string documentText = string.Join("\n\n", chunks.Select(c =>
            $"Dokument: {c.Name}\n---\n{c.Text}\n---"));

        string systemPrompt =
            "Du er en procedureassistent for virksomheden. " +
            "Svar kun ud fra uddragene fra dokumenterne nedenfor, og brug aldrig din egen generelle viden. " +
            $"Hvis svaret ikke står i uddragene, så svar præcis: \"{NotFoundAnswer}\" " +
            "Svar kort og præcist, og svar altid på dansk. " +
            "Hvis flere dokumenter siger noget forskelligt om det samme, så brug det nyeste ud fra version og dato i dokumentnavnet, " +
            "og nævn kort, at der findes en ældre version. " +
            $"Når du har fundet svaret, så slut med en linje, der starter med \"{SourcesPrefix}\" " +
            "efterfulgt af navnene på de dokumenter, dit svar bygger på, adskilt med semikolon. " +
            "Skriv aldrig et dokument i kildelinjen, som du kun nævner som forældet.\n\n" +
            documentText;

        _logger.LogInformation("Sender spørgsmål til modellen med {Count} tidligere beskeder: {Question}",
            recentHistory.Count, question);

        string reply = await _openAIProvider.GetAnswerAsync(systemPrompt, recentHistory, question);

        _logger.LogInformation("Svar modtaget fra modellen.");

        (string answer, List<string> usedNames) = SplitSources(reply);

        bool notFound = answer.StartsWith("Det ved jeg ikke");

        List<Source> sources = notFound
            ? new List<Source>()
            : chunks
                .Select(c => c.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(name => usedNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                .Select(name => new Source { Title = name })
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
