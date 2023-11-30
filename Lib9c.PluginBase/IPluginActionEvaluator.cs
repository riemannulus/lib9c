namespace Lib9c.PluginBase
{
    public interface IPluginActionEvaluator
    {
        byte[][] Evaluate(byte[] blockBytes, byte[]? baseStateRootHashBytes);

        bool HasTrie(byte[] hash);
    }
}
