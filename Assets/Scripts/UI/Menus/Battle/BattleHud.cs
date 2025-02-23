using Ulko.Battle;
using HotChocolate.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class BattleHudData
    {
        public IGameState gameState;
        public UIRoot uiRoot;
        public BattleInstance battleInstance;
    }

    public class BattleHud : Menu
    {
        public BattleHeroView heroPrefab;
        public RectTransform heroParent;

        private MenuStack stack;
        private BattleHudData data;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as BattleHudData;

            PlayerProfile.OnPartyChanged += RefreshHeroes;
        }

        protected override void _OnPop() 
        {
            PlayerProfile.OnPartyChanged -= RefreshHeroes;
        }

        protected override void _OnFocusIn(bool fromPush, string previousMenu)
        {
            RefreshHeroes();
            data.uiRoot.SetInfo(null);
        }

        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        private void RefreshHeroes()
        {
            foreach (Transform child in heroParent)
            {
                HotChocolate.UI.Menu.DisposeAllAnimations(child.gameObject);
                Destroy(child.gameObject);
            }

            foreach(var hero in data.battleInstance.Heroes)
            {
                var instance = Instantiate(heroPrefab, heroParent);
                instance.Init(hero, false);
            }
        }
    }
}
