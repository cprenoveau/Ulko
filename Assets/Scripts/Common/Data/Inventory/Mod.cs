using Ulko.Data.Abilities;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Inventory
{
    [CreateAssetMenu(fileName = "Mod", menuName = "Ulko/Inventory/Mod", order = 1)]
    public class Mod : ItemAsset
    {
        public ColorTag color;
        public List<ElementalTag> elements = new List<ElementalTag>();

        public List<Ability> giveAbilities = new List<Ability>();
        public List<Status> giveStatus = new List<Status>();

        public List<ItemStat> stats = new List<ItemStat>();

        public List<Mod> mergeFrom = new List<Mod>();

        public override ItemType Type => ItemType.Mod;

        public Color Color => color.color;

        public float GetStat(Stat stat)
        {
            var statObj = stats.Find(s => s.stat == stat);
            if (statObj != null) return statObj.value;
            return 0;
        }

        public override string Description()
        {
            string str = "";
            for (int i = 0; i < stats.Count; ++i)
            {
                str += TextFormat.Localize(stats[i].stat) + " ";
                if (stats[i].value > 0) str += "+";
                str += TextFormat.ToString(stats[i].stat, stats[i].value) + "%";

                if (i < stats.Count - 1)
                    str += " ";
            }

            if (giveAbilities.Count > 0)
            {
                if (!string.IsNullOrEmpty(str))
                    str += " ";

                for (int i = 0; i < giveAbilities.Count; ++i)
                {
                    str += Localization.Localize(giveAbilities[i].id);
                    if (i < giveAbilities.Count - 1)
                        str += " ";
                }
            }

            if (giveStatus.Count > 0)
            {
                if (!string.IsNullOrEmpty(str))
                    str += " ";

                for (int i = 0; i < giveStatus.Count; ++i)
                {
                    str += Localization.Localize(giveStatus[i].id);
                    if (i < giveStatus.Count - 1)
                        str += " ";
                }
            }

            return str;
        }
    }
}
