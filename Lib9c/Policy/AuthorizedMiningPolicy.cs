using System.Collections.Immutable;
using Libplanet;

namespace Nekoyume.BlockChain.Policy
{
    public static class AuthorizedMiningPolicy
    {
        public static VariableSubPolicy<ImmutableHashSet<Address>> Default
        {
            get
            {
                return VariableSubPolicy<ImmutableHashSet<Address>>
                    .Create(ImmutableHashSet<Address>.Empty);
            }
        }

        public static VariableSubPolicy<ImmutableHashSet<Address>> Mainnet
        {
            get
            {
                return VariableSubPolicy<ImmutableHashSet<Address>>
                    .Create(ImmutableHashSet<Address>.Empty)
                    .Add(new SpannedSubPolicy<ImmutableHashSet<Address>>(
                        startIndex: 0,
                        endIndex: BlockPolicySource.AuthorizedMiningPolicyEndIndex,
                        interval: BlockPolicySource.AuthorizedMiningPolicyInterval,
                        value: BlockPolicySource.AuthorizedMiners));
            }
        }
    }
}
