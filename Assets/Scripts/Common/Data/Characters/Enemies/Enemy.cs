using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Ulko.Data.Characters
{
    public class Enemy : IJsonObject
    {
        public string id;
        public string name;
        public List<Level> levels = new List<Level>();

        public Enemy() { }

        public Enemy(JToken json)
        {
            FromJson(json);
        }

        public Level GetLevelData(int level)
        {
            if (level > 0 && level <= levels.Count)
            {
                return levels[level - 1];
            }
            return null;
        }

        public void FromJson(JToken json)
        {
            id = json["id"].ToString();
            name = json["name"].ToString();
            levels = json["levels"].ParseList<Level>();
        }

        public JToken ToJson()
        {
            var json = new JObject
            {
                { "id", id },
                { "name", name },
                { "levels", levels.ToJson() }
            };

            return json;
        }
    }
}
