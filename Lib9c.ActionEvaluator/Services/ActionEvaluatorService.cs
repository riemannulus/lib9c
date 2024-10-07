using Lib9c.Plugin.Shared;
using Libplanet.Action;
using Libplanet.Extensions.ActionEvaluatorCommonComponents;

namespace Lib9c.ActionEvaluator.Services;

public class ActionEvaluatorService
{
    private IPluginActionEvaluator _actionEvaluator;

    public ActionEvaluatorService(IPluginActionEvaluator actionEvaluator){
        _actionEvaluator = actionEvaluator;
    }

    public IEnumerable<ICommittedActionEvaluation> Evaluate(Libplanet.Types.Blocks.Block block)
    {
        var evals = _actionEvaluator?.Evaluate(block.Serialize(), block.StateRootHash.ToByteArray());
        return evals != null
            ? evals.Select(ActionEvaluationMarshaller.Deserialize)
            : Enumerable.Empty<ICommittedActionEvaluation>();
    }

}
