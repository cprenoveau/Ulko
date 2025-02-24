using System;
using System.Collections.Generic;
using System.Linq;
using Ulko.Data.Abilities;
using UnityEngine;

namespace Ulko.Battle
{
    public class BattleInstance : IDisposable
    {
        public delegate void ShowDialogueDelegate(Data.Dialogue dialogue, System.Action callback);
        public event ShowDialogueDelegate OnShowDialogue;

        public Data.BattleAsset BattleAsset { get; private set; }
        public Battlefield Battlefield { get; private set; }
        public List<Character> Heroes { get; private set; } = new List<Character>();
        public List<Character> Enemies { get; private set; } = new List<Character>();
        public Data.Timeline.IMilestone Milestone { get; private set; }
        public BattleConfig Config { get; private set; }

        private readonly Character heroPrefab;
        private readonly Character enemyPrefab;
        private readonly Transform parent;

        private readonly List<ICharacterData> heroInfos = new();

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
            Data.BattleAsset battleAsset,
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
            Data.BattleAsset battleAsset,
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

                if (Heroes[i].CharacterData?.Id != hero.id)
                {
                    var info = GetOrCreateHero(hero.id);
                    Heroes[i].Init(info, Battlefield.HeroPosition(i, partyCount), i);

                    Heroes[i].SetState(Character.AnimState.Idle);
                }
            }
        }

        private ICharacterData GetOrCreateHero(string heroId)
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
                    enemy.asset.level,
                    Battlefield.enemyStandDirection,
                    BattleAsset.EnemySuffix(i));

                instance.Init(enemyInfo, pos, enemy.positionIndex);

                instance.SetState(Character.AnimState.Idle);
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

        public IEnumerable<Character> GetAlliesOf(Character actor, FetchCondition condition)
        {
            if (actor.CharacterSide == CharacterSide.Heroes)
                return GetHeroes(condition);
            else
                return GetEnemies(condition);
        }

        public IEnumerable<Character> GetEnemiesOf(Character actor, FetchCondition condition)
        {
            if (actor.CharacterSide == CharacterSide.Heroes)
                return GetEnemies(condition);
            else
                return GetHeroes(condition);
        }
    }
}