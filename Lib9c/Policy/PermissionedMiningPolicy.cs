using System.Collections.Immutable;
using Libplanet;

namespace Nekoyume.BlockChain.Policy
{
    public static class PermissionedMiningPolicy
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
                        startIndex: BlockPolicySource.PermissionedMiningStartIndex,
                        endIndex: null,
                        interval: 1,
                        value: BlockPolicySource.AuthorizedMiners));
            }
        }
    }
}
