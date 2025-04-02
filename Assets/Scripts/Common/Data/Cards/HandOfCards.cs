using System.Collections;
using System.Collections.Generic;

namespace Ulko.Data
{
    public class HandOfCards : IEnumerable<ICard>
    {
        private readonly List<ICard> cards = new();

        public HandOfCards() { }
        public HandOfCards(IEnumerable<ICard> cards)
        {
            this.cards.AddRange(cards);
        }

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

            foreach(var card in cards)
            {
                if(TryAddCard(card))
                    result.Add(card);
            }

            return result;
        }

        public bool TryRemoveCard(ICard card)
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

        public List<ICard> TryRemoveCards(IEnumerable<ICard> cards)
        {
            var result = new List<ICard>();

            foreach (var card in cards)
            {
                if(TryRemoveCard(card))
                    result.Add(card);
            }

            return result;
        }

        public void DiscardAt(int index, DeckOfCards discardPile)
        {
            if (index >= 0 && index < cards.Count)
            {
                discardPile.TryAddCard(cards[index]);
                cards.RemoveAt(index);
            }
        }

        public void Discard(IEnumerable<ICard> cards, DeckOfCards discardPile)
        {
            discardPile.TryAddCards(TryRemoveCards(cards));
        }

        public bool IsInDeck(ICard card)
        {
            return cards.FindIndex(c => c.Id == card.Id) != -1;
        }

        public void Flush()
        {
            cards.Clear();
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
