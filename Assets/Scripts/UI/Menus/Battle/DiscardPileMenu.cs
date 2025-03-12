using Ulko.Battle;
using HotChocolate.UI;
using Ulko.Data;

namespace Ulko.UI
{
    public class DiscardPileMenuData
    {
        public IGameState gameState;
        public UIRoot uiRoot;
        public BattleInstance battleInstance;
    }

    public class DiscardPileMenu : Menu
    {
        public DeckOfCardsView cardDeckView;

        private MenuStack stack;
        private DiscardPileMenuData data;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        private void Start()
        {
            cardDeckView.OnCardSelected += UpdateInfo;
            cardDeckView.OnCardClicked += OnCardClicked;
        }

        private void OnDestroy()
        {
            cardDeckView.OnCardSelected -= UpdateInfo;
            cardDeckView.OnCardClicked -= OnCardClicked;
        }

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as DiscardPileMenuData;

            Refresh();
        }

        protected override void _OnPop() { }
        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        private void Refresh()
        {
            cardDeckView.Init();

            cardDeckView.AddCards(data.battleInstance.DiscardPile, (string id) => { return data.battleInstance.FindCharacter(id).CaptureState(); });

            if (cardDeckView.Cards.Count > 0)
                Select(cardDeckView.Cards[0].button.gameObject);
        }

        private void UpdateInfo(CardView cardView)
        {
            var abilityView = cardView.GetComponentInChildren<AbilityView>();
            if (abilityView != null)
            {
                data.uiRoot.SetInfo(abilityView.AbilityAsset.FlavorText);
            }
        }

        private void OnCardClicked(CardView cardView)
        {
            cardView.button.SuperSelect(false);
        }
    }
}
