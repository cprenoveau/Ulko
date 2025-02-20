using Newtonsoft.Json.Linq;
using System;

namespace Ulko.Data
{
    public enum Stat
    {
        MaxHP,
        MaxMP,
        Attack,
        Magic,
        Defense,
        MagicDefense,
        Evade,
        Accuracy,
        Critical,
        Speed
    }

    [Serializable]
    public class Level : IJsonObject
    {
        public int level;
        public int exp;
        public float maxHp;
        public float maxMp;
        public float atk;
        public float mag;
        public float def;
        public float mDef;
        public float evd;
        public float acc;
        public float crit;
        public float spd;

        public float GetStat(Stat stat)
        {
            switch (stat)
            {
                case Stat.MaxHP: return maxHp;
                case Stat.MaxMP: return maxMp;
                case Stat.Attack: return atk;
                case Stat.Magic: return mag;
                case Stat.Defense: return def;
                case Stat.MagicDefense: return mDef;
                case Stat.Evade: return evd;
                case Stat.Accuracy: return acc;
                case Stat.Critical: return crit;
                case Stat.Speed: return spd;
            }

            return 0;
        }

        public void SetStat(Stat stat, float value)
        {
            switch (stat)
            {
                case Stat.MaxHP: maxHp = value; break;
                case Stat.MaxMP: maxMp = value; break;
                case Stat.Attack: atk = value; break;
                case Stat.Magic: mag = value; break;
                case Stat.Defense: def = value; break;
                case Stat.MagicDefense: mDef = value; break;
                case Stat.Evade: evd = value; break;
                case Stat.Accuracy: acc = value; break;
                case Stat.Critical: crit = value; break;
                case Stat.Speed: spd = value; break;
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
            maxHp = json["maxHp"].ToObject<float>();
            maxMp = json["maxMp"].ToObject<float>();
            atk = json["atk"].ToObject<float>();
            mag = json["mag"].ToObject<float>();
            def = json["def"].ToObject<float>();
            mDef = json["mDef"].ToObject<float>();
            evd = json["evd"].ToObject<float>();
            acc = json["acc"].ToObject<float>();
            crit = json["crit"].ToObject<float>();
            spd = json["spd"].ToObject<float>();
        }

        public JToken ToJson()
        {
            var json = new JObject
            {
                { "level", level },
                { "exp", exp },
                { "maxHp", maxHp },
                { "maxMp", maxMp },
                { "atk", atk },
                { "mag", mag },
                { "def", def },
                { "mDef", mDef },
                { "evd", evd },
                { "acc", acc },
                { "crit", crit },
                { "spd", spd }
            };

            return json;
        }
    }
}
