using System;
using System.Collections.Generic;
using Ulko.Data;
using Ulko.Persistence;
using UnityEngine;

namespace Ulko
{
    public static partial class PlayerProfile
    {
        public static event Action OnPartyChanged;
        public static event Action<Hero> OnHeroChanged;

        public static IEnumerable<Hero> Party => loadedGame.party;
        public static IEnumerable<Hero> ActiveParty => loadedGame.party.FindAll(h => h.isActive);

        public static void UpdatePartyMember(Hero hero)
        {
            CreateOrGetHero(hero.id);

            int heroIndex = loadedGame.party.FindIndex(h => h.id == hero.id);
            loadedGame.party[heroIndex] = hero.Clone();

            OnHeroChanged?.Invoke(loadedGame.party[heroIndex]);
        }

        public static bool HeroIsActive(string heroId)
        {
            var hero = GetPartyMember(heroId);
            return hero != null && hero.isActive;
        }

        public static Hero GetPartyMember(string heroId)
        {
            return loadedGame.party.Find(h => h.id == heroId);
        }

        public static Hero GetPartyMember(int index)
        {
            if (index < 0 || index >= loadedGame.party.Count)
                return null;

            return loadedGame.party[index];
        }

        public static int GetPartyIndex(string heroId)
        {
            return loadedGame.party.FindIndex(h => h.id == heroId);
        }

        public static Data.Characters.HeroAsset GetNextPartyMember(string heroId, bool activeOnly)
        {
            int index = GetPartyIndex(heroId);
            if (index == -1) return null;

            int nextIndex = index;
            for (int i = 0; i < loadedGame.party.Count; ++i)
            {
                nextIndex = (nextIndex + 1) % loadedGame.party.Count;

                if (!activeOnly || GetPartyMember(nextIndex).isActive)
                {
                    var hero = FindHeroAsset(GetPartyMember(nextIndex).id);
                    if (hero != null)
                        return hero;
                }
            }

            return FindHeroAsset(heroId);
        }

        public static Data.Characters.HeroAsset GetPreviousPartyMember(string heroId, bool activeOnly)
        {
            int index = GetPartyIndex(heroId);
            if (index == -1) return null;

            int previousIndex = index;
            for (int i = 0; i < loadedGame.party.Count; ++i)
            {
                previousIndex = previousIndex > 0 ? previousIndex - 1 : loadedGame.party.Count - 1;

                if (!activeOnly || GetPartyMember(previousIndex).isActive)
                {
                    var hero = FindHeroAsset(GetPartyMember(previousIndex).id);
                    if (hero != null)
                        return hero;
                }
            }

            return FindHeroAsset(heroId);
        }

        public static void AddPartyMember(string heroId)
        {
            var hero = CreateOrGetHero(heroId);
            hero.isActive = true;

            OnPartyChanged?.Invoke();
        }

        public static void RemovePartyMember(string heroId)
        {
            var hero = GetPartyMember(heroId);
            if (hero != null) hero.isActive = false;

            OnPartyChanged?.Invoke();
        }

        public static void SwapPartyMember(string oldHeroId, string newHeroId)
        {
            int oldHeroIndex = loadedGame.party.FindIndex(h => h.id == oldHeroId);
            if (oldHeroIndex == -1)
            {
                Debug.LogWarning("SwapPartyMember: hero with id " + oldHeroId + " is not in party");
                return;
            }

            var newHero = CreateOrGetHero(newHeroId);
            int newHeroIndex = loadedGame.party.FindIndex(h => h.id == newHeroId);

            loadedGame.party[newHeroIndex] = loadedGame.party[oldHeroIndex];
            loadedGame.party[oldHeroIndex] = newHero;

            OnPartyChanged?.Invoke();
        }

        public static void SetParty(List<Data.Characters.HeroAsset> party, bool setPartyOrder)
        {
            for (int i = 0; i < loadedGame.party.Count; ++i)
            {
                var member = loadedGame.party[i];
                member.isActive = party.Find(m => m.id == member.id) != null;
            }

            foreach (var member in party)
            {
                var hero = CreateOrGetHero(member.id);
                hero.isActive = true;
            }

            if (setPartyOrder)
            {
                var oldParty = new List<Hero>();
                oldParty.AddRange(loadedGame.party);

                loadedGame.party.Clear();
                foreach (var member in party)
                {
                    int index = oldParty.FindIndex(p => p.id == member.id);
                    loadedGame.party.Add(oldParty[index]);
                    oldParty.RemoveAt(index);
                }

                loadedGame.party.AddRange(oldParty);
            }
        }

