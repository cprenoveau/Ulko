using Newtonsoft.Json.Linq;
using System;

namespace Ulko.Data
{
    public enum Stat
    {
        Fortitude,
        Wisdom,
        Intuition,
        Intelligence
    }

    [Serializable]
    public class Level : IJsonObject, IClonable
    {
        public int level;
        public int exp;
        public float fortitude;
        public float wisdom;
        public float intuition;
        public float intelligence;

        public void Clone(object source)
        {
            Clone(source as Level);
        }

        public void Clone(Level source)
        {
            level = source.level;
            exp = source.exp;
            fortitude = source.fortitude;
            wisdom = source.wisdom;
            intuition = source.intuition;
            intelligence = source.intelligence;
        }

        public float GetStat(Stat stat)
        {
            switch (stat)
            {
                case Stat.Fortitude: return fortitude;
                case Stat.Wisdom: return wisdom;
                case Stat.Intuition: return intuition;
                case Stat.Intelligence: return intelligence;
            }

            return 0;
        }

        public void SetStat(Stat stat, float value)
        {
            switch (stat)
            {
                case Stat.Fortitude: fortitude = value; break;
                case Stat.Wisdom: wisdom = value; break;
                case Stat.Intuition: intuition = value; break;
                case Stat.Intelligence: intelligence = value; break;
            }
        }

        public Level() { }

        public Level(JToken json)
        {
            FromJson(json);
        }

        public void FromJson(JToken json)
        {
            level = json["level"].ToObject<int>();
            exp = json["exp"].ToObject<int>();
            fortitude = json["fortitude"].ToObject<float>();
            wisdom = json["wisdom"].ToObject<float>();
            intuition = json["intuition"].ToObject<float>();
            intelligence = json["intelligence"].ToObject<float>();
        }

        public JToken ToJson()
        {
            var json = new JObject
            {
                { "level", level },
                { "exp", exp },
                { "fortitude", fortitude },
                { "wisdom", wisdom },
                { "intuition", intuition },
                { "intelligence", intelligence }
            };

            return json;
        }
    }
}
