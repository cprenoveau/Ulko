using System;
using System.Collections.Generic;
using System.Linq;
using Ulko.Data;
using Ulko.Data.Abilities;
using UnityEngine;

namespace Ulko.Battle
{
    public class BattleInstance : IDisposable
    {
        public delegate void ShowDialogueDelegate(Data.Dialogue dialogue, System.Action callback);
        public event ShowDialogueDelegate OnShowDialogue;

        public BattleAsset BattleAsset { get; private set; }
        public Battlefield Battlefield { get; private set; }
        public List<Character> Heroes { get; private set; } = new List<Character>();
        public List<Character> Enemies { get; private set; } = new List<Character>();
        public Data.Timeline.IMilestone Milestone { get; private set; }
        public BattleConfig Config { get; private set; }

        public DeckOfCards<AbilityCard> DrawPile { get; private set; } = new DeckOfCards<AbilityCard>();
        public DeckOfCards<AbilityCard> DiscardPile { get; private set; } = new DeckOfCards<AbilityCard>();
        public HandOfCards<AbilityCard> CurrentHand { get; private set; } = new HandOfCards<AbilityCard>();
        public int FreeRedrawInTurns { get; private set; }

        private readonly Character heroPrefab;
        private readonly Character enemyPrefab;
        private readonly Transform parent;

        private readonly List<ICharacterInternal> heroInfos = new();

        private List<(string hero, int exp)> expEarned;
        public List<(string hero, int exp)> GetExp()
        {
            if (expEarned != null)
                return expEarned;

            var participants = new List<string>();
            var benchers = new List<string>();

            var party = PlayerProfile.ActiveParty;
            foreach (var hero in party)
            {
                var participant = heroInfos.FirstOrDefault(h => h.Id == hero.id);
                if (participant != null)
                {
                    if (participant.HP > 0)
                    {
                        participants.Add(participant.Id);
                    }
                    else
                    {
                        benchers.Add(participant.Id); //count dead heroes as benchers
                    }
                }
                else
                {
                    benchers.Add(hero.id);
                }
            }

            float x = TotalExp() / (participants.Count + 0.5f * benchers.Count);

            expEarned = new List<(string hero, int exp)>();
            foreach(var participant in participants)
            {
                expEarned.Add(new (participant, Mathf.RoundToInt(x)));
            }

            foreach(var bencher in benchers)
            {
                expEarned.Add(new(bencher, Mathf.RoundToInt(x * 0.5f)));
            }

            return expEarned;
        }

        public int TotalExp()
        {
            int exp = 0;
            foreach(var enemy in Enemies)
            {
                exp += enemy.Exp;
            }

            return exp;
        }

        public static BattleInstance Create(
            BattleAsset battleAsset,
            Battlefield battlefield,
            Character heroPrefab,
            Character enemyPrefab,
            Transform parent,
            Data.Timeline.IMilestone milestone,
            BattleConfig config)
        {
            var instance = new BattleInstance(battleAsset, battlefield, heroPrefab, enemyPrefab, parent, milestone, config);

            instance.RefreshHeroes();
            instance.RefreshEnemies();

            return instance;
        }

        private BattleInstance(
            BattleAsset battleAsset,
            Battlefield battlefield,
            Character heroPrefab,
            Character enemyPrefab,
            Transform parent,
            Data.Timeline.IMilestone milestone,
            BattleConfig config)
        {
            BattleAsset = battleAsset;
            Battlefield = battlefield;
            Milestone = milestone;
            Config = config;

            this.heroPrefab = heroPrefab;
            this.enemyPrefab = enemyPrefab;
            this.parent = parent;
        }

        public void RefreshHeroes()
        {
            InitHeroes(heroPrefab, parent, Config.maxHeroCount);

            var party = PlayerProfile.ActiveParty;
            int partyCount = Mathf.Min(party.Count(), Config.maxHeroCount);

            for (int i = 0; i < partyCount; ++i)
            {
                var hero = party.ElementAt(i);

                if (!Heroes[i].Initialized || Heroes[i].Id != hero.id)
                {
                    var info = GetOrCreateHero(hero.id);
                    Heroes[i].Init(info, Battlefield.HeroPosition(i, partyCount), i);

                    Heroes[i].SetAnimationState(Character.AnimState.Idle);
                }
            }

            RefreshDeck();

            CurrentHand.Flush();
            DrawHand();

            FreeRedrawInTurns = Config.freeRedrawInTurns;
        }

        private void RefreshDeck()
        {
            DiscardPile.Flush();
            DrawPile.Flush();

            foreach (var hero in Heroes)
            {
                foreach (var ability in hero.Abilities)
                {
                    DrawPile.TryAddCard(new Card<AbilityCard>(new AbilityCard(ability, hero.Id)));
                }
            }

            DrawPile.Shuffle();
        }

