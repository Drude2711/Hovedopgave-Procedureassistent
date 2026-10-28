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

        Console.WriteLine("Procedureassistent – testkonsol. Skriv et spørgsmål (tom linje afslutter).");

        while (true)
        {
            Console.Write("\nSpørgsmål: ");
            string? question = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(question))
            {
                break;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            AskResponse response = await common.AskAsync(question);
            stopwatch.Stop();

            Console.WriteLine($"\nSvar: {response.Answer}");
            Console.WriteLine($"Kilder: {(response.Sources.Count == 0 ? "ingen" : string.Join(", ", response.Sources.Select(s => s.Title)))}");
            Console.WriteLine($"Svartid: {stopwatch.Elapsed.TotalSeconds:0.0} sek.");
        }
    }
}
