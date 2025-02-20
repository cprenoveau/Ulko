using Ulko.Data.Abilities;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Inventory
{
    [CreateAssetMenu(fileName = "Armor", menuName = "Ulko/Inventory/Armor", order = 1)]
    public class Armor : ItemAsset
    {
        public List<ItemStat> stats = new List<ItemStat>();
        public List<Status> giveStatus = new List<Status>();

        public override ItemType Type => ItemType.Armor;

        public int GetStat(Stat stat)
        {
            var statObj = stats.Find(s => s.stat == stat);
            if (statObj != null) return (int)statObj.value;
            return 0;
        }

        public override string Description()
        {
            string str = "";
            for(int i = 0; i < stats.Count; ++i)
            {
                str += TextFormat.Localize(stats[i].stat) + " ";
                if (stats[i].value > 0) str += "+";
                str += TextFormat.ToString(stats[i].stat, stats[i].value);

                if (i < stats.Count - 1)
                    str += " ";
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