        private ICharacterInternal GetOrCreateHero(string heroId)
        {
            var hero = heroInfos.Find(h => h.Id == heroId);
            if(hero == null)
            {
                var heroAsset = Milestone.Party.Find(x => x.id == heroId);

                hero = new Hero(
                    heroAsset,
                    Database.Heroes[heroId],
                    Battlefield.heroStandDirection);

                heroInfos.Add(hero);
            }

            return hero;
        }

        private void InitHeroes(Character heroPrefab, Transform parent, int maxHeroCount)
        {
            var party = PlayerProfile.ActiveParty;
            int partyCount = Mathf.Min(party.Count(), maxHeroCount);

            if (Heroes.Count == partyCount)
                return;

            while (Heroes.Count > partyCount)
            {
                if (Heroes.Last() != null)
                    GameObject.Destroy(Heroes.Last().gameObject);

                Heroes.RemoveAt(Heroes.Count - 1);
            }

            while (Heroes.Count < partyCount)
            {
                Heroes.Add(GameObject.Instantiate(heroPrefab, parent));
            }
        }

        public void RefreshEnemies()
        {
            foreach (var enemy in Enemies)
            {
                GameObject.Destroy(enemy.gameObject);
            }

            for (int i = 0; i < BattleAsset.enemies.Count; ++i)
            {
                var enemy = BattleAsset.enemies[i];
                Vector3 pos = Battlefield.enemyPositions[enemy.positionIndex].transform.position;

                var instance = GameObject.Instantiate(enemyPrefab, parent);

                var enemyInfo = new Enemy(
                    enemy.asset,
                    Database.Enemies[enemy.asset.id],
                    Battlefield.enemyStandDirection,
                    BattleAsset.EnemySuffix(i));

                instance.Init(enemyInfo, pos, enemy.positionIndex);

                instance.SetAnimationState(Character.AnimState.Idle);
                Enemies.Add(instance);
            }
        }

        public void Dispose()
        {
            foreach(var hero in Heroes)
            {
                GameObject.Destroy(hero.gameObject);
            }

            foreach(var enemy in Enemies)
            {
                GameObject.Destroy(enemy.gameObject);
            }

            heroInfos.Clear();

            Heroes.Clear();
            Enemies.Clear();
        }

        public delegate void CharacterStateChanged(CharacterState oldState, CharacterState newState, CharacterAction action);
        public event CharacterStateChanged OnCharacterStateChanged;

        public void ApplyState(ActionState state)
        {
            foreach (var characterState in state.characters)
            {
                var character = FindCharacter(characterState.id);
                if (character != null)
                {
                    var oldState = character.CaptureState();

                    if(!oldState.Equals(characterState))
                    {
                        character.ApplyState(characterState);
                        OnCharacterStateChanged?.Invoke(oldState, characterState, state.pendingAction);
                    }
                }
            }
        }

        public List<CharacterState> CaptureCharacterStates()
        {
            return GetAllCharacters(FetchCondition.All).Select(c => c.CaptureState()).ToList();
        }

        public event Action OnIncrementTurnCount;

        public void IncrementTurnCount()
        {
            foreach(var character in GetAllCharacters(FetchCondition.AliveOnly))
            {
                character.IncrementTurnCount();
            }

            FreeRedrawInTurns--;
            if (FreeRedrawInTurns < 0) FreeRedrawInTurns = 0;

            OnIncrementTurnCount?.Invoke();
        }

        public Character FindCharacter(string id)
        {
            var hero = Heroes.FirstOrDefault(h => h.Id == id);
            if (hero != null) return hero;

            return Enemies.FirstOrDefault(e => e.Id == id);
        }

        public Character FindFirstCharacterNoSuffix(string id)
        {
            var hero = Heroes.FirstOrDefault(h => h.IdWithoutSuffix == id);
            if (hero != null) return hero;

            return Enemies.FirstOrDefault(e => e.IdWithoutSuffix == id);
        }

        public List<Character> FindCharacters(List<string> ids)
        {
            var list = new List<Character>();
            foreach (var id in ids)
                list.Add(FindCharacter(id));

            return list;
        }

        public enum FetchCondition
        {
            AliveOnly,
            DeadOnly,
            All
        }

        public List<Character> GetAllCharacters(FetchCondition condition)
        {
            var list = new List<Character>();
            list.AddRange(GetHeroes(condition));
            list.AddRange(GetEnemies(condition));

            return list;
        }

        public IEnumerable<Character> GetEnemies(FetchCondition condition)
        {
            switch (condition)
            {
                case FetchCondition.AliveOnly:
                    return Enemies.Where(e => !e.IsDead);

                case FetchCondition.DeadOnly:
                    return Enemies.Where(e => e.IsDead);

                default:
                    return Enemies;
            }
        }

        public IEnumerable<Character> GetHeroes(FetchCondition condition)
        {
            switch (condition)
            {
                case FetchCondition.AliveOnly:
                    return Heroes.Where(e => !e.IsDead);

                case FetchCondition.DeadOnly:
                    return Heroes.Where(e => e.IsDead);

                default:
                    return Heroes;
            }
        }