        private static Hero CreateOrGetHero(string heroId)
        {
            var hero = GetPartyMember(heroId);
            if (hero != null) return hero;

            var data = Database.Heroes[heroId];
            hero = new Hero(data, GetHeroMaxHP(heroId), GetHeroExp(heroId), false);
            loadedGame.party.Add(hero);

            return hero;
        }

        public static void ReviveParty()
        {
            foreach (var hero in Party)
            {
                if (hero.hp == 0)
                    hero.hp = 1;
            }
        }

        public static bool HealParty()
        {
            bool healed = false;
            foreach (var hero in Party)
            {
                if (HealHero(hero.id))
                    healed = true;
            }
            return healed;
        }

        public static bool HealHero(string heroId)
        {
            var hero = CreateOrGetHero(heroId);
            int maxHp = GetHeroMaxHP(heroId);

            if (hero.hp < maxHp)
            {
                hero.hp = maxHp;
                return true;
            }

            return false;
        }

        public static void AddHeroExp(string heroId, int exp)
        {
            int oldMaxHp = GetHeroMaxHP(heroId);

            var hero = CreateOrGetHero(heroId);
            hero.exp += exp;

            int newMaxHp = GetHeroMaxHP(heroId);
            hero.hp += newMaxHp - oldMaxHp;
        }

        public static void SetHeroLevel(string heroId, int level)
        {
            int oldMaxHp = GetHeroMaxHP(heroId);

            var heroData = Database.Heroes[heroId];
            var hero = CreateOrGetHero(heroId);

            hero.exp = heroData.GetLevelData(level).Exp;

            int newMaxHp = GetHeroMaxHP(heroId);
            hero.hp += newMaxHp - oldMaxHp;
        }

        public static int GetHeroExp(string heroId)
        {
            var heroData = Database.Heroes[heroId];
            var hero = GetPartyMember(heroId);

            return hero != null ? hero.exp : heroData.GetLevelData(heroData.minLevel).Exp;
        }

        public static int GetHeroLevel(string heroId)
        {
            var heroData = Database.Heroes[heroId];
            var hero = GetPartyMember(heroId);

            return hero != null ? heroData.GetLevelDataFromExp(hero.exp).Lvl : heroData.minLevel;
        }

        public static int GetHeroMaxHP(string heroId)
        {
            return GetHeroMaxHP(heroId, GetHeroLevel(heroId));
        }

        public static int GetHeroMaxHP(string heroId, int level)
        {
            return (int)GetHeroStats(heroId, level).GetStat(Stat.MaxHP);
        }

        public static Level GetHeroStats(string heroId, int level)
        {
            var heroData = Database.Heroes[heroId];
            return heroData.GetLevelData(level).Clone();
        }

        public static Level GetHeroStats(string heroId)
        {
            return GetHeroStats(heroId, GetHeroLevel(heroId));
        }

        public static int GetHeroStat(string heroId, int level, Stat stat)
        {
            return (int)GetHeroStats(heroId, level).GetStat(stat);
        }

        public static int GetHeroStat(string heroId, Stat stat)
        {
            return GetHeroStat(heroId, GetHeroLevel(heroId), stat);
        }

        public static Data.Characters.HeroAsset FindHeroAsset(string heroId)
        {
            var milestone = GetCurrentMilestone();

            var heroRef = milestone.Party.Find(x => x.id == heroId);
            if (heroRef != null)
                return heroRef;

            var chapter = GetCurrentChapter();

            int milestoneIndex = chapter.milestones.IndexOf(milestone);
            for (int i = milestoneIndex; i >= 0; --i)
            {
                heroRef = chapter.milestones[i].Party.Find(x => x.id == heroId);
                if (heroRef != null)
                    return heroRef;
            }

            return heroRef;
        }
    }
}
