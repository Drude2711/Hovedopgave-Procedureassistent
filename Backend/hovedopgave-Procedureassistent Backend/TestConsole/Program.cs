using Common;
using Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestConsole;

internal class Program
{
    static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddLogging(logging => logging.AddConsole());

        using IHost host = builder.Build();
        ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();

        var common = new CommonServices(logger);

        // Samtalen gemmes her i klienten og sendes med hver gang
        var history = new List<ChatTurn>();

        Console.WriteLine("Procedureassistent – testkonsol. Skriv et spørgsmål (tom linje afslutter, 'ny' starter forfra).");

        while (true)
        {
            Console.Write("\nSpørgsmål: ");
            string? question = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(question))
            {
                break;
            }
            if (question.Trim().Equals("ny", StringComparison.OrdinalIgnoreCase))
            {
                history.Clear();
                Console.WriteLine("Samtalen er nulstillet.");
                continue;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            AskResponse response = await common.AskAsync(question, history);
            stopwatch.Stop();

            history.Add(new ChatTurn { Role = "user", Text = question });
            history.Add(new ChatTurn { Role = "assistant", Text = response.Answer });

            Console.WriteLine($"\nSvar: {response.Answer}");
            Console.WriteLine($"Kilder: {(response.Sources.Count == 0 ? "ingen" : string.Join(", ", response.Sources.Select(s => s.Title)))}");
            Console.WriteLine($"Svartid: {stopwatch.Elapsed.TotalSeconds:0.0} sek.");
        }
    }
}
