using System;
using System.Collections.Generic;
using Ulko.Battle;
using Ulko.Data;
using Ulko.Data.Abilities;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class DeckOfCardsView : MonoBehaviour
    {
        public CardView cardPrefab;
        public GridLayoutGroup cardsAnchor;

        public List<CardView> Cards { get; private set; } = new List<CardView>();

        public event Action<CardView> OnCardSelected;
        public event Action<CardView> OnCardClicked;

        public void Init()
        {
            foreach (Transform card in cardsAnchor.transform)
            {
                Destroy(card.gameObject);
            }
        }

        public delegate CharacterState GetCharacterDelegate(string id);
        public void AddCards(DeckOfCards<AbilityCard> cardDeck, GetCharacterDelegate getCharacter)
        {
            foreach (var card in cardDeck)
            {
                var cardInstance = Instantiate(cardPrefab, cardsAnchor.transform);

                var abilityView = cardInstance.GetComponentInChildren<AbilityView>();
                var characterState = getCharacter(card.Data.ownerId);

                abilityView.Init(card.Data.abilityAsset, characterState);

                cardInstance.OnSelected += (CardView cardView) => { OnCardSelected?.Invoke(cardView); };
                cardInstance.OnClick += (CardView cardView) => { OnCardClicked?.Invoke(cardView); };

                Cards.Add(cardInstance);
            }

            Layout.SetupGrid(cardsAnchor, Vector2.zero);
        }
    }
}
