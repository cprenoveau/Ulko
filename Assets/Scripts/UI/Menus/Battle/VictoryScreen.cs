using Ulko.Battle;
using HotChocolate.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class VictoryScreenData
    {
        public IGameState gameState;
        public UIRoot uiRoot;
        public BattleInstance battleInstance;
        public Action onClose;
    }

    public class VictoryScreen : Menu
    {
        public MenuDefinition levelUp;

        public TMP_Text expText;
        public Button nextButton;

        private MenuStack stack;
        private VictoryScreenData data;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        private void Start()
        {
            nextButton.onClick.AddListener(ShowNext);
            Localization.LocaleChanged += Refresh;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= Refresh;
        }

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as VictoryScreenData;

            Refresh();
        }

        protected override void _OnPop()
        {
            data.onClose?.Invoke();
        }

        protected override void _OnFocusIn(bool fromPush, string previousMenu)
        {
            data.uiRoot.SetInfo(null);
        }

        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        private void Refresh()
        {
            expText.text = Localization.LocalizeFormat("total_exp", data.battleInstance.TotalExp());
        }

        public override bool CanClose()
        {
            return false;
        }

        private void ShowNext()
        {
            ApplyNextExp();
        }

        private int heroIndex = -1;
        private void ApplyNextExp()
        {
            var expPerHero = data.battleInstance.GetExp();
            heroIndex++;

            if(heroIndex < expPerHero.Count)
            {
                var hero = expPerHero[heroIndex];

                int oldLevel = PlayerProfile.GetHeroLevel(hero.heroId);
                PlayerProfile.AddHeroExp(hero.heroId, hero.exp);
                int newLevel = PlayerProfile.GetHeroLevel(hero.heroId);

                if(oldLevel != newLevel)
                {
                    PlayerProfile.HealHero(hero.heroId);

                    stack.Push(levelUp.asset, levelUp.id, new LevelUpScreenData()
                    {
                        gameState = data.gameState,
                        uiRoot = data.uiRoot,
                        heroId = hero.heroId,
                        oldLevel = oldLevel,
                        onClose = ApplyNextExp
                    });
                }
                else
                {
                    ApplyNextExp();
                }
            }
            else
            {
                stack.Pop();
            }
        }
    }
}
