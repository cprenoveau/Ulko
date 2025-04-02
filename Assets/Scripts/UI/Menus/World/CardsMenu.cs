using HotChocolate.UI;
using System.Collections.Generic;
using Ulko.Data;
using Ulko.Data.Abilities;
using UnityEngine;

namespace Ulko.UI
{
    public class CardsMenu : Menu
    {
        public DeckOfCardsView equipedDeckView;
        public DeckOfCardsView reserveDeckView;

        private MenuStack stack;
        private MenuData data;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        private void Start()
        {
            equipedDeckView.OnCardSelected += UpdateInfo;
            equipedDeckView.OnCardClicked += AddToReserve;

            reserveDeckView.OnCardSelected += UpdateInfo;
            reserveDeckView.OnCardClicked += RemoveFromReserve;
        }

        private void OnDestroy()
        {
            equipedDeckView.OnCardSelected -= UpdateInfo;
            equipedDeckView.OnCardClicked -= AddToReserve;

            reserveDeckView.OnCardSelected -= UpdateInfo;
            reserveDeckView.OnCardClicked -= RemoveFromReserve;
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
            equipedDeckView.Init(PlayerProfile.CurrentDeck(), CaptureState, false);
            reserveDeckView.Init(PlayerProfile.ReserveDeck(), CaptureState, false);

            Layout.SetupGrids(equipedDeckView.cardsAnchor, reserveDeckView.cardsAnchor, Vector2.zero);

            if (equipedDeckView.Cards.Count > 0)
                Select(equipedDeckView.Cards[0].button.gameObject);
        }

        private CharacterState CaptureState(string heroId)
        {
            var hero = PlayerProfile.GetPartyMember(heroId);
            var heroAsset = PlayerProfile.FindHero(PlayerProfile.CurrentStory, PlayerProfile.GetProgression(), heroId);

            return new CharacterState(heroId, heroAsset.displayName, hero.hp, CharacterSide.Heroes, PlayerProfile.GetHeroStats(heroId), new Level(), new List<StatusState>());
        }

        private void UpdateInfo(CardView cardView)
        {
            var abilityView = cardView.GetComponentInChildren<AbilityView>();
            if (abilityView != null)
            {
                data.uiRoot.SetInfo(abilityView.AbilityAsset.FlavorText);
            }
        }

        private void AddToReserve(CardView cardView)
        {
            PlayerProfile.PutInReserve(cardView.Card as Card<AbilityCardData>);

            Refresh();
        }

        private void RemoveFromReserve(CardView cardView)
        {
            PlayerProfile.RemoveFromReserve(cardView.Card as Card<AbilityCardData>);

            Refresh();
        }
    }
}
