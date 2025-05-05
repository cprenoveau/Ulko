using System.Collections.Generic;
using System.Linq;
using Ulko.Data;
using UnityEngine;

namespace Ulko
{
    public static partial class PlayerProfile
    {
        public static (DeckOfCards equiped, DeckOfCards reserve) CurrentDeck(int minCards, int maxCards)
        {
            var currentDeck = new DeckOfCards();
            var reserveDeck = new DeckOfCards();

            //make sure to remove all cards from inactive heroes
            loadedGame.reserveDeck.RemoveAll(c => !HeroIsActive(c.Data.ownerId));

            var equipedCards = AllAbilityCards();
            equipedCards.RemoveAll(c => IsInReserve(c));

            //not enough equiped cards
            while (equipedCards.Count < minCards && loadedGame.reserveDeck.Count > 0)
            {
                var card = loadedGame.reserveDeck.First();
                if (TryRemoveFromReserve(card, maxCards))
                {
                    equipedCards.Add(card);
                }
            }

            //too many equiped cards
            while (equipedCards.Count > maxCards)
            {
                foreach (var hero in ActiveParty)
                {
                    if (equipedCards.Count <= maxCards)
                        break;

                    int cardIndex = equipedCards.FindIndex(c => c.Data.ownerId == hero.id);
                    if (cardIndex != -1)
                    {
                        var card = equipedCards[cardIndex];

                        if (TryPutInReserve(card, minCards, maxCards))
                        {
                            equipedCards.RemoveAt(cardIndex);
                        }
                    }
                }
            }

            foreach (var card in equipedCards)
            {
                currentDeck.TryAddCard(card);
            }

            foreach (var card in loadedGame.reserveDeck)
            {
                reserveDeck.TryAddCard(card);
            }

            return (currentDeck, reserveDeck);
        }

        public static List<Card<AbilityCardData>> AllAbilityCards()
        {
            var cards = new List<Card<AbilityCardData>>();

            foreach (var hero in ActiveParty)
            {
                var heroAsset = FindHeroAsset(hero.id);
                var abilities = heroAsset.Abilities;

                for (int i = 0; i < abilities.Count(); ++i)
                {
                    int cardId = hero.id.GetHashCode() + i;
                    var card = new Card<AbilityCardData>(cardId, new AbilityCardData(abilities.ElementAt(i), hero.id));

                    cards.Add(card);
                }
            }

            return cards;
        }

        public static int CurrentDeckCount(int maxCards)
        {
            int abilities = 0;

            foreach (var hero in ActiveParty)
            {
                var heroAsset = FindHeroAsset(hero.id);
                abilities += heroAsset.Abilities.Count();
            }

            int cardCount = abilities - loadedGame.reserveDeck.Count;
            return Mathf.Clamp(cardCount, 0, maxCards);
        }

        public static bool IsInReserve(int cardId)
        {
            return loadedGame.reserveDeck.FirstOrDefault(c => c.Id == cardId) != null;
        }

        public static bool IsInReserve(Card<AbilityCardData> card)
        {
            return IsInReserve(card.Id);
        }

        public static bool TryPutInReserve(Card<AbilityCardData> card, int minCardsInDeck, int maxCardsInDeck)
        {
            if (card != null && !IsInReserve(card.Id) && CurrentDeckCount(maxCardsInDeck) > minCardsInDeck)
            {
                loadedGame.reserveDeck.Add(card);
                return true;
            }

            return false;
        }

        public static bool TryRemoveFromReserve(Card<AbilityCardData> card, int maxCardsInDeck)
        {
            if (card != null && IsInReserve(card.Id) && CurrentDeckCount(maxCardsInDeck) < maxCardsInDeck)
            {
                loadedGame.reserveDeck.RemoveAll(c => c.Id == card.Id);
                return true;
            }

            return false;
        }
    }
}
