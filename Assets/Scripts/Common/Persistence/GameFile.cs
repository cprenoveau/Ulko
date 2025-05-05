using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using System.IO;
using Ulko.Data;
using Ulko.Data.Timeline;

namespace Ulko.Persistence
{
    //cannot be a struct because IClonable requires a parameterless constructor. language restriction
    public class Hero : IJsonObject, IClonable
    {
        public string id;
        public int hp;
        public int exp;
        public bool isActive;

        public Hero() { }

        public Hero(Data.Characters.Hero data, int hp, int exp, bool isActive)
        {
            id = data.id;
            this.hp = hp;
            this.exp = exp;
            this.isActive = isActive;
        }

        public Hero(Hero source)
        {
            Clone(source);
        }

        public Hero(JToken json)
        {
            FromJson(json);
        }

        public void Clone(object source)
        {
            Clone(source as Hero);
        }

        public void Clone(Hero source)
        {
            id = source.id;
            hp = source.hp;
            exp = source.exp;
            isActive = source.isActive;
        }

        public void FromJson(JToken json)
        {
            id = json["id"].ToString();
            hp = json["hp"] != null ? json["hp"].ToObject<int>() : 0;
            exp = json["exp"] != null ? json["exp"].ToObject<int>() : 0;
            isActive = json["isActive"] != null ? json["isActive"].ToObject<bool>() : false;
        }

        public JToken ToJson()
        {
            var json = new JObject
            {
                { "id", id },
                { "hp", hp },
                { "exp", exp },
                { "isActive", isActive }
            };

            return json;
        }
    }

    public class Location : IJsonObject, IClonable
    {
        public string area;
        public float x;
        public float y;
        public float z;
        public float standX;
        public float standY;
        public int encounterIndex = -1;

        public Location() { }

        public Location(Location source)
        {
            Clone(source);
        }

        public Location(JToken json)
        {
            FromJson(json);
        }

        public void Clone(object source)
        {
            Clone(source as Location);
        }

        public void Clone(Location source)
        {
            area = source.area;
            x = source.x;
            y = source.y;
            z = source.z;
            standX = source.standX;
            standY = source.standY;
            encounterIndex = source.encounterIndex;
        }

        public void FromJson(JToken json)
        {
            area = json["area"].ToString();
            x = json["x"].ToObject<float>();
            y = json["y"].ToObject<float>();
            z = json["z"].ToObject<float>();
            standX = json["standX"].ToObject<float>();
            standY = json["standY"].ToObject<float>();
            encounterIndex = json["encounterIndex"].ToObject<int>();
        }

        public JToken ToJson()
        {
            var json = new JObject
            {
                { "area", area },
                { "x", x },
                { "y", y },
                { "z", z },
                { "standX", standX },
                { "standY", standY },
                { "encounterIndex", encounterIndex }
            };

            return json;
        }
    }

    [Serializable]
    public class ChapterProgression : IJsonObject, IClonable
    {
        public string chapterId;
        public bool fullyCompleted;
        public List<string> completedMilestones = new();

        public bool ChapterIsCompleted()
        {
            return fullyCompleted;
        }

        public bool ChapterIsInProgress()
        {
            return completedMilestones.Count > 0;
        }

        public bool MilestoneIsCompleted(string milestoneName)
        {
            return completedMilestones.Contains(milestoneName);
        }

        public ChapterProgression() { }

        public ChapterProgression(ChapterProgression source)
        {
            Clone(source);
        }

        public ChapterProgression(JToken json)
        {
            FromJson(json);
        }

        public void Clone(object source)
        {
            Clone(source as ChapterProgression);
        }

        public void Clone(ChapterProgression source)
        {
            chapterId = source.chapterId;
            fullyCompleted = source.fullyCompleted;
            completedMilestones = source.completedMilestones.Clone();
        }

        public void FromJson(JToken json)
        {
            chapterId = json["chapterId"].ToString();
            fullyCompleted = json["fullyCompleted"].ToObject<bool>();
            completedMilestones = json["completedMilestones"].ParseStringList();
        }

        public JToken ToJson()
        {
            var json = new JObject
            {
                { "chapterId", chapterId },
                { "fullyCompleted", fullyCompleted },
                { "completedMilestones", completedMilestones.ToJson() }
            };

            return json;
        }
    }

    public class GameFile : IJsonObject, IClonable
    {
        public double playTime;

        public Location location;
        public List<Hero> party = new();

        public List<Card<AbilityCardData>> reserveDeck = new();

        public string currentStoryId;
        public Dictionary<string, ChapterProgression> chapterProgression = new();

        public ChapterProgression GetChapterProgression(string chapterId)
        {
            if (chapterProgression.ContainsKey(chapterId))
                return chapterProgression[chapterId];
            else
                return new ChapterProgression() { chapterId = chapterId };
        }

        public void ResetChapterProgression(string chapterId)
        {
            chapterProgression.Remove(chapterId);
        }

        public bool ChapterIsCompleted(string chapterId)
        {
            return GetChapterProgression(chapterId).ChapterIsCompleted();
        }

        public void CompleteMilestone(string chapterId, string milestoneName)
        {
            if (!chapterProgression.ContainsKey(chapterId))
            {
                chapterProgression.Add(chapterId, new ChapterProgression() { chapterId = chapterId });
            }

            if (!chapterProgression[chapterId].MilestoneIsCompleted(milestoneName))
            {
                chapterProgression[chapterId].completedMilestones.Add(milestoneName);
            }
        }

        public void CompleteChapter(string chapterId)
        {
            if (!chapterProgression.ContainsKey(chapterId))
            {
                chapterProgression.Add(chapterId, new ChapterProgression() { chapterId = chapterId });
            }

            chapterProgression[chapterId].fullyCompleted = true;
        }

        public GameFile() { }

        public GameFile(GameFile source)
        {
            Clone(source);
        }

        public GameFile(JToken json)
        {
            FromJson(json);
        }

        public void Clone(object source)
        {
            Clone(source as GameFile);
        }

        public void Clone(GameFile source)
        {
            playTime = source.playTime;
            location = source.location.Clone();
            party = source.party.Clone();
            reserveDeck = source.reserveDeck.Clone();
            currentStoryId = source.currentStoryId;
            chapterProgression = source.chapterProgression.Clone();
        }

        public void FromJson(JToken json)
        {
            playTime = json["playTime"] != null ? json["playTime"].ToObject<double>() : 0;
            location = json["location"].Parse<Location>();
            party = json["party"].ParseList<Hero>();
            reserveDeck = json["reserveDeck"].ParseList<Card<AbilityCardData>>();
            currentStoryId = json["currentStoryId"].ToString();
            chapterProgression = json["chapterProgression"].ParseDict<ChapterProgression>();
        }

        public JToken ToJson()
        {
            var json = new JObject
            {
                { "playTime", playTime },
                { "location", location.ToJson() },
                { "party", party.ToJson() },
                { "reserveDeck", reserveDeck.ToJson() },
                { "currentStoryId", currentStoryId },
                { "chapterProgression", chapterProgression.ToJson() }
            };

            return json;
        }

        public const string GAMES_FOLDER = "games";
        public const string GAME_FILE_EXTENSION = ".dd";

        public static string GameSavePath => Path.Combine(Application.persistentDataPath, GAMES_FOLDER);
        public static string GameFileName(string name) => name + GAME_FILE_EXTENSION;

        public static bool Save(GameFile game, string directory, string filename)
        {
            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(Path.Combine(directory, filename), game.ToJson().ToString());
            }
            catch (Exception e)
            {
                Debug.LogError("Could not save file " + filename + " to directory " + directory + " " + e.Message);
                return false;
            }

            return true;
        }

        public static bool Load(string directory, string filename, out GameFile game)
        {
            try
            {
                if (File.Exists(Path.Combine(directory, filename)))
                {
                    var json = File.ReadAllText(Path.Combine(directory, filename));
                    game = new GameFile(JObject.Parse(json));
                }
                else
                {
                    game = null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Could not load file " + filename + " from directory " + directory + " " + e.Message);
                game = null;
                return false;
            }

            return true;
        }

        public static bool Delete(string directory, string filename)
        {
            try
            {
                if (File.Exists(Path.Combine(directory, filename)))
                {
                    File.Delete(Path.Combine(directory, filename));
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Could not delete file " + filename + " from directory " + directory + " " + e.Message);
                return false;
            }

            return true;
        }
    }
}
