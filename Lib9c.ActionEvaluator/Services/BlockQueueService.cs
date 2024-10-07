using Libplanet.Types.Blocks;

namespace Lib9c.ActionEvaluator.Services;

public class BlockQueueService
{
    private  List<Block> _blocks = new List<Block>();
    private readonly object _lock = new object();

    public void Enqueue(Block block)
    {
        lock (_lock)
        {
            _blocks.Add(block);
        }
    }

    public Block Dequeue()
    {
        lock (_lock)
        {
            if (_blocks.Count == 0)
            {
                return null;
            }
            var block = _blocks[0];
            _blocks.RemoveAt(0);
            return block;
        }
    }
}
