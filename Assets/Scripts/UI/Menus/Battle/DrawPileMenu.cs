﻿using Ulko.Battle;
using HotChocolate.UI;

namespace Ulko.UI
{
    public class DrawPileMenuData
    {
        public IGameState gameState;
        public UIRoot uiRoot;
        public BattleInstance battleInstance;
    }

    public class DrawPileMenu : Menu
    {
        public DeckOfCardsView cardDeckView;

        private MenuStack stack;
        private DrawPileMenuData data;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        private void Start()
        {
            cardDeckView.OnCardSelected += UpdateInfo;
        }

        private void OnDestroy()
        {
            cardDeckView.OnCardSelected -= UpdateInfo;
        }

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as DrawPileMenuData;

            Refresh();
        }

        protected override void _OnPop() { }
        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        private void Refresh()
        {
            cardDeckView.Init();

            cardDeckView.AddCards(data.battleInstance.DrawPile, data.battleInstance);

            if (cardDeckView.Cards.Count > 0)
                Select(cardDeckView.Cards[0].button.gameObject);
        }

        private void UpdateInfo(CardView cardView)
        {
            var abilityView = cardView.GetComponentInChildren<AbilityView>();
            if(abilityView != null)
            {
                data.uiRoot.SetInfo(abilityView.AbilityAsset.Description());
            }
        }
    }
}
