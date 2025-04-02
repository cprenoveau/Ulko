using System;
using System.Collections;
using System.Collections.Generic;

namespace Ulko.Data
{
    public class DeckOfCards : IEnumerable<ICard>
    {
        public DeckOfCards() { }
        public DeckOfCards(IEnumerable<ICard> cards)
        {
            this.cards.AddRange(cards);
        }

        private readonly List<ICard> cards = new();

        public bool TryAddCard(ICard card)
        {
            if (IsInDeck(card))
                return false;

            cards.Add(card);
            return true;
        }

        public List<ICard> TryAddCards(IEnumerable<ICard> cards)
        {
            var result = new List<ICard>();

            foreach (var card in cards)
            {
                if (TryAddCard(card))
                    result.Add(card);
            }

            return result;
        }

        public bool TryRemoveCard(ICard card)
        {
            int index = cards.FindIndex(c => c.Id == card.Id);
            if(index != -1)
            {
                cards.RemoveAt(index);
                return true;
            }

            return false;
        }

        public List<ICard> TryRemoveCards(IEnumerable<ICard> cards)
        {
            var result = new List<ICard>();

            foreach (var card in cards)
            {
                if (TryRemoveCard(card))
                    result.Add(card);
            }

            return result;
        }

        public void ShuffleDeckInto(DeckOfCards discardPile)
        {
            TryAddCards(discardPile);
            discardPile.Flush();
            Shuffle();
        }

        public bool IsInDeck(ICard card)
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

        public List<ICard> Peek(int n)
        {
            var result = new List<ICard>();
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
        public HandOfCards DrawCards(int n, HandOfCards currentHand)
        {
            currentHand.TryAddCards(DrawCards(n));
            return currentHand;
        }

        /// <summary>
        /// Picks n cards from top of the deck.
        /// Picked cards are removed from deck. If n is greater than deck card count, the whole deck is returned.
        /// </summary>
        public List<ICard> DrawCards(int n)
        {
            var result = new List<ICard>();

            int deckCardCount = cards.Count;
            for(int i = 0; i < n && i < deckCardCount; ++i)
            {
                var card = cards[0];

                result.Add(card);
                cards.RemoveAt(0);
            }

            return result;
        }

        public IEnumerator<ICard> GetEnumerator()
        {
            return cards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return cards.GetEnumerator();
        }
    }
}
