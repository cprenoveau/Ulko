using Ulko.Data.Abilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Ulko
{
    public class Card
    {
        public int Id { get; private set; } //unique card id
        public string OwnerId { get; private set; }
        public IActionAsset Action { get; private set; }

        private static int currentId;

        public Card(string ownerId, IActionAsset action)
        {
            Id = currentId++;
            OwnerId = ownerId;
            Action = action;
        }
    }

    public class CardDeck : IEnumerable<Card>
    {
        public CardDeck() { }
        public CardDeck(IEnumerable<Card> cards)
        {
            this.cards.AddRange(cards);
        }

        private readonly List<Card> cards = new();

        public bool TryAddCard(Card card)
        {
            if (IsInDeck(card))
                return false;

            cards.Add(card);
            return true;
        }

        public List<Card> TryAddCards(IEnumerable<Card> cards)
        {
            var result = new List<Card>();

            foreach (var card in cards)
            {
                if (TryAddCard(card))
                    result.Add(card);
            }

            return result;
        }

        public bool TryRemoveCard(Card card)
        {
            int index = cards.FindIndex(c => c.Id == card.Id);
            if(index != -1)
            {
                cards.RemoveAt(index);
                return true;
            }

            return false;
        }

        public List<Card> TryRemoveCards(IEnumerable<Card> cards)
        {
            var result = new List<Card>();

            foreach (var card in cards)
            {
                if (TryRemoveCard(card))
                    result.Add(card);
            }

            return result;
        }

        public bool IsInDeck(Card card)
        {
            return cards.FindIndex(c => c.Id == card.Id) != -1;
        }

        public void Flush()
        {
            cards.Clear();
        }

        //https://stackoverflow.com/questions/273313/randomize-a-listt
        private static readonly Random rng = new();
        public void Shuffle()
        {
            int n = cards.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (cards[n], cards[k]) = (cards[k], cards[n]);
            }
        }

        public List<Card> Peek(int n)
        {
            var result = new List<Card>();
            for(int i = 0; i < n && i < cards.Count; ++i)
            {
                result.Add(cards[i]);
            }

            return result;
        }

        /// <summary>
        /// Picks n cards from deck and adds them to current hand.
        /// Picked cards are removed from deck. If n is greater than deck card count, the whole deck is added to hand.
        /// </summary>
        public CardHand DrawCards(int n, CardHand currentHand)
        {
            currentHand.TryAddCards(DrawCards(n));
            return currentHand;
        }

        /// <summary>
        /// Picks n cards from top of the deck.
        /// Picked cards are removed from deck. If n is greater than deck card count, the whole deck is returned.
        /// </summary>
        public List<Card> DrawCards(int n)
        {
            var result = new List<Card>();

            int deckCardCount = cards.Count;
            for(int i = 0; i < n && i < deckCardCount; ++i)
            {
                var card = cards[0];

                result.Add(card);
                cards.RemoveAt(0);
            }

            return result;
        }

        public IEnumerator<Card> GetEnumerator()
        {
            return cards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return cards.GetEnumerator();
        }
    }
}
