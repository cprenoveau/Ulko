using Newtonsoft.Json.Linq;
using System;

namespace Ulko.Data
{
    public enum Stat
    {
        MaxHP,
        Strength,
        Wisdom,
        Intuition,
        Intelligence,
        Shield
    }

    [Serializable]
    public class Level : IJsonObject, IClonable, IEquatable<Level>
    {
        public int level;
        public int exp;
        public float maxHP;
        public float strength;
        public float wisdom;
        public float intuition;
        public float intelligence;
        public float shield;

        public static Level operator +(Level left, Level right)
        {
            return new Level()
            {
                level = left.level + right.level,
                exp = left.exp + right.exp,
                maxHP = left.maxHP + right.maxHP,
                strength = left.strength + right.strength,
                wisdom = left.wisdom + right.wisdom,
                intuition = left.intuition + right.intuition,
                intelligence = left.intelligence + right.intelligence,
                shield = left.shield + right.shield
            };
        }

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
            shield = source.shield;
        }

        public float GetStat(Stat stat)
        {
            switch (stat)
            {
                case Stat.MaxHP: return maxHP;
                case Stat.Strength: return strength;
                case Stat.Wisdom: return wisdom;
                case Stat.Intuition: return intuition;
                case Stat.Intelligence: return intelligence;
                case Stat.Shield: return shield;
            }

            return 0;
        }

        public void SetStat(Stat stat, float value)
        {
            switch (stat)
            {
                case Stat.MaxHP: maxHP = value; break;
                case Stat.Strength: strength = value; break;
                case Stat.Wisdom: wisdom = value; break;
                case Stat.Intuition: intuition = value; break;
                case Stat.Intelligence: intelligence = value; break;
                case Stat.Shield: shield = value; break;
            }
        }

        public void AddToStat(Stat stat, float value)
        {
            SetStat(stat, GetStat(stat) + value);
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
            shield = json["shield"] != null ? json["shield"].ToObject<float>() : 0;
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
                { "intelligence", intelligence },
                { "shield", shield }
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
                && intelligence.Equals(other.intelligence)
                && shield.Equals(other.shield);
        }
    }
}
