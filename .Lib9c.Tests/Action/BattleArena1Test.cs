namespace Lib9c.Tests.Action
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Libplanet.Action;
    using Libplanet.Action.State;
    using Libplanet.Crypto;
    using Libplanet.Types.Assets;
    using Nekoyume;
    using Nekoyume.Action;
    using Nekoyume.Arena;
    using Nekoyume.Model;
    using Nekoyume.Model.Arena;
    using Nekoyume.Model.EnumType;
    using Nekoyume.Model.Item;
    using Nekoyume.Model.State;
    using Nekoyume.TableData;
    using Serilog;
    using Xunit;
    using Xunit.Abstractions;
    using static Lib9c.SerializeKeys;

    public class BattleArena1Test
    {
        private readonly Dictionary<string, string> _sheets;
        private readonly TableSheets _tableSheets;

        private readonly Address _agent1Address;
        private readonly Address _agent2Address;
        private readonly Address _agent3Address;
        private readonly Address _agent4Address;
        private readonly Address _avatar1Address;
        private readonly Address _avatar2Address;
        private readonly Address _avatar3Address;
        private readonly Address _avatar4Address;
        private readonly AvatarState _avatar1;
        private readonly AvatarState _avatar4;
        private readonly Currency _crystal;
        private readonly Currency _ncg;
        private IAccount _state;

        public BattleArena1Test(ITestOutputHelper outputHelper)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.TestOutput(outputHelper)
                .CreateLogger();

            _state = new MockStateDelta();

            _sheets = TableSheetsImporter.ImportSheets();
            var tableSheets = new TableSheets(_sheets);
            foreach (var (key, value) in _sheets)
            {
                _state = _state.SetState(Addresses.TableSheet.Derive(key), value.Serialize());
            }

            _tableSheets = new TableSheets(_sheets);
#pragma warning disable CS0618
            // Use of obsolete method Currency.Legacy(): https://github.com/planetarium/lib9c/discussions/1319
            _crystal = Currency.Legacy("CRYSTAL", 18, null);
            _ncg = Currency.Legacy("NCG", 2, null);
#pragma warning restore CS0618
            var goldCurrencyState = new GoldCurrencyState(_ncg);

            var rankingMapAddress = new PrivateKey().ToAddress();
            var clearStageId = Math.Max(
                _tableSheets.StageSheet.First?.Id ?? 1,
                GameConfig.RequireClearedStageLevel.ActionsInRankingBoard);

            // account 1
            var (agent1State, avatar1State) = GetAgentStateWithAvatarState(
                _sheets,
                _tableSheets,
                rankingMapAddress,
                clearStageId);

            _agent1Address = agent1State.address;
            _avatar1 = avatar1State;
            _avatar1Address = avatar1State.address;

            // account 2
            var (agent2State, avatar2State) = GetAgentStateWithAvatarState(
                _sheets,
                _tableSheets,
                rankingMapAddress,
                clearStageId);
            _agent2Address = agent2State.address;
            _avatar2Address = avatar2State.address;

            // account 3
            var (agent3State, avatar3State) = GetAgentStateWithAvatarState(
                _sheets,
                _tableSheets,
                rankingMapAddress,
                1);
            _agent3Address = agent3State.address;
            _avatar3Address = avatar3State.address;

            // account 4
            var (agent4State, avatar4State) = GetAgentStateWithAvatarState(
                _sheets,
                _tableSheets,
                rankingMapAddress,
                1);

            _agent4Address = agent4State.address;
            _avatar4 = avatar4State;
            _avatar4Address = avatar4State.address;

            _state = _state
                .SetState(Addresses.GoldCurrency, goldCurrencyState.Serialize())
                .SetState(_agent1Address, agent1State.Serialize())
                .SetState(_avatar1Address.Derive(LegacyInventoryKey), _avatar1.inventory.Serialize())
                .SetState(_avatar1Address.Derive(LegacyWorldInformationKey), _avatar1.worldInformation.Serialize())
                .SetState(_avatar1Address.Derive(LegacyQuestListKey), _avatar1.questList.Serialize())
                .SetState(_avatar1Address, _avatar1.Serialize())
                .SetState(_agent2Address, agent2State.Serialize())
                .SetState(_avatar2Address, avatar2State.Serialize())
                .SetState(_agent3Address, agent3State.Serialize())
                .SetState(_avatar3Address, avatar3State.Serialize())
                .SetState(_agent4Address, agent4State.Serialize())
                .SetState(_avatar4Address.Derive(LegacyInventoryKey), _avatar4.inventory.Serialize())
                .SetState(_avatar4Address.Derive(LegacyWorldInformationKey), _avatar4.worldInformation.Serialize())
                .SetState(_avatar4Address.Derive(LegacyQuestListKey), _avatar4.questList.Serialize())
                .SetState(_avatar4Address, avatar4State.Serialize())
                .SetState(Addresses.GameConfig, new GameConfigState(_sheets[nameof(GameConfigSheet)]).Serialize());

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.TestOutput(outputHelper)
                .CreateLogger();
        }

        public static (AgentState AgentState, AvatarState AvatarState) GetAgentStateWithAvatarState(
            IReadOnlyDictionary<string, string> sheets,
            TableSheets tableSheets,
            Address rankingMapAddress,
            int clearStageId)
        {
            var agentAddress = new PrivateKey().ToAddress();
            var agentState = new AgentState(agentAddress);

            var avatarAddress = agentAddress.Derive("avatar");
            var avatarState = new AvatarState(
                avatarAddress,
                agentAddress,
                0,
                tableSheets.GetAvatarSheets(),
                new GameConfigState(sheets[nameof(GameConfigSheet)]),
                rankingMapAddress)
            {
                worldInformation = new WorldInformation(
                    0,
                    tableSheets.WorldSheet,
                    clearStageId),
            };
            agentState.avatarAddresses.Add(0, avatarAddress);

            return (agentState, avatarState);
        }

        public (List<Guid> Equipments, List<Guid> Costumes) GetDummyItems(AvatarState avatarState)
        {
            var items = Doomfist.GetAllParts(_tableSheets, avatarState.level);
            foreach (var equipment in items)
            {
                avatarState.inventory.AddItem(equipment);
            }

            var equipments = items.Select(e => e.NonFungibleId).ToList();

            var random = new TestRandom();
            var costumes = new List<Guid>();
            if (avatarState.level >= GameConfig.RequireCharacterLevel.CharacterFullCostumeSlot)
            {
                var costumeId = _tableSheets
                    .CostumeItemSheet
                    .Values
                    .First(r => r.ItemSubType == ItemSubType.FullCostume)
                    .Id;

                var costume = (Costume)ItemFactory.CreateItem(
                    _tableSheets.ItemSheet[costumeId], random);
                avatarState.inventory.AddItem(costume);
                costumes.Add(costume.ItemId);
            }

            return (equipments, costumes);
        }

        public IAccount JoinArena(IActionContext context, Address signer, Address avatarAddress, long blockIndex, int championshipId, int round, IRandom random)
        {
            var preCurrency = 1000 * _crystal;
            _state = _state.MintAsset(context, signer, preCurrency);

            var action = new JoinArena1()
            {
                championshipId = championshipId,
                round = round,
                costumes = new List<Guid>(),
                equipments = new List<Guid>(),
                avatarAddress = avatarAddress,
            };

            _state = action.Execute(new ActionContext
            {
                PreviousState = _state,
                Signer = signer,
                Random = random,
                Rehearsal = false,
                BlockIndex = blockIndex,
            });
            return _state;
        }

        [Theory]
        [InlineData(1, 1, 1, 1, 2, 3)]
        [InlineData(1, 1, 1, 1, 2, 4)]
        [InlineData(1, 1, 1, 5, 2, 0)]
        [InlineData(1, 1, 1, 5, 2, 1)]
        [InlineData(1, 1, 2, 5, 2, 0)]
        [InlineData(1, 1, 2, 5, 2, 1)]
        public void Execute(long nextBlockIndex, int championshipId, int round, int ticket, int arenaInterval, int randomSeed)
        {
            var context = new ActionContext();
            var arenaSheet = _state.GetSheet<ArenaSheet>();
            if (!arenaSheet.TryGetValue(championshipId, out var row))
            {
                throw new SheetRowNotFoundException(
                    nameof(ArenaSheet), $"championship Id : {championshipId}");
            }

            if (!row.TryGetRound(round, out var roundData))
            {
                throw new RoundNotFoundException(
                    $"[{nameof(BattleArena1)}] ChampionshipId({row.ChampionshipId}) - round({round})");
            }

            var random = new TestRandom(randomSeed);
            _state = JoinArena(context, _agent1Address, _avatar1Address, roundData.StartBlockIndex, championshipId, round, random);
            _state = JoinArena(context, _agent2Address, _avatar2Address, roundData.StartBlockIndex, championshipId, round, random);

            var arenaInfoAdr = ArenaInformation.DeriveAddress(_avatar1Address, championshipId, round);
            if (!_state.TryGetArenaInformation(arenaInfoAdr, out var beforeInfo))
            {
                throw new ArenaInformationNotFoundException($"arenaInfoAdr : {arenaInfoAdr}");
            }

            var usedTicket = 2;
            beforeInfo.UseTicket(usedTicket);
            _state = _state.SetState(arenaInfoAdr, beforeInfo.Serialize());

            var buyTicket = ticket - beforeInfo.Ticket;
            if (buyTicket > 0)
            {
                var currency = buyTicket * _ncg * roundData.TicketPrice;
                _state = _state.MintAsset(context, _agent1Address, currency);
            }

            var action = new BattleArena1()
            {
                myAvatarAddress = _avatar1Address,
                enemyAvatarAddress = _avatar2Address,
                championshipId = championshipId,
                round = round,
                ticket = ticket,
                costumes = new List<Guid>(),
                equipments = new List<Guid>(),
            };

            var myScoreAdr = ArenaScore.DeriveAddress(_avatar1Address, championshipId, round);
            var enemyScoreAdr = ArenaScore.DeriveAddress(_avatar2Address, championshipId, round);
            if (!_state.TryGetArenaScore(myScoreAdr, out var beforeMyScore))
            {
                throw new ArenaScoreNotFoundException($"myScoreAdr : {myScoreAdr}");
            }

            if (!_state.TryGetArenaScore(enemyScoreAdr, out var beforeEnemyScore))
            {
                throw new ArenaScoreNotFoundException($"enemyScoreAdr : {enemyScoreAdr}");
            }

            Assert.Empty(_avatar1.inventory.Materials);

            var gameConfigState = SetArenaInterval(arenaInterval);
            _state = _state.SetState(GameConfigState.Address, gameConfigState.Serialize());

            var blockIndex = roundData.StartBlockIndex + nextBlockIndex;
            _state = action.Execute(new ActionContext
            {
                PreviousState = _state,
                Signer = _agent1Address,
                Random = random,
                Rehearsal = false,
                BlockIndex = blockIndex,
            });

            if (!_state.TryGetArenaScore(myScoreAdr, out var myAfterScore))
            {
                throw new ArenaScoreNotFoundException($"myScoreAdr : {myScoreAdr}");
            }

            if (!_state.TryGetArenaScore(enemyScoreAdr, out var enemyAfterScore))
            {
                throw new ArenaScoreNotFoundException($"enemyScoreAdr : {enemyScoreAdr}");
            }

            if (!_state.TryGetArenaInformation(arenaInfoAdr, out var afterInfo))
            {
                throw new ArenaInformationNotFoundException($"arenaInfoAdr : {arenaInfoAdr}");
            }

            var (myWinScore, myDefeatScore, enemyWinScore) =
                ArenaHelper.GetScoresV1(beforeMyScore.Score, beforeEnemyScore.Score);

            var addMyScore = (afterInfo.Win * myWinScore) + (afterInfo.Lose * myDefeatScore);
            var addEnemyScore = afterInfo.Win * enemyWinScore;
            var expectedMyScore = Math.Max(beforeMyScore.Score + addMyScore, ArenaScore.ArenaScoreDefault);
            var expectedEnemyScore = Math.Max(beforeEnemyScore.Score + addEnemyScore, ArenaScore.ArenaScoreDefault);

            Assert.Equal(expectedMyScore, myAfterScore.Score);
            Assert.Equal(expectedEnemyScore, enemyAfterScore.Score);
            Assert.Equal(ArenaInformation.MaxTicketCount - usedTicket, beforeInfo.Ticket);
            Assert.Equal(0, beforeInfo.Win);
            Assert.Equal(0, beforeInfo.Lose);

            var useTicket = Math.Min(ticket, beforeInfo.Ticket);
            Assert.Equal(beforeInfo.Ticket - useTicket, afterInfo.Ticket);
            Assert.Equal(ticket, afterInfo.Win + afterInfo.Lose);
            var balance = _state.GetBalance(_agent1Address, _state.GetGoldCurrency());
            Assert.Equal(0, balance.RawValue);

            var avatarState = _state.GetAvatarStateV2(_avatar1Address);
            var medalCount = 0;
            if (roundData.ArenaType != ArenaType.OffSeason)
            {
                var medalId = ArenaHelper.GetMedalItemId(championshipId, round);
                avatarState.inventory.TryGetItem(medalId, out var medal);
                Assert.Equal(afterInfo.Win, medal.count);
                medalCount = medal.count;
            }

            var materialCount = avatarState.inventory.Materials.Count();
            var high = (ArenaHelper.GetRewardCount(beforeMyScore.Score) * ticket) + medalCount;
            Assert.InRange(materialCount, 0, high);
        }

        public GameConfigState SetArenaInterval(int interval)
        {
            var gameConfigState = _state.GetGameConfigState();
            var sheet = _tableSheets.GameConfigSheet;
            foreach (var value in sheet.Values)
            {
                if (value.Key.Equals("daily_arena_interval"))
                {
                    IReadOnlyList<string> field = new[]
                    {
                        value.Key,
                        interval.ToString(),
                    };
                    value.Set(field);
                }
            }

            gameConfigState.Set(sheet);
            return gameConfigState;
        }

        [Fact]
        public void Execute_InvalidAddressException()
        {
            var action = new BattleArena1()
            {
                myAvatarAddress = _avatar1Address,
                enemyAvatarAddress = _avatar1Address,
                championshipId = 1,
                round = 1,
                ticket = 1,
                costumes = new List<Guid>(),
                equipments = new List<Guid>(),
            };

            Assert.Throws<InvalidAddressException>(() => action.Execute(new ActionContext()
            {
                PreviousState = _state,
                Signer = _agent1Address,
                Random = new TestRandom(),
            }));
        }

        [Fact]
        public void Execute_FailedLoadStateException()
        {
            var action = new BattleArena1()
            {
                myAvatarAddress = _avatar2Address,
                enemyAvatarAddress = _avatar1Address,
                championshipId = 1,
                round = 1,
                ticket = 1,
                costumes = new List<Guid>(),
                equipments = new List<Guid>(),
            };

            Assert.Throws<FailedLoadStateException>(() => action.Execute(new ActionContext()
            {
                PreviousState = _state,
                Signer = _agent1Address,
                Random = new TestRandom(),
            }));
        }

        [Fact]
        public void Execute_NotEnoughClearedStageLevelException()
        {
            var action = new BattleArena1()
            {
                myAvatarAddress = _avatar4Address,
                enemyAvatarAddress = _avatar2Address,
                championshipId = 1,
                round = 1,
                ticket = 1,
                costumes = new List<Guid>(),
                equipments = new List<Guid>(),
            };

            Assert.Throws<NotEnoughClearedStageLevelException>(() => action.Execute(new ActionContext()
            {
                PreviousState = _state,
                Signer = _agent4Address,
                Random = new TestRandom(),
                BlockIndex = 1,
            }));
        }

        [Fact]
        public void Execute_SheetRowNotFoundException()
        {
            var action = new BattleArena1()
            {
                myAvatarAddress = _avatar1Address,
                enemyAvatarAddress = _avatar2Address,
                championshipId = 9999999,
                round = 1,
                ticket = 1,
                costumes = new List<Guid>(),
                equipments = new List<Guid>(),
            };

            Assert.Throws<SheetRowNotFoundException>(() => action.Execute(new ActionContext()
            {
                PreviousState = _state,
                Signer = _agent1Address,
                Random = new TestRandom(),
            }));
        }

        [Fact]
        public void Execute_ThisArenaIsClosedException()
        {
            var action = new BattleArena1()
            {
                myAvatarAddress = _avatar1Address,
                enemyAvatarAddress = _avatar2Address,
                championshipId = 1,
                round = 1,
                ticket = 1,
                costumes = new List<Guid>(),
                equipments = new List<Guid>(),
            };

            Assert.Throws<ThisArenaIsClosedException>(() => action.Execute(new ActionContext()
            {
                PreviousState = _state,
                Signer = _agent1Address,
                Random = new TestRandom(),
                BlockIndex = 4480001,
            }));
        }

        [Fact]
        public void Execute_ArenaParticipantsNotFoundException()
        {
            var action = new BattleArena1()
            {
                myAvatarAddress = _avatar1Address,
                enemyAvatarAddress = _avatar2Address,
                championshipId = 1,
                round = 1,
                ticket = 1,
                costumes = new List<Guid>(),
                equipments = new List<Guid>(),
            };

            Assert.Throws<ArenaParticipantsNotFoundException>(() => action.Execute(new ActionContext()
            {
                PreviousState = _state,
                Signer = _agent1Address,
                Random = new TestRandom(),
                BlockIndex = 1,
            }));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Execute_AddressNotFoundInArenaParticipantsException(bool excludeMe)
        {
            var context = new ActionContext();
            var championshipId = 1;
            var round = 1;
            var arenaSheet = _state.GetSheet<ArenaSheet>();
            if (!arenaSheet.TryGetValue(championshipId, out var row))
            {
                throw new SheetRowNotFoundException(
                    nameof(ArenaSheet), $"championship Id : {championshipId}");
            }

            if (!row.TryGetRound(round, out var roundData))
            {
                throw new RoundNotFoundException(
                    $"[{nameof(BattleArena1)}] ChampionshipId({row.ChampionshipId}) - round({round})");
            }

            var random = new TestRandom();
            _state = excludeMe
                ? JoinArena(context, _agent2Address, _avatar2Address, roundData.StartBlockIndex, championshipId, round, random)
                : JoinArena(context, _agent1Address, _avatar1Address, roundData.StartBlockIndex, championshipId, round, random);

            var action = new BattleArena1()
            {
                myAvatarAddress = _avatar1Address,
                enemyAvatarAddress = _avatar2Address,
                championshipId = championshipId,
                round = round,
                ticket = 1,
                costumes = new List<Guid>(),
                equipments = new List<Guid>(),
            };

            Assert.Throws<AddressNotFoundInArenaParticipantsException>(() => action.Execute(new ActionContext()
            {
                PreviousState = _state,
                Signer = _agent1Address,
                Random = new TestRandom(),
                BlockIndex = 1,
            }));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Execute_ValidateScoreDifferenceException(bool isSigner)
        {
            var context = new ActionContext();
            var championshipId = 1;
            var round = 2;
            var arenaSheet = _state.GetSheet<ArenaSheet>();
            if (!arenaSheet.TryGetValue(championshipId, out var row))
            {
                throw new SheetRowNotFoundException(
                    nameof(ArenaSheet), $"championship Id : {championshipId}");
            }

            if (!row.TryGetRound(round, out var roundData))
            {
                throw new RoundNotFoundException(
                    $"[{nameof(BattleArena1)}] ChampionshipId({row.ChampionshipId}) - round({round})");
            }

            var random = new TestRandom();
            _state = JoinArena(context, _agent1Address, _avatar1Address, roundData.StartBlockIndex, championshipId, round, random);
            _state = JoinArena(context, _agent2Address, _avatar2Address, roundData.StartBlockIndex, championshipId, round, random);

            var arenaScoreAdr = ArenaScore.DeriveAddress(isSigner ? _avatar1Address : _avatar2Address, roundData.ChampionshipId, roundData.Round);
            _state.TryGetArenaScore(arenaScoreAdr, out var arenaScore);
            arenaScore.AddScore(900);
            _state = _state.SetState(arenaScoreAdr, arenaScore.Serialize());

            var action = new BattleArena1()
            {
                myAvatarAddress = _avatar1Address,
                enemyAvatarAddress = _avatar2Address,
                championshipId = championshipId,
                round = round,
                ticket = 1,
                costumes = new List<Guid>(),
                equipments = new List<Guid>(),
            };

            var blockIndex = roundData.StartBlockIndex + 1;
            Assert.Throws<ValidateScoreDifferenceException>(() => action.Execute(new ActionContext()
            {
                BlockIndex = blockIndex,
                PreviousState = _state,
                Signer = _agent1Address,
                Random = new TestRandom(),
            }));
        }
    }
}
