using HemnetScraperService.HemnetScraperModel;
using Microsoft.EntityFrameworkCore;


namespace HemnetScraperService.Scraper
{
    public class HemnetScraperBackgroundService : BackgroundService
    {
        private readonly ILogger<HemnetScraperBackgroundService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDbContextFactory<HemnetDbContext> _dbContextFactory;
        private readonly IConfiguration _configuration;

        public HemnetScraperBackgroundService(ILogger<HemnetScraperBackgroundService> logger, IHttpClientFactory httpClientFactory, IDbContextFactory<HemnetDbContext> dbContextFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("The scraper service is started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var nextUpdateTimeSpan = await Task.Run<TimeSpan>(async () =>
                    {
                        HemnetScraper hemnetScraper = new(_logger, _httpClientFactory, _dbContextFactory, _configuration);
                        return await hemnetScraper.Scrape();
                    });

                    _logger.LogInformation("Sleeping for {Seconds} seconds.", nextUpdateTimeSpan.TotalSeconds);

                    PeriodicTimer periodicTimer = new(nextUpdateTimeSpan);
                    await periodicTimer.WaitForNextTickAsync(stoppingToken);
                    periodicTimer.Dispose();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, message: string.Empty);
                }
            }

            _logger.LogInformation("The scraper service is closed.");
        }
    }
}
