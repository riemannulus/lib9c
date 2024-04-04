using System.Collections.Generic;
using System.Linq;
using Bencodex.Types;
using Nekoyume.Model.Rune;

namespace Nekoyume.Model.State
{
    public class AllRuneState : IState
    {
        private Dictionary<int, RuneState> Runes { get; }

        public AllRuneState()
        {
            Runes = new Dictionary<int, RuneState>();
        }

        public AllRuneState(int runeId, int level = 0)
        {
            Runes = new Dictionary<int, RuneState>
            {
                { runeId, new RuneState(runeId, level) }
            };
        }

        public AllRuneState(List serialized)
        {
            Runes = new Dictionary<int, RuneState>();
            foreach (var item in serialized.OfType<List>())
            {
                Runes.Add(item[0].ToInteger(), new RuneState(item));
            }
        }

        public bool TryGetRuneState(int runeId, out RuneState runeState)
        {
            runeState = GetRuneState(runeId);
            return runeState is not null;
        }

        public RuneState GetRuneState(int runeId)
        {
            return Runes.TryGetValue(runeId, out var runeState)
                ? runeState
                : null;
        }


        public void AddRuneState(int runeId, int level = 0)
        {
            if (Runes.ContainsKey(runeId))
            {
                throw new DuplicatedRuneIdException($"Rune ID {runeId} already exists");
            }

            Runes[runeId] = new RuneState(runeId, level);
        }

        public void AddRuneState(RuneState runeState)
        {
            if (Runes.ContainsKey(runeState.RuneId))
            {
                throw new DuplicatedRuneIdException($"Rune ID {runeState.RuneId} already exists");
            }

            Runes[runeState.RuneId] = runeState;
        }

        public void SetRuneState(int runeId, int level)
        {
            if (!Runes.ContainsKey(runeId))
            {
                throw new RuneNotFoundException($"Rune ID {runeId} not exists.");
            }

            var rune = Runes[runeId];
            rune.LevelUp(level - rune.Level);
        }

        public void SetRuneState(RuneState runeState)
        {
            if (!Runes.ContainsKey(runeState.RuneId))
            {
                throw new RuneNotFoundException($"Rune ID {runeState.RuneId} not exists.");
            }

            Runes[runeState.RuneId] = runeState;
        }

        public IValue Serialize()
        {
            return Runes.OrderBy(r => r.Key).Aggregate(
                List.Empty,
                (current, rune) => current.Add(rune.Value.Serialize())
            );
        }
    }
}
