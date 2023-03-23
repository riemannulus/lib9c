using System;
using System.Linq;
using Libplanet.Consensus;

namespace Libplanet.Tests.Common.Action
{
    public static class ValidatorSetOperatorTypeExtensions
    {
        public static Func<ValidatorSet, Validator, ValidatorSet>
            ToFunc(this ValidatorSetOperatorType @operator)
        {
            switch (@operator)
            {
                case ValidatorSetOperatorType.Append:
                    return (set, validator) =>
                    {
                        if (set.Validators.Any(val =>
                                val.OperatorAddress.Equals(validator.OperatorAddress)))
                        {
                            return set;
                        }

                        return set.Update(validator);
                    };
                case ValidatorSetOperatorType.Remove:
                    return (set, validator) =>
                        set.Update(new Validator(validator.PublicKey, 0));
                case ValidatorSetOperatorType.Update:
                    return (set, validator) => set.Update(validator);
            }

            throw new ArgumentException(
                "Unsupported operator: " + @operator, nameof(@operator));
        }
    }
}