        public IEnumerable<Character> GetAlliesOf(CharacterSide side, FetchCondition condition)
        {
            if (side == CharacterSide.Heroes)
                return GetHeroes(condition);
            else
                return GetEnemies(condition);
        }

        public IEnumerable<Character> GetEnemiesOf(CharacterSide side, FetchCondition condition)
        {
            if (side == CharacterSide.Heroes)
                return GetEnemies(condition);
            else
                return GetHeroes(condition);
        }

        public List<Character> GetTargetCandidates(AbilityTarget target, CharacterState actor)
        {
            var candidates = new List<Character>();

            switch (target.targetType)
            {
                case AbilityTarget.TargetType.Allies:
                    candidates.AddRange(GetAlliesOf(actor.characterSide, FetchCondition.All));

                    if (!target.HasCondition(typeof(IsOnSameSideCondition)))
                        candidates.AddRange(GetEnemiesOf(actor.characterSide, FetchCondition.All));

                    break;

                case AbilityTarget.TargetType.Enemies:
                    candidates.AddRange(GetEnemiesOf(actor.characterSide, FetchCondition.All));

                    if (!target.HasCondition(typeof(IsOnSameSideCondition)))
                        candidates.AddRange(GetAlliesOf(actor.characterSide, FetchCondition.All));

                    break;
            }

            for (int i = 0; i < candidates.Count;)
            {
                if (!target.IsValidTarget(actor, candidates[i].CaptureState()))
                    candidates.RemoveAt(i);
                else
                    ++i;
            }

            return candidates;
        }

        public List<Character> GetRandomTargets(AbilityTarget target, List<Character> candidates)
        {
            if (candidates.Count == 0)
                return candidates;

            if (target.targetSize == AbilityTarget.TargetSize.One)
            {
                return new List<Character> { GetRandomSingleTarget(candidates) };
            }
            else
            {
                return candidates;
            }
        }

        public Character GetRandomSingleTarget(List<Character> candidates)
        {
            if (candidates.Count == 0)
                return null;

            var index = UnityEngine.Random.Range(0, candidates.Count);
            return candidates[index];
        }

        public List<BattleActions> GetPossibleHeroActions()
        {
            var actions = new List<BattleActions>();
            var characters = CaptureCharacterStates();

            int cardIndex = 0;
            foreach(var card in CurrentHand)
            {
                var actor = FindCharacter(card.Data.ownerId);
                if(actor != null)
                {
                    actions.AddRange(CreateActions(cardIndex, card.Data.abilityAsset, false, actor, characters));
                    cardIndex++;
                }
            }

            actions.AddRange(CreateActions(-1,Config.cardThrowAbility, true, Heroes.First(), characters));

            return actions;
        }

        public void RedrawHand(PlayerAction playerAction)
        {
            CurrentHand.Discard(CurrentHand.ToList(), DiscardPile);
            DrawHand();

            if (FreeRedrawInTurns > 0)
            {
                playerAction.CancelAction();
            }
            else
            {
                playerAction.PossibleActions = GetPossibleHeroActions();
            }

            FreeRedrawInTurns = Config.freeRedrawInTurns;
        }

        public void DrawHand()
        {
            int drawCards = Heroes.Count + 1 - CurrentHand.Count();
            if (drawCards > DrawPile.Count())
            {
                DrawPile.ShuffleDeckInto(DiscardPile);
            }

            DrawPile.DrawCards(drawCards, CurrentHand);
        }

        public List<BattleActions> GetPossibleEnemyActions(Character actor)
        {
            var actions = new List<BattleActions>();
            var characters = CaptureCharacterStates();

            actions.AddRange(CreateActions(0, actor.Abilities[UnityEngine.Random.Range(0, actor.Abilities.Count)], false, actor, characters));

            return actions;
        }

        private List<BattleActions> CreateActions(int cardIndex, AbilityAsset ability, bool isCardThrow, Character actor, List<CharacterState> characters)
        {
            var actions = new List<BattleActions>();
            var targetCandidates = GetTargetCandidates(ability.target, actor.CaptureState());

            if (targetCandidates.Count() == 0)
                return actions;

            if (ability.target.targetSize == AbilityTarget.TargetSize.One)
            {
                foreach (var target in targetCandidates)
                {
                    actions.Add(new BattleActions(cardIndex, ability, isCardThrow, actor.Id, new List<string> { target.Id }, characters));
                }
            }
            else
            {
                var enemies = targetCandidates.Where(c => c.CharacterSide == CharacterSide.Enemies).ToList();
                var heroes = targetCandidates.Where(c => c.CharacterSide == CharacterSide.Heroes).ToList();

                if (enemies.Count > 0)
                {
                    actions.Add(new BattleActions(cardIndex, ability, isCardThrow, actor.Id, enemies.Select(e => e.Id).ToList(), characters));
                }

                if (heroes.Count > 0)
                {
                    actions.Add(new BattleActions(cardIndex, ability, isCardThrow, actor.Id, heroes.Select(e => e.Id).ToList(), characters));
                }
            }

            return actions;
        }
    }
}