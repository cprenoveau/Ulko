using Ulko.Data;
using Ulko.Data.Abilities;
using Ulko.Data.Inventory;
using Ulko.Persistence;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Ulko
{
    public static partial class PlayerProfile
    {
        public static IEnumerable<(FileInfo file, GameFile game)> AllFiles { get; private set; }
        public static string LoadedFile { get; private set; }
        public static bool GameIsLoaded => loadedGame != null;

        public static event Action OnPartyChanged;
        public static event Action<Hero> OnHeroChanged;

        private static GameFile loadedGame;
        private static GameFile gameCopy;
        private static TextAsset newGameAsset;

        public static void Init(TextAsset newGameAsset)
        {
            PlayerProfile.newGameAsset = newGameAsset;
            RefreshFiles();
        }

        public static void SaveState()
        {
            gameCopy = loadedGame.Clone();
        }

        public static void RestoreState()
        {
            loadedGame = gameCopy.Clone();
        }

        public static IEnumerable<Hero> Party => loadedGame.party;
        public static IEnumerable<Hero> ActiveParty => loadedGame.party.FindAll(h => h.isActive);

        public static void UpdatePartyMember(Hero hero)
        {
            CreateOrGetHero(hero.id);

            int heroIndex = loadedGame.party.FindIndex(h => h.id == hero.id);
            loadedGame.party[heroIndex] = hero.Clone();

            OnHeroChanged?.Invoke(loadedGame.party[heroIndex]);
        }

        public static Hero GetPartyMember(string heroId)
        {
            return loadedGame?.party.Find(h => h.id == heroId);
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

        public static Data.Characters.HeroAsset GetNextPartyMember(string heroId)
        {
            int index = GetPartyIndex(heroId);
            if (index == -1) return null;

            int nextIndex = index;
            for(int i = 0; i < loadedGame.party.Count; ++i)
            {
                nextIndex = (nextIndex + 1) % loadedGame.party.Count;

                var hero = FindHero(CurrentStory, GetProgression(), GetPartyMember(nextIndex).id);
                if (hero != null)
                    return hero;
            }

            return FindHero(CurrentStory, GetProgression(), heroId);
        }

        public static Data.Characters.HeroAsset GetPreviousPartyMember(string heroId)
        {
            int index = GetPartyIndex(heroId);
            if (index == -1) return null;

            int previousIndex = index;
            for (int i = 0; i < loadedGame.party.Count; ++i)
            {
                previousIndex = previousIndex > 0 ? previousIndex - 1 : loadedGame.party.Count - 1;

                var hero = FindHero(CurrentStory, GetProgression(), GetPartyMember(previousIndex).id);
                if (hero != null)
                    return hero;
            }

            return FindHero(CurrentStory, GetProgression(), heroId);
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
            for(int i = 0; i < loadedGame.party.Count;)
            {
                var member = loadedGame.party[i];

                member.isActive = party.Find(m => m.id == member.id) != null;
                if(!member.isActive && Database.Heroes[member.id].isGuest)
                {
                    loadedGame.party.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }

            foreach(var member in party)
            {
                var hero = CreateOrGetHero(member.id);
                hero.isActive = true;

                if (member.overrideWeapon != null)
                    hero.equippedWeapon = member.overrideWeapon.id;

                if (member.overrideArmor.Count > 0)
                    hero.equippedArmor = member.overrideArmor.Select(a => a.id).ToList();
            }

            if(setPartyOrder)
            {
                var oldParty = new List<Hero>();
                oldParty.AddRange(loadedGame.party);

                loadedGame.party.Clear();
                foreach(var member in party)
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
            hero = new Hero(data, GetHeroStat(heroId, Stat.MaxHP), GetHeroStat(heroId, Stat.MaxMP), GetHeroExp(heroId));
            loadedGame.party.Add(hero);

            return hero;
        }

        public static bool HealParty(int cost)
        {
            if (Money < cost)
                return false;

            bool healed = false;
            foreach (var hero in Party)
            {
                int maxHp = GetHeroStat(hero.id, Stat.MaxHP);
                int maxMp = GetHeroStat(hero.id, Stat.MaxMP);

                if (hero.hp < maxHp)
                {
                    hero.hp = maxHp;
                    healed = true;
                }

                if (hero.mp < maxMp)
                {
                    hero.mp = maxMp;
                    healed = true;
                }
            }

            if (healed) AddMoney(-cost);
            return healed;
        }

        public static void AddHeroExp(string heroId, int exp)
        {
            var hero = CreateOrGetHero(heroId);
            hero.exp += exp;
        }

        public static int GetHeroExp(string heroId)
        {
            var heroData = Database.Heroes[heroId];
            var hero = GetPartyMember(heroId);

            return hero != null ? hero.exp : heroData.GetLevelData(heroData.minLevel).exp;
        }

        public static int GetHeroLevel(string heroId)
        {
            var heroData = Database.Heroes[heroId];
            var hero = GetPartyMember(heroId);

            return hero != null ? heroData.GetLevelDataFromExp(hero.exp).level : heroData.minLevel;
        }

        public static int GetHeroStat(string heroId, Stat stat)
        {
            return GetHeroStat(heroId, GetHeroLevel(heroId), stat);
        }

        public static int GetHeroStat(string heroId, int level, Stat stat)
        {
            return GetHeroStat(heroId, level, stat, GetWeaponData(heroId), GetArmorData(heroId), GetModData(heroId));
        }

        public static int GetHeroStat(string heroId, Stat stat, Weapon weapon, IEnumerable<Armor> armors, IEnumerable<Mod> mods)
        {
            return GetHeroStat(heroId, GetHeroLevel(heroId), stat, weapon, armors, mods);
        }

        private static int GetHeroStat(string heroId, int level, Stat stat, Weapon weapon, IEnumerable<Armor> armors, IEnumerable<Mod> mods)
        {
            var heroData = Database.Heroes[heroId];
            var heroStat = heroData.GetLevelData(level).GetStat(stat);

            if(weapon != null)
                heroStat += weapon.GetStat(stat);

            foreach (var armor in armors)
            {
                heroStat += armor.GetStat(stat);
            }

            foreach(var mod in mods)
            {
                heroStat *= 1f + mod.GetStat(stat) / 100f;
            }

            return Mathf.CeilToInt(heroStat);
        }

        private static void InsertUnique<T>(ref List<T> a, List<T> b)
        {
            foreach(var element in b)
            {
                if (!a.Contains(element))
                    a.Add(element);
            }
        }

        public static List<Ability> GetAllAbilities(Data.Characters.HeroAsset hero)
        {
            var abilities = new List<Ability>();

            abilities.AddRange(hero.GetAbilities(GetHeroLevel(hero.id)).Select(a => a.ability));
            InsertUnique(ref abilities, GetModAbilities(hero.id));

            return abilities;
        }

        public static List<Status> GetAllStatuses(Data.Characters.HeroAsset hero)
        {
            var statuses = new List<Status>();

            statuses.AddRange(hero.GetStatuses(GetHeroLevel(hero.id)).Select(a => a.status));
            InsertUnique(ref statuses, GetWeaponStatuses(hero.id));
            InsertUnique(ref statuses, GetArmorStatuses(hero.id));
            InsertUnique(ref statuses, GetModStatuses(hero.id));

            return statuses;
        }

        public static List<Status> GetWeaponStatuses(string heroId)
        {
            var statuses = new List<Status>();
            var weapon = GetWeaponData(heroId);

            if(weapon != null)
                statuses.AddRange(weapon.giveStatus);

            return statuses;
        }

        public static List<Status> GetArmorStatuses(string heroId)
        {
            var statuses = new List<Status>();
            var armors = GetArmorData(heroId);
            foreach(var armor in armors)
            {
                statuses.AddRange(armor.giveStatus);
            }
            return statuses;
        }

        public static List<Ability> GetModAbilities(string heroId)
        {
            var abilities = new List<Ability>();

            var mods = GetModData(heroId);

            foreach(var mod in mods)
            {
                InsertUnique(ref abilities, mod.giveAbilities);
            }

            return abilities;
        }

        public static List<Status> GetModStatuses(string heroId)
        {
            var statuses = new List<Status>();

            var mods = GetModData(heroId);
            foreach (var mod in mods)
            {
                foreach (var status in mod.giveStatus)
                {
                    if (statuses.Find(a => a.id == status.id) == null)
                    {
                        statuses.Add(status);
                    }
                }
            }

            return statuses;
        }

        public static bool UseAbility(string heroId, Ability ability)
        {
            var hero = GetPartyMember(heroId);
            if(hero != null && hero.mp >= ability.mpCost)
            {
                hero.mp -= ability.mpCost;
                return true;
            }

            return false;
        }

        public static int Money => loadedGame.money;
        public static void AddMoney(int amount)
        {
            loadedGame.money += amount;
            if (loadedGame.money < 0) loadedGame.money = 0;
        }

        public static double Time => loadedGame.playTime;
        public static void AddTime(double amount)
        {
            loadedGame.playTime += amount;
        }

        public static Location CurrentLocation => loadedGame.location.Clone();

        public static void SetPosition(Vector3 pos)
        {
            loadedGame.location.x = pos.x;
            loadedGame.location.y = pos.y;
            loadedGame.location.z = pos.z;
        }

        public static void SetArea(string area)
        {
            loadedGame.location.area = area;
        }

        public static string CurrentStory => loadedGame.currentStory;

        public static Progression GetProgression()
        {
            return GetProgression(loadedGame.currentStory);
        }

        public static Progression GetProgression(string story)
        {
            if (!loadedGame.stories.ContainsKey(story))
                return new Progression();
            else
                return loadedGame.stories[story].Clone();
        }

        public static void SetProgression(string story, Progression progression)
        {
            if (!loadedGame.stories.ContainsKey(story))
                loadedGame.stories.Add(story, progression.Clone());
            else
                loadedGame.stories[story] = progression.Clone();
        }

        public static void SetMilestone(string milestoneName)
        {
            var prog = Database.GetProgression(milestoneName);
            if(prog.story != null)
            {
                SetProgression(prog.story, prog.prog);
            }
        }

        public static void SetNextMilestone()
        {
            loadedGame.location = new Location();
            SetProgression(loadedGame.currentStory, GetNextMilestone());
        }

        public static Progression GetNextMilestone()
        {
            return GetNextMilestone(loadedGame.currentStory, GetProgression(loadedGame.currentStory));
        }

        public static Progression GetNextMilestone(string story, Progression progression)
        {
            int actIndex = progression.act;
            int chapterIndex = progression.chapter;
            int milestoneIndex = progression.milestone;

            var chapter = Database.GetChapter(story, actIndex, chapterIndex);
            if (milestoneIndex + 1 < chapter.milestones.Count)
            {
                return new Progression { act = actIndex, chapter = chapterIndex, milestone = milestoneIndex + 1 };
            }
            else
            {
                var act = Database.GetAct(loadedGame.currentStory, actIndex);
                if(chapterIndex + 1 < act.chapters.Count)
                {
                    return new Progression { act = actIndex, chapter = chapterIndex + 1, milestone = 0 };
                }
                else if(actIndex + 1 < Database.Stories[loadedGame.currentStory].acts.Count)
                {
                    return new Progression { act = actIndex + 1, chapter = 0, milestone = 0 };
                }
                else
                {
                    return new Progression(); //back to beginning
                }
            }
        }

        public static Progression GetPreviousMilestone(Progression progression)
        {
            int actIndex = progression.act;
            int chapterIndex = progression.chapter;
            int milestoneIndex = progression.milestone;

            if (milestoneIndex - 1 >= 0)
            {
                return new Progression { act = actIndex, chapter = chapterIndex, milestone = milestoneIndex - 1 };
            }
            else
            {
                if (chapterIndex - 1 >= 0)
                {
                    return new Progression { act = actIndex, chapter = chapterIndex - 1, milestone = 0 };
                }
                else if (actIndex - 1 >= 0)
                {
                    return new Progression { act = actIndex - 1, chapter = 0, milestone = 0 };
                }
                else
                {
                    return new Progression();
                }
            }
        }

        public static bool IsFirst(Progression progression)
        {
            return progression.act == 0 && progression.chapter == 0 && progression.milestone == 0;
        }

        public static Data.Characters.HeroAsset FindHero(string story, Progression progression, string heroId)
        {
            var milestone = Database.GetMilestone(story, progression.act, progression.chapter, progression.milestone);

            var heroRef = milestone.Party.Find(x => x.id == heroId);
            var lastProgression = progression;

            while (heroRef == null && !IsFirst(lastProgression))
            {
                lastProgression = GetPreviousMilestone(lastProgression);
                var newMilestone = Database.GetMilestone(story, lastProgression.act, lastProgression.chapter, lastProgression.milestone);
                heroRef = newMilestone.Party.Find(x => x.id == heroId);
            }

            return heroRef;
        }

        public static void NewGame()
        {
            loadedGame = new GameFile(JObject.Parse(newGameAsset.text));
            HealParty(0);

            LoadedFile = null;
        }

        public static bool SaveGame(string filename)
        {
            if(loadedGame != null && GameFile.Save(loadedGame, GameFile.GameSavePath, filename))
            {
                RefreshFiles();
                LoadedFile = filename;
                return true;
            }
            return false;
        }

        public static bool LoadGame(string filename)
        {
            if(GameFile.Load(GameFile.GameSavePath, filename, out GameFile game))
            {
                loadedGame = game;
                LoadedFile = filename;
                return true;
            }

            return false;
        }

        public static bool DeleteGame(string filename)
        {
            if(GameFile.Delete(GameFile.GameSavePath, filename))
            {
                RefreshFiles();
                if (LoadedFile == filename) LoadedFile = null;
                return true;
            }
            return false;
        }

        private static void RefreshFiles()
        {
            var games = new List<(FileInfo file, GameFile game)>();

            var dir = new DirectoryInfo(GameFile.GameSavePath);
            if (dir.Exists)
            {
                FileInfo[] files = dir.GetFiles("*");

                foreach (FileInfo file in files)
                {
                    if(GameFile.Load(GameFile.GameSavePath, file.Name, out GameFile game))
                    {
                        games.Add((file, game));
                    }
                }
            }

            AllFiles = games.OrderByDescending(g => g.file.LastWriteTimeUtc);
        }
    }
}
