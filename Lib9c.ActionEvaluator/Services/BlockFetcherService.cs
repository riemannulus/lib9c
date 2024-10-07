namespace Lib9c.ActionEvaluator.Services;

public class BlockFetcherService : BackgroundService
{
    public BlockQueueService BlockQueueService { get; }

    public BlockFetcherService(BlockQueueService blockQueueService)
    {
        BlockQueueService = blockQueueService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        FetchBlockAsync(stoppingToken);

    private Task FetchBlockAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            // Fetch block
            // var block = FetchBlock();
            // BlockQueueService.Queue(block);
        }

        return Task.CompletedTask;
    }
}
