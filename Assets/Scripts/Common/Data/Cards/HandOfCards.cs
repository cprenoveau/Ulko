using System.Collections;
using System.Collections.Generic;

namespace Ulko.Data
{
    public class HandOfCards<T> : IEnumerable<Card<T>>
    {
        private readonly List<Card<T>> cards = new();

        public HandOfCards() { }
        public HandOfCards(IEnumerable<Card<T>> cards)
        {
            this.cards.AddRange(cards);
        }

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

            foreach(var card in cards)
            {
                if(TryAddCard(card))
                    result.Add(card);
            }

            return result;
        }

        public bool TryRemoveCard(Card<T> card)
        {
            int index = cards.FindIndex(c => c.Id == card.Id);
            return TryRemoveCardAt(index);
        }

        public bool TryRemoveCardAt(int index)
        {
            if (index >= 0 && index < cards.Count)
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
                if(TryRemoveCard(card))
                    result.Add(card);
            }

            return result;
        }

        public void DiscardAt(int index, DeckOfCards<T> discardPile)
        {
            if (index >= 0 && index < cards.Count)
            {
                discardPile.TryAddCard(cards[index]);
                cards.RemoveAt(index);
            }
        }

        public void Discard(IEnumerable<Card<T>> cards, DeckOfCards<T> discardPile)
        {
            discardPile.TryAddCards(TryRemoveCards(cards));
        }

        public bool IsInDeck(Card<T> card)
        {
            return cards.FindIndex(c => c.Id == card.Id) != -1;
        }

        public void Flush()
        {
            cards.Clear();
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
