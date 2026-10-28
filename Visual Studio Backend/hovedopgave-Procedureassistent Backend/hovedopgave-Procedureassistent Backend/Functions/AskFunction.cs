using Common;
using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace hovedopgave_Procedureassistent_Backend.Functions;

/// <summary>
/// Tynd HTTP-funktion: læser spørgsmålet og sender det videre til Common.
/// Al logik ligger i Common, så den også kan køres fra TestConsole.
/// </summary>
public class AskFunction
{
    private readonly ILogger<AskFunction> _logger;

    public AskFunction(ILogger<AskFunction> logger)
    {
        _logger = logger;
    }

    // POST /api/ask  –  body: { "question": "..." }
    [Function("ask")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ask")] HttpRequest req)
    {
        AskRequest? request;
        try
        {
            request = await req.ReadFromJsonAsync<AskRequest>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ugyldig JSON i forespørgslen");
            return new BadRequestObjectResult("Body skal være JSON: { \"question\": \"...\" }");
        }

        if (request is null || string.IsNullOrWhiteSpace(request.Question))
        {
            return new BadRequestObjectResult("Feltet 'question' mangler.");
        }

        try
        {
            var common = new CommonServices(_logger);
            AskResponse response = await common.AskAsync(request.Question);
            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved behandling af spørgsmålet");
            return new ObjectResult("Kunne ikke hente svar.") { StatusCode = 502 };
        }
    }
}
