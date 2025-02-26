using System;
using System.Collections;
using System.Collections.Generic;

namespace Ulko.Data
{
    public class Card<T>
    {
        public int Id { get; private set; } //unique card id
        public T Data { get; private set; }

        private static int currentId;

        public Card(T data)
        {
            Id = currentId++;
            Data = data;
        }
    }

    public class DeckOfCards<T> : IEnumerable<Card<T>>
    {
        public DeckOfCards() { }
        public DeckOfCards(IEnumerable<Card<T>> cards)
        {
            this.cards.AddRange(cards);
        }

        private readonly List<Card<T>> cards = new();

        public bool TryAddCard(Card<T> card)
        {
            if (IsInDeck(card))
                return false;

            cards.Add(card);
            return true;
        }

        public List<Card<T>> TryAddCards(IEnumerable<Card<T>> cards)
        {
            var result = new List<Card<T>>();

            foreach (var card in cards)
            {
                if (TryAddCard(card))
                    result.Add(card);
            }

            return result;
        }

        public bool TryRemoveCard(Card<T> card)
        {
            int index = cards.FindIndex(c => c.Id == card.Id);
            if(index != -1)
            {
                cards.RemoveAt(index);
                return true;
            }

            return false;
        }

        public List<Card<T>> TryRemoveCards(IEnumerable<Card<T>> cards)
        {
            var result = new List<Card<T>>();

            foreach (var card in cards)
            {
                if (TryRemoveCard(card))
                    result.Add(card);
            }

            return result;
        }

        public bool IsInDeck(Card<T> card)
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

        public List<Card<T>> Peek(int n)
        {
            var result = new List<Card<T>>();
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
        public HandOfCards<T> DrawCards(int n, HandOfCards<T> currentHand)
        {
            currentHand.TryAddCards(DrawCards(n));
            return currentHand;
        }

        /// <summary>
        /// Picks n cards from top of the deck.
        /// Picked cards are removed from deck. If n is greater than deck card count, the whole deck is returned.
        /// </summary>
        public List<Card<T>> DrawCards(int n)
        {
            var result = new List<Card<T>>();

            int deckCardCount = cards.Count;
            for(int i = 0; i < n && i < deckCardCount; ++i)
            {
                var card = cards[0];

                result.Add(card);
                cards.RemoveAt(0);
            }

            return result;
        }

        public IEnumerator<Card<T>> GetEnumerator()
        {
            return cards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return cards.GetEnumerator();
        }
    }
}
