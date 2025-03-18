using Ulko.Data;
using HotChocolate.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class StatsMenu : Menu
    {
        public HeroMenu heroMenu;
        public StatView statPrefab;
        public GridLayoutGroup statParent;

        private MenuStack stack;

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;

            heroMenu.Init(data as HeroMenuData, Refresh);
            Refresh();
        }

        protected override void _OnPop() { }
        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu)
        {
            if (fromPop)
                heroMenu.Data.uiRoot.SetInfo(null);
        }

        private void Refresh()
        {
            foreach (Transform child in statParent.transform)
            {
                HotChocolate.UI.Menu.DisposeAllAnimations(child.gameObject);
                Destroy(child.gameObject);
            }

            bool selected = false;
            foreach (Stat stat in Enum.GetValues(typeof(Stat)))
            {
                if (stat == Stat.MaxHP)
                    continue;

                var instance = Instantiate(statPrefab, statParent.transform);
                instance.Init(stat, PlayerProfile.GetHeroStat(heroMenu.Data.hero.id, stat), 0);

                instance.GetComponent<PointedButton>().OnSelected += () => { HoverStat(instance); };

                if(!selected)
                {
                    Select(instance.gameObject);
                    selected = true;
                }
            }

            Layout.SetupGrid(statParent, Vector2.zero);
        }

        private void HoverStat(StatView stat)
        {
            heroMenu.Data.uiRoot.SetInfo(Localization.Localize(stat.Stat.ToString().ToLower() + "_desc"));
        }
    }
}
