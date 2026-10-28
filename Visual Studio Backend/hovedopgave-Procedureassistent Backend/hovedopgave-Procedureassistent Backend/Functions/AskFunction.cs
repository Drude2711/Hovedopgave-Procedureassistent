using hovedopgave_Procedureassistent_Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace hovedopgave_Procedureassistent_Backend.Functions;

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

        _logger.LogInformation("Spørgsmål modtaget: {Question}", request.Question);

        // Trin 2 i walking skeleton: hårdkodet svar.
        // Trin 3 erstatter dette med et kald til GPT-5 i Azure AI Foundry.
        var response = new AskResponse
        {
            Answer = $"Hej fra backend! Du spurgte: \"{request.Question}\"",
            Sources = new List<Source> { new() { Title = "Backend-test" } }
        };

        return new OkObjectResult(response);
    }
}
