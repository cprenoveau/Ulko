using Ulko.Data;
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
            for(int i = 0; i < loadedGame.party.Count; ++i)
            {
                var member = loadedGame.party[i];
                member.isActive = party.Find(m => m.id == member.id) != null;
            }

            foreach(var member in party)
            {
                var hero = CreateOrGetHero(member.id);
                hero.isActive = true;
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
            hero = new Hero(data, GetHeroStats(heroId).maxHP, GetHeroExp(heroId));
            loadedGame.party.Add(hero);

            return hero;
        }

        public static bool HealParty()
        {
            bool healed = false;
            foreach (var hero in Party)
            {
                int maxHp = GetHeroStats(hero.id).maxHP;

                if (hero.hp < maxHp)
                {
                    hero.hp = maxHp;
                    healed = true;
                }
            }

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

        public static double Time => loadedGame.playTime;
        public static void AddTime(double amount)
        {
            loadedGame.playTime += amount;
        }

        public static Location CurrentLocation => loadedGame.location.Clone();

        public static void SetPosition(Vector3 pos, Vector2 standDirection)
        {
            loadedGame.location.x = pos.x;
            loadedGame.location.y = pos.y;
            loadedGame.location.z = pos.z;
            loadedGame.location.standX = standDirection.x;
            loadedGame.location.standY = standDirection.y;
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
            HealParty();

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
