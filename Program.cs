using HemnetScraperService.Helpers;
using HemnetScraperService.HemnetScraperModel;
using HemnetScraperService.Scraper;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Timeout;
using Serilog;
using System.Net;
using System.Net.Sockets;


namespace HemnetScraperService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages();

            builder.Services.AddHostedService<HemnetScraperBackgroundService>();
            builder.Services.AddWindowsService();

            builder.Logging.ClearProviders();
            var logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(builder.Configuration)
                            .CreateLogger();

            builder.Logging.AddSerilog(logger);

            builder.Services
                .AddHttpClient("DesktopHemnetClient", httpClient =>
                {
                    foreach (var header in HemnetScraperConstants.DESKTOP_HEADERS)
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler
                    {
                        UseCookies = false,
                        AllowAutoRedirect = false,
                        AutomaticDecompression = DecompressionMethods.All,
                    };
                })
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(30));

            builder.Services
                .AddHttpClient("AndroidHemnetClient", httpClient =>
                {
                    foreach (var header in HemnetScraperConstants.ANDROID_HEADERS)
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                 {
                     return new HttpClientHandler
                     {
                         UseCookies = false,
                         AllowAutoRedirect = false,
                         AutomaticDecompression = DecompressionMethods.GZip
                     };
                 })
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(30));

            builder.Services.AddDbContextFactory<HemnetDbContext>(options =>
            {
                string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
                options.UseSqlite(Environment.ExpandEnvironmentVariables(connectionString));
            });

            var app = builder.Build();

            //Load Historical Data
            using (var scope = app.Services.CreateScope())
            {
                using var dbContext = scope.ServiceProvider.GetRequiredService<HemnetDbContext>();
                HistoricalDataLoader.Load(dbContext);
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapRazorPages();
            app.UseDeveloperExceptionPage();
            app.Run();
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromMilliseconds(500), retryCount: 3);
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(p => p.Content.ReadAsStringAsync()
                                        .GetAwaiter().GetResult()
                                        .Contains("\"data\":null"))
                .Or<TimeoutRejectedException>()
                .Or<SocketException>()
                .WaitAndRetryAsync(delay);
        }
    }
}