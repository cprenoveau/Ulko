using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Ulko.Data.Characters
{
    public class Hero : IJsonObject
    {
        public string id;
        public int minLevel;
        public List<Level> levels = new();

        public Hero() { }

        public Hero(JToken json)
        {
            FromJson(json);
        }

        public int MaxLevel => levels.Count;
        public bool IsMaxLevel(int level) => level == MaxLevel;

        public Level GetLevelData(int level)
        {
            if(level > 0 && level <= levels.Count)
            {
                return levels[level - 1];
            }
            return null;
        }

        public Level GetLevelDataFromExp(int exp)
        {
            for (int i = 0; i < levels.Count; ++i)
            {
                if (exp == levels[i].Exp)
                {
                    return levels[i];
                }
                else if (exp < levels[i].Exp)
                {
                    return levels[i - 1];
                }
            }

            return null;
        }

        public void FromJson(JToken json)
        {
            id = json["id"].ToString();
            minLevel = json["minLevel"].ToObject<int>();
            levels = json["levels"].ParseList<Level>();
        }

        public JToken ToJson()
        {
            var json = new JObject
            {
                { "id", id },
                { "minLevel", minLevel },
                { "levels", levels.ToJson() }
            };

            return json;
        }
    }
}
