namespace Nekoyume.BlockChain.Policy
{
    public static class MaxTransactionsPerSignerPerBlockPolicy
    {
        public static VariableSubPolicy<int> Default
        {
            get
            {
                return VariableSubPolicy<int>
                    .Create(int.MaxValue);
            }
        }

        public static VariableSubPolicy<int> Mainnet
        {
            get
            {
                return VariableSubPolicy<int>
                    .Create(int.MaxValue)
                    .Add(new SpannedSubPolicy<int>(
                        startIndex: BlockPolicySource.MinTransactionsPerBlockStartIndex,
                        value: BlockPolicySource.MaxTransactionsPerSignerPerBlock));
            }
        }
    }
}
