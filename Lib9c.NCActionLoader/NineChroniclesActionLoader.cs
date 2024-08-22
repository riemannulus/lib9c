using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Action.Loader;

namespace Lib9c.NCActionLoader;

public class NineChroniclesActionLoader : IActionLoader
{
    private readonly IActionLoader _actionLoader = new Nekoyume.Action.Loader.NCActionLoader();

    public NineChroniclesActionLoader()
    {
    }

    public IAction LoadAction(long index, IValue value) =>
        _actionLoader.LoadAction(index, value);
}
