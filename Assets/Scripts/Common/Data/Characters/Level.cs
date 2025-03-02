using Newtonsoft.Json.Linq;
using System;

namespace Ulko.Data
{
    public enum Stat
    {
        Strength,
        Wisdom,
        Intuition,
        Intelligence
    }

    [Serializable]
    public class Level : IJsonObject, IClonable, IEquatable<Level>
    {
        public int level;
        public int exp;
        public int maxHP;
        public float strength;
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
            maxHP = source.maxHP;
            strength = source.strength;
            wisdom = source.wisdom;
            intuition = source.intuition;
            intelligence = source.intelligence;
        }

        public float GetStat(Stat stat)
        {
            switch (stat)
            {
                case Stat.Strength: return strength;
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
                case Stat.Strength: strength = value; break;
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
            maxHP = json["maxHP"].ToObject<int>();
            strength = json["strength"].ToObject<float>();
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
                { "maxHP", maxHP },
                { "strength", strength },
                { "wisdom", wisdom },
                { "intuition", intuition },
                { "intelligence", intelligence }
            };

            return json;
        }

        public bool Equals(Level other)
        {
            return level.Equals(other.level)
                && exp.Equals(other.exp)
                && maxHP.Equals(other.maxHP)
                && strength.Equals(other.strength)
                && wisdom.Equals(other.wisdom)
                && intuition.Equals(other.intuition)
                && intelligence.Equals(other.intelligence);
        }
    }
}
