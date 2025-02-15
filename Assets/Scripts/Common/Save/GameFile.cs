using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Ulko.Save
{
    //needs to be a class to allow parameterless constructor for IClonable. language limitation
    public class Hero : IJsonObject, IClonable
    {
        public string id;
        public int hp;
        public int mp;
        public int exp;
        public bool isActive;

        public Hero() { }

        public Hero(string id, int hp, int mp, int exp)
        {
            this.id = id;
            this.hp = hp;
            this.mp = mp;
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
            mp = source.mp;
            exp = source.exp;
            isActive = source.isActive;
        }

        public void FromJson(JToken json)
        {
            id = json["id"].ToString();
            hp = json["hp"] != null ? json["hp"].ToObject<int>() : 0;
            mp = json["mp"] != null ? json["mp"].ToObject<int>() : 0;
            exp = json["exp"] != null ? json["exp"].ToObject<int>() : 0;
            isActive = json["isActive"] != null ? json["isActive"].ToObject<bool>() : false;
        }

        public JToken ToJson()
        {
            var json = new JObject
            {
                { "id", id },
                { "hp", hp },
                { "mp", mp },
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

    public class Item : IJsonObject, IClonable
    {
        public string id;
        public int count;

        public Item() { }

        public Item(string id, int count)
        {
            this.id = id;
            this.count = count;
        }

        public Item(Item source)
        {
            Clone(source);
        }

        public Item(JToken json)
        {
            FromJson(json);
        }

        public void Clone(object source)
        {
            Clone(source as Item);
        }

        public void Clone(Item source)
        {
            id = source.id;
            count = source.count;
        }

        public void FromJson(JToken json)
        {
            id = json["id"].ToString();
            count = json["count"].ToObject<int>();
        }

        public JToken ToJson()
        {
            var json = new JObject
            {
                { "id", id },
                { "count", count }
            };

            return json;
        }
    }

    public class Chest : IJsonObject, IClonable
    {
        public string id;
        public List<Item> collectedItems;

        public Chest() { }

        public Chest(string id, List<Item> collectedItems)
        {
            this.id = id;
            this.collectedItems = collectedItems;
        }

        public Chest(Chest source)
        {
            Clone(source);
        }

        public Chest(JToken json)
        {
            FromJson(json);
        }

        public void Clone(object source)
        {
            Clone(source as Chest);
        }

        public void Clone(Chest source)
        {
            id = source.id;
            collectedItems = source.collectedItems.Clone();
        }

        public void FromJson(JToken json)
        {
            id = json["id"].ToString();
            collectedItems = json["collectedItems"].ParseList<Item>();
        }

        public JToken ToJson()
        {
            var json = new JObject
            {
                { "id", id },
                { "collectedItems", collectedItems.ToJson() }
            };

            return json;
        }
    }

    public class Game : IJsonObject, IClonable
    {
        public double playTime;
        public int money;

        public Location location;
        public List<Hero> party = new();

        public string currentStory;
        public Dictionary<string, Progression> stories = new();

        public List<Item> inventory = new();
        public List<Chest> collectedChests = new();

        public Game() { }

        public Game(Game source)
        {
            Clone(source);
        }

        public Game(JToken json)
        {
            FromJson(json);
        }

        public void Clone(object source)
        {
            Clone(source as Game);
        }

        public void Clone(Game source)
        {
            playTime = source.playTime;
            money = source.money;
            location = source.location.Clone();
            party = source.party.Clone();
            currentStory = source.currentStory;
            stories = source.stories.Clone();
            inventory = source.inventory.Clone();
            collectedChests = source.collectedChests.Clone();
        }

        public void FromJson(JToken json)
        {
            playTime = json["playTime"] != null ? json["playTime"].ToObject<double>() : 0;
            money = json["money"].ToObject<int>();
            location = json["location"].Parse<Location>();
            party = json["party"].ParseList<Hero>();
            currentStory = json["currentStory"].ToString();
            stories = json["stories"].ParseDict<Progression>();
            inventory = json["inventory"].ParseList<Item>();
            collectedChests = json["collectedChests"].ParseList<Chest>();
        }

        public JToken ToJson()
        {
            var json = new JObject
            {
                { "playTime", playTime },
                { "money", money },
                { "location", location.ToJson() },
                { "party", party.ToJson() },
                { "currentStory", currentStory },
                { "stories", stories.ToJson() },
                { "inventory", inventory.ToJson() },
                { "collectedChests", collectedChests.ToJson() }
            };

            return json;
        }

        public const string GAMES_FOLDER = "games";
        public const string GAME_FILE_EXTENSION = ".ulko";

        public static string GameSavePath => Path.Combine(Application.persistentDataPath, GAMES_FOLDER);
        public static string GameFileName(string name) => name + GAME_FILE_EXTENSION;

        public static bool Save(Game game, string directory, string filename)
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

        public static bool Load(string directory, string filename, out Game game)
        {
            try
            {
                if (File.Exists(Path.Combine(directory, filename)))
                {
                    var json = File.ReadAllText(Path.Combine(directory, filename));
                    game = new Game(JObject.Parse(json));
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
