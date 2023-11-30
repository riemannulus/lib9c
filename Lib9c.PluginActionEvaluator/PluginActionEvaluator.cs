using System.Security.Cryptography;
using Libplanet.Action;
using Libplanet.Common;
using Libplanet.Store;
using Lib9c.PluginBase;
using Nekoyume.Action;
using Nekoyume.Action.Loader;
using Libplanet.Extensions.ActionEvaluatorCommonComponents;
using Libplanet.RocksDBStore;

namespace Lib9c.PluginActionEvaluator
{
    public class PluginActionEvaluator : IPluginActionEvaluator
    {
        private readonly IActionEvaluator _actionEvaluator;
        private readonly IStateStore _stateStore;

        public PluginActionEvaluator(IPluginKeyValueStore keyValueStore)
        {
            var kvStore = new WrappedKeyValueStore(keyValueStore);
            _stateStore = new TrieStateStore(kvStore);
            _actionEvaluator = new ActionEvaluator(
                _ => new RewardGold(),
                _stateStore,
                new NCActionLoader());
        }

        public byte[][] Evaluate(byte[] blockBytes, byte[]? baseStateRootHashBytes)
        {
            return _actionEvaluator.Evaluate(
                PreEvaluationBlockMarshaller.Deserialize(blockBytes),
                baseStateRootHashBytes is { } bytes ? new HashDigest<SHA256>(bytes) : null)
                .Select(eval => ActionEvaluationMarshaller.Serialize(eval)).ToArray();
        }

        public bool HasTrie(byte[] hash)
        {
            try
            {
                _stateStore.GetStateRoot(new HashDigest<SHA256>(hash));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
