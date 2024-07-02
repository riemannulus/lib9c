using System.Collections.Immutable;
using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Nekoyume.Action.DPoS.Util;
using Nekoyume.Module;

namespace Nekoyume.Action.DPoS.Sys
{
    /// <summary>
    /// An action for mortgage gas fee for a transaction.
    /// Should be executed at the beginning of the tx.
    /// </summary>
    public sealed class Mortgage : ActionBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="Mortgage"/>.
        /// </summary>
        public Mortgage()
        {
        }

        /// <inheritdoc cref="IAction.PlainValue"/>
        public override IValue PlainValue => new Bencodex.Types.Boolean(true);

        /// <inheritdoc cref="IAction.LoadPlainValue(IValue)"/>
        public override void LoadPlainValue(IValue plainValue)
        {
            // Method intentionally left empty.
        }

        /// <inheritdoc cref="IAction.Execute(IActionContext)"/>
        public override IWorld Execute(IActionContext context)
        {
            IWorld state = context.PreviousState;
            if (context.MaxGasPrice is not { } realGasPrice)
            {
                return state;
            }

            Address paymaster = context.Signer;
            Address pledgeContractAddress = context.Signer.GetPledgeAddress();
            if (state.TryGetLegacyState(pledgeContractAddress, out List contract)
                && contract[1].ToBoolean())
            {
                paymaster = contract[0].ToAddress();
            }

            FungibleAssetValue balance = state.GetBalance(paymaster, realGasPrice.Currency);
            if (balance >= realGasPrice * context.GasLimit())
                return state.BurnAsset(
                    context,
                    paymaster,
                    realGasPrice * context.GasLimit());

            string msg =
                $"The account {paymaster}'s balance of {realGasPrice.Currency} is " +
                "insufficient to pay gas fee: " +
                $"{balance} < {realGasPrice * context.GasLimit()}.";
            throw new InsufficientBalanceException(msg, paymaster, balance);
        }
    }
}
