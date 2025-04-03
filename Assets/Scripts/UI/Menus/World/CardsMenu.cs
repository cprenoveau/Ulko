using HotChocolate.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Ulko.Battle;
using Ulko.Data;
using Ulko.Data.Abilities;
using UnityEngine;

namespace Ulko.UI
{
    public class CardsMenu : Menu
    {
        public BattleConfig battleConfig;

        public TMP_Text deckLabel;
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
            var (equiped, reserve) = PlayerProfile.CurrentDeck(battleConfig.maxCardsInDeck);

            equipedDeckView.Init(equiped, CaptureState, false);
            reserveDeckView.Init(reserve, CaptureState, false);

            Layout.SetupGrids(equipedDeckView.cardsAnchor, reserveDeckView.cardsAnchor, Vector2.zero);

            if (equipedDeckView.Cards.Count > 0)
                Select(equipedDeckView.Cards[0].button.gameObject);

            deckLabel.text = "Deck " + equiped.Count() + "/" + battleConfig.maxCardsInDeck;
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
            if(PlayerProfile.TryPutInReserve(cardView.Card as Card<AbilityCardData>, battleConfig.minCardsInDeck, battleConfig.maxCardsInDeck))
            {
                Refresh();
            }
            else
            {
                cardView.button.SuperSelect(false);
            }
        }

        private void RemoveFromReserve(CardView cardView)
        {
            if(PlayerProfile.TryRemoveFromReserve(cardView.Card as Card<AbilityCardData>, battleConfig.maxCardsInDeck))
            {
                Refresh();
            }
            else
            {
                cardView.button.SuperSelect(false);
            }
        }
    }
}
