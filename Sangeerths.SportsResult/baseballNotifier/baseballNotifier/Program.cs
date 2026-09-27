using baseballNotifier;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Microsoft.Extensions.Logging;

Env.Load(); // reads .env from the working directory

AnsiConsole.Write(
    new FigletText("Baseball Notifier")
        .Centered()
        .Color(Color.Green));

AnsiConsole.Write(new Rule("[grey]Daily sports data scraper & emailer[/]").RuleStyle("grey").Centered());
AnsiConsole.MarkupLine("[bold]Status:[/] [green]starting up[/]");
AnsiConsole.WriteLine();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(static logging =>
    {
      
        logging.ClearProviders();
    })
    .ConfigureServices(static services =>
    {
        services.AddHttpClient();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();