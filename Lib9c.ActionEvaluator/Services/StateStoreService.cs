using System.Security.Cryptography;
using Bencodex.Types;
using Libplanet.Action.State;
using Libplanet.Common;
using Libplanet.Crypto;
using Libplanet.Store;
using Libplanet.Store.Trie;

namespace Lib9c.ActionEvaluator.Services;

public class StateStoreService
{
    public TrieStateStore StateStore { get; }

    public StateStoreService(IKeyValueStore keyValueStore)
    {
        StateStore = new TrieStateStore(keyValueStore);
    }

    public IValue? GetState(HashDigest<SHA256> srh, Address account, Address key)
    {
        var world = new WorldBaseState(StateStore.GetStateRoot(srh), StateStore);
        return world.GetAccountState(account).GetState(key);
    }
}
