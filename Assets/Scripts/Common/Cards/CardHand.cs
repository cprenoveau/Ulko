using System.Collections;
using System.Collections.Generic;

namespace Ulko
{
    public class CardHand : IEnumerable<Card>
    {
        private readonly List<Card> cards = new();

        public CardHand() { }
        public CardHand(IEnumerable<Card> cards)
        {
            this.cards.AddRange(cards);
        }

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

            foreach(var card in cards)
            {
                if(TryAddCard(card))
                    result.Add(card);
            }

            return result;
        }

        public bool TryRemoveCard(Card card)
        {
            int index = cards.FindIndex(c => c.Id == card.Id);
            if (index != -1)
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
                if(TryRemoveCard(card))
                    result.Add(card);
            }

            return result;
        }

        public void DiscardCards(IEnumerable<Card> cards, CardDeck discardPile)
        {
            discardPile.TryAddCards(TryRemoveCards(cards));
        }

        public bool IsInDeck(Card card)
        {
            return cards.FindIndex(c => c.Id == card.Id) != -1;
        }

        public void Flush()
        {
            cards.Clear();
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
