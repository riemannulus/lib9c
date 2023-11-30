using System.Reflection;
using System.Security.Cryptography;
using Lib9c.PluginBase;
using Libplanet.Action;
using Libplanet.Action.Loader;
using Libplanet.Common;
using Libplanet.Extensions.ActionEvaluatorCommonComponents;
using Libplanet.RocksDBStore;
using Libplanet.Types.Blocks;

namespace Libplanet.Extensions.PluginActionEvaluator
{
    public class PluggedActionEvaluator : IActionEvaluator
    {
        private readonly IPluginActionEvaluator _pluginActionEvaluator;

        public IActionLoader ActionLoader => throw new NotImplementedException();

        public PluggedActionEvaluator(string pluginPath, string aevTypeName, RocksDBKeyValueStore keyValueStore)
        {
            _pluginActionEvaluator = CreateActionEvaluator(pluginPath, aevTypeName, keyValueStore);
        }

        public static Assembly LoadPlugin(string absolutePath)
        {
            PluginLoadContext loadContext = new PluginLoadContext(absolutePath);
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(absolutePath)));
        }

        public static IPluginActionEvaluator CreateActionEvaluator(Assembly assembly, string aevTypeName, RocksDBKeyValueStore keyValueStore)
        {
            if (assembly.GetType(aevTypeName) is Type type &&
                Activator.CreateInstance(type, args: new PluginRocksDBKeyValueStore(keyValueStore)) as IPluginActionEvaluator
                is IPluginActionEvaluator pluginActionEvaluator)
            {
                return pluginActionEvaluator;
            }

            throw new NullReferenceException("PluginActionEvaluator not found with given parameters");
        }

        public bool HasTrie(byte[] hash)
            => _pluginActionEvaluator.HasTrie(hash);

        public static IPluginActionEvaluator CreateActionEvaluator(string pluginPath, string aevTypeName, RocksDBKeyValueStore keyValueStore)
            => CreateActionEvaluator(LoadPlugin(pluginPath), aevTypeName, keyValueStore);

        public IReadOnlyList<ICommittedActionEvaluation> Evaluate(IPreEvaluationBlock block, HashDigest<SHA256>? baseStateRootHash)
            => _pluginActionEvaluator.Evaluate(
                PreEvaluationBlockMarshaller.Serialize(block),
                baseStateRootHash is { } srh ? srh.ToByteArray() : null)
                .Select(eval => ActionEvaluationMarshaller.Deserialize(eval)).ToList().AsReadOnly();
    }
}
