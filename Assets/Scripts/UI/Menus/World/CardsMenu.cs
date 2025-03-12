using HotChocolate.UI;
using System.Collections.Generic;
using Ulko.Data;
using Ulko.Data.Abilities;

namespace Ulko.UI
{
    public class CardsMenu : Menu
    {
        public DeckOfCardsView cardDeckView;

        private MenuStack stack;
        private MenuData data;

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
            this.data = data as MenuData;

            Refresh();
        }

        protected override void _OnPop() { }
        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        private void Refresh()
        {
            var currentDeck = new DeckOfCards<AbilityCard>();

            foreach (var hero in PlayerProfile.ActiveParty)
            {
                var heroAsset = PlayerProfile.FindHero(PlayerProfile.CurrentStory, PlayerProfile.GetProgression(), hero.id);
                foreach (var ability in heroAsset.abilities)
                {
                    currentDeck.TryAddCard(new Card<AbilityCard>(new AbilityCard(ability, hero.id)));
                }
            }

            cardDeckView.Init();
            cardDeckView.AddCards(currentDeck, CaptureState);

            if (cardDeckView.Cards.Count > 0)
                Select(cardDeckView.Cards[0].button.gameObject);
        }

        private CharacterState CaptureState(string heroId)
        {
            var hero = PlayerProfile.GetPartyMember(heroId);
            var heroAsset = PlayerProfile.FindHero(PlayerProfile.CurrentStory, PlayerProfile.GetProgression(), heroId);

            return new CharacterState(Id, Localization.Localize(heroAsset.displayName), hero.hp, CharacterSide.Heroes, PlayerProfile.GetHeroStats(heroId), new List<StatusState>());
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
