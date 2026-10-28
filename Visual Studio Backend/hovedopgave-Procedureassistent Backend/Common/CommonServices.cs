using Common.Models;
using Common.Services;
using Microsoft.Extensions.Logging;

namespace Common;

/// <summary>
/// Indgangen til Common. Bruges af både Function-projektet og TestConsole,
/// så de kalder præcis den samme logik.
/// </summary>
public class CommonServices
{
    private readonly ILogger _logger;
    private readonly ProcedureAssistantService _procedureAssistantService;

    public CommonServices(ILogger logger)
    {
        _logger = logger;
        _procedureAssistantService = new ProcedureAssistantService(_logger);
    }

    public async Task<AskResponse> AskAsync(string question)
    {
        return await _procedureAssistantService.AskAsync(question);
    }
}
