using System;
using System.Collections.Generic;
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

        public delegate CharacterState GetCharacterDelegate(string ownerId);
        public void Init(DeckOfCards cardDeck, GetCharacterDelegate getCharacter, bool setupGrid = true)
        {
            Cards.Clear();

            foreach (Transform card in cardsAnchor.transform)
            {
                Destroy(card.gameObject);
            }

            foreach (var card in cardDeck)
            {
                var cardInstance = Instantiate(cardPrefab, cardsAnchor.transform);
                cardInstance.Init(card);

                var abilityCard = card as Card<AbilityCardData>;

                var abilityView = cardInstance.GetComponentInChildren<AbilityView>();
                var characterState = getCharacter(abilityCard.Data.ownerId);

                abilityView.Init(abilityCard.Data.abilityAsset, characterState);

                cardInstance.OnSelected += (CardView cardView) => { OnCardSelected?.Invoke(cardView); };
                cardInstance.OnClick += (CardView cardView) => { OnCardClicked?.Invoke(cardView); };

                Cards.Add(cardInstance);
            }

            if(setupGrid)
                Layout.SetupGrid(cardsAnchor, Vector2.zero);
        }
    }
}
