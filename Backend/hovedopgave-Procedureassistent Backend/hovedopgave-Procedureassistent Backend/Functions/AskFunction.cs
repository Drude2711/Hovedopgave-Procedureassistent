using System.Security.Claims;
using Common;
using Common.Authentication;
using Common.Models;
using Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace hovedopgave_Procedureassistent_Backend.Functions;

public class AskFunction {
    private readonly ILogger<AskFunction> _logger;

    public AskFunction(ILogger<AskFunction> logger)
    {
        _logger = logger;
    }

    [Function("ask")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ask")] HttpRequest req)
    {
        string userName = "anonym";
        if (AppSettings.AuthEnabled)
        {
            ClaimsIdentity? identity = await TokenValidator.ValidateAsync(req.Headers.Authorization.ToString());
            if (identity is null)
            {
                _logger.LogWarning("Afvist: manglende eller ugyldigt token");
                return new UnauthorizedResult();
            }
            userName = identity.FindFirst("name")?.Value ?? "ukendt";
        }

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

        _logger.LogInformation("Spørgsmål fra {User}", userName);

        try
        {
            var common = new CommonServices(_logger);
            AskResponse response = await common.AskAsync(request.Question, request.History);
            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved behandling af spørgsmålet");
            return new ObjectResult("Kunne ikke hente svar.") { StatusCode = 502 };
        }
    }
}