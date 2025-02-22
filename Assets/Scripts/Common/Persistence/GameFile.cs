using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using System.IO;

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

        public Hero(Data.Characters.Hero data, int hp, int exp)
        {
            id = data.id;
            this.hp = hp;
            this.exp = exp;
            isActive = true;
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
        }

        public void FromJson(JToken json)
        {
            area = json["area"].ToString();
            x = json["x"].ToObject<float>();
            y = json["y"].ToObject<float>();
            z = json["z"].ToObject<float>();
        }

        public JToken ToJson()
        {
            var json = new JObject
            {
                { "area", area },
                { "x", x },
                { "y", y },
                { "z", z }
            };

            return json;
        }
    }

    [Serializable]
    public class Progression : IJsonObject, IClonable
    {
        public int act;
        public int chapter;
        public int milestone;

        public Progression() { }

        public Progression(Progression source)
        {
            Clone(source);
        }

        public Progression(JToken json)
        {
            FromJson(json);
        }

        public void Clone(object source)
        {
            Clone(source as Progression);
        }

        public void Clone(Progression source)
        {
            act = source.act;
            chapter = source.chapter;
            milestone = source.milestone;
        }

        public void FromJson(JToken json)
        {
            act = json["act"].ToObject<int>();
            chapter = json["chapter"].ToObject<int>();
            milestone = json["milestone"].ToObject<int>();
        }

        public JToken ToJson()
        {
            var json = new JObject
            {
                { "act", act },
                { "chapter", chapter },
                { "milestone", milestone }
            };

            return json;
        }
    }

    public class GameFile : IJsonObject, IClonable
    {
        public double playTime;

        public Location location;
        public List<Hero> party = new();

        public string currentStory;
        public Dictionary<string, Progression> stories = new();

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
            currentStory = source.currentStory;
            stories = source.stories.Clone();
        }

        public void FromJson(JToken json)
        {
            playTime = json["playTime"] != null ? json["playTime"].ToObject<double>() : 0;
            location = json["location"].Parse<Location>();
            party = json["party"].ParseList<Hero>();
            currentStory = json["currentStory"].ToString();
            stories = json["stories"].ParseDict<Progression>();
        }

        public JToken ToJson()
        {
            var json = new JObject
            {
                { "playTime", playTime },
                { "location", location.ToJson() },
                { "party", party.ToJson() },
                { "currentStory", currentStory },
                { "stories", stories.ToJson() }
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
