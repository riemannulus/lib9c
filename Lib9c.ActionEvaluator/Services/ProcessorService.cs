using Libplanet.Action;
using Libplanet.Extensions.ActionEvaluatorCommonComponents;
using Libplanet.Types.Blocks;

namespace Lib9c.ActionEvaluator.Services;

public class ProcessorService : BackgroundService
{
    private readonly ActionEvaluatorService _aevService;
    private readonly BlockFetcherService _blockFetcherService;
    private readonly IActionEvaluationHub _actionEvaluationHub;

    public ProcessorService(
        BlockFetcherService blockFetcherService,
        ActionEvaluatorService aevService,
        IActionEvaluationHub actionEvaluationHub)
    {
        _aevService = aevService;
        _blockFetcherService = blockFetcherService;
        _actionEvaluationHub = actionEvaluationHub;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        ProcessBlockAsync(stoppingToken);

    private async Task ProcessBlockAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            Block block = _blockFetcherService.BlockQueueService.Dequeue();

            // FIXME: This is a temporary implementation. Use correct oldTip.
            await _actionEvaluationHub.BroadcastRenderBlockAsync(
                oldTip: block.Serialize(),
                newTip: block.Serialize());

            IEnumerable<ICommittedActionEvaluation> evals = _aevService.Evaluate(block);

            // Process evals
            foreach (ICommittedActionEvaluation eval in evals)
            {
                await _actionEvaluationHub.BroadcastRenderAsync(eval.Serialize());
            }

            await Task.Delay(1000, cancellationToken);
        }
    }
}
