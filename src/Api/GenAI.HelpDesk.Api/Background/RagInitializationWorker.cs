using GenAI.HelpDesk.Api.Services.Interfaces;

namespace GenAI.HelpDesk.Api.Background;

public class RagInitializationWorker : BackgroundService
{
    private readonly IRagService _ragService;
    private readonly ILogger<RagInitializationWorker> _logger;

    public RagInitializationWorker(IRagService ragService, ILogger<RagInitializationWorker> logger)
    {
        _ragService = ragService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting RAG initialization worker...");
        await _ragService.InitializeAsync();
        _logger.LogInformation("RAG initialization worker completed successfully.");
    }
}
