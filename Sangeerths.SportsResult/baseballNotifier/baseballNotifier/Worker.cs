using HtmlAgilityPack;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace baseballNotifier
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly HttpClient _httpClient;

        // Loaded from .env via DotNetEnv (see Program.cs)
        private readonly string emailFromAddress = Environment.GetEnvironmentVariable("EMAIL_FROM_ADDRESS") ?? "";
        private readonly string emailAppPassword = Environment.GetEnvironmentVariable("EMAIL_APP_PASSWORD") ?? "";
        private readonly string emailToAddress = Environment.GetEnvironmentVariable("EMAIL_TO_ADDRESS") ?? "";

        // SMTP settings — adjust to your provider (these are Gmail defaults)
        private readonly string smtpAddress = "smtp.gmail.com";
        private readonly int portNumber = 587;
        private readonly bool enableSSL = true;

        public Worker(ILogger<Worker> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // PeriodicTimer sets the execution frequency (24 hours)
            using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromDays(1));

            // Run immediately on service start, then every 24 hours
            do
            {
                try
                {
                    AnsiConsole.Write(new Rule($"[grey]{DateTime.Now:yyyy-MM-dd HH:mm:ss}[/]").LeftJustified());
                    _logger.LogInformation("Starting daily sports data scrape...");
                    await ScrapeAndSendEmailAsync();
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine("[bold red]Error:[/] an exception occurred during execution.");
                    AnsiConsole.WriteException(ex);
                    _logger.LogError(ex, "An error occurred during execution.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task ScrapeAndSendEmailAsync()
        {
            // Sandbox scraping endpoint for testing compliance
            string url = "https://www.scrapethissite.com/pages/forms/";

            string html = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("[yellow]Fetching page...[/]", async ctx =>
                {
                    return await _httpClient.GetStringAsync(url);
                });

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNodeCollection? rows = doc.DocumentNode.SelectNodes("//table/tr[@class='team']");

            if (rows == null || rows.Count == 0)
            {
                AnsiConsole.MarkupLine("[bold red]No data found[/] — the page structure may have changed.");
                _logger.LogWarning("No rows found on the page.");
                return;
            }

            // Build the console table
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("[bold]Year[/]");
            table.AddColumn("[bold]Team Name[/]");
            table.AddColumn("[bold]Wins[/]");

            // Build the HTML email body in parallel
            StringBuilder bodyBuilder = new StringBuilder();
            bodyBuilder.Append("<h2>Daily NHL Team Stats Update</h2>");
            bodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0'>");
            bodyBuilder.Append("<tr><th>Year</th><th>Team Name</th><th>Wins</th></tr>");

            foreach (var row in rows)
            {
                string teamName = row.SelectSingleNode("./td[@class='name']")?.InnerText.Trim() ?? "N/A";
                string year = row.SelectSingleNode("./td[@class='year']")?.InnerText.Trim() ?? "N/A";
                string wins = row.SelectSingleNode("./td[@class='wins']")?.InnerText.Trim() ?? "N/A";

                table.AddRow(year, teamName, wins);
                bodyBuilder.Append($"<tr><td>{year}</td><td>{teamName}</td><td>{wins}</td></tr>");
            }

            bodyBuilder.Append("</table>");

            AnsiConsole.MarkupLine($"[green]Scraped {rows.Count} rows successfully.[/]");
            AnsiConsole.Write(table);

            AnsiConsole.Status()
                .Spinner(Spinner.Known.Star)
                .Start("[yellow]Sending email...[/]", ctx =>
                {
                    SendEmail(bodyBuilder.ToString());
                });
        }

        private void SendEmail(string bodyHtml)
        {
            using MailMessage mail = new MailMessage();
            mail.From = new MailAddress(emailFromAddress);
            mail.To.Add(emailToAddress);
            mail.Subject = "Daily NHL Team Stats Report";
            mail.Body = bodyHtml;
            mail.IsBodyHtml = true;

            using SmtpClient smtp = new SmtpClient(smtpAddress, portNumber);
            smtp.Credentials = new NetworkCredential(emailFromAddress, emailAppPassword);
            smtp.EnableSsl = enableSSL;
            smtp.Send(mail);

            AnsiConsole.MarkupLine("[bold green]Email sent successfully.[/]");
            _logger.LogInformation("Email sent successfully.");
        }
    }
}