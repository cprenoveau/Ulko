using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class HandOfCardsView : MonoBehaviour
    {
        public Selectable previousSelectable;
        public Selectable nextSelectable;

        public RectTransform cardsAnchor;
        public RectTransform selectedCardAnchor;

        public float maxSpacing = 60f;
        public float selectedScale = 1.2f;
        public float selectedOffset = 60f;
        public float maxAngle = 5f;
        public float radius = 5000f;

        public int CardCount => cards.Count;

        public List<CardView> ClickedCardStack { get; private set; } = new List<CardView>();

        public event Action<CardView> OnCardSelected;
        public event Action<CardView> OnCardDeselected;
        public event Action<CardView> OnCardClicked;

        private List<CardView> cards = new();

        public T AddCard<T>(T cardPrefab) where T : CardView
        {
            var cardInstance = Instantiate(cardPrefab, cardsAnchor);
            cardInstance.name = "Card " + cards.Count;
            cardInstance.GetComponent<RectTransform>().SetAsFirstSibling();

            cardInstance.OnSelected += OnSelect;
            cardInstance.OnDeselected += OnDeselect;
            cardInstance.OnClick += OnClick;

            cards.Add(cardInstance);

            Refresh();

            return cardInstance;
        }

        public CardView GetCard(int cardIndex)
        {
            if (cardIndex < 0 || cardIndex >= CardCount)
            {
                Debug.LogWarning("HandOfCardsView: Trying to get card index " + cardIndex + " that's out of range");
                return null;
            }

            return cards[cardIndex];
        }

        public bool Cancel()
        {
            if (ClickedCardStack.Count > 0)
            {
                var card = ClickedCardStack[0];
                ClickedCardStack.RemoveAt(0);
                card.button.SuperSelect(false);

                return true;
            }

            return false;
        }

        public void RemoveCard(int cardIndex)
        {
            var card = GetCard(cardIndex);

            if(card == null)
                return;

            card.OnSelected -= OnSelect;
            card.OnDeselected -= OnDeselect;

            cards.RemoveAt(cardIndex);
            GameObject.Destroy(card.gameObject);

            Refresh();
        }

        public void RemoveAllCards()
        {
            foreach(var card in cards)
            {
                card.OnSelected -= OnSelect;
                card.OnDeselected -= OnDeselect;

                Destroy(card.gameObject);
            }

            cards.Clear();
            Refresh();
        }

        private void OnSelect(CardView selectedCard)
        {
            selectedCard.transform.SetParent(selectedCardAnchor);
            UpdateIndexes();

            RefreshTransforms();

            ClickedCardStack.Remove(selectedCard);
            OnCardSelected?.Invoke(selectedCard);
        }

        private void OnDeselect(CardView selectedCard)
        {
            selectedCard.transform.SetParent(cardsAnchor);
            UpdateIndexes();

            RefreshTransforms();

            ClickedCardStack.Remove(selectedCard);
            OnCardDeselected?.Invoke(selectedCard);
        }

        private void OnClick(CardView clickedCard)
        {
            ClickedCardStack.Remove(clickedCard);
            ClickedCardStack.Insert(0, clickedCard);

            OnCardClicked?.Invoke(clickedCard);
        }

        private void UpdateIndexes()
        {
            int index = 0;
            for(int i = cards.Count - 1; i >= 0; --i)
            {
                if (cards[i].transform.parent == cardsAnchor)
                {
                    cards[i].transform.SetSiblingIndex(index);
                    index++;
                }
            }
        }

        private void RefreshTransforms()
        {
            float anchorWidth = cardsAnchor.rect.width;
            float cardWidth = cards[0].GetComponent<RectTransform>().rect.width;
            float spacing = (anchorWidth - cardWidth) / (cards.Count - 1);
            spacing = Mathf.Clamp(spacing, 0, maxSpacing);

            float totalWidth = cardWidth + (cards.Count - 1) * spacing;
            float startX = (cardWidth - totalWidth) / 2f;

            for (int i = 0; i < cards.Count; i++)
            {
                float angle = 0;
                if (cards.Count > 1)
                {
                    float angleStep = maxAngle * 2 / (cards.Count - 1);
                    angle = maxAngle - (i * angleStep);
                }
                cards[i].transform.localEulerAngles = new Vector3(0, 0, angle);

                float xPos = startX + i * spacing;
                float yPos = radius * Mathf.Sin(Mathf.Deg2Rad * (90 - angle)) - radius;
                cards[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);

                cards[i].transform.localScale = Vector3.one;

                if (cards[i].transform.parent == selectedCardAnchor)
                {
                    cards[i].GetComponent<RectTransform>().anchoredPosition += new Vector2(0, selectedOffset);
                    cards[i].transform.localScale = new Vector3(selectedScale, selectedScale, 1);
                    cards[i].transform.localRotation = Quaternion.identity;
                }
            }
        }

        private void Refresh()
        {
            foreach(var card in ClickedCardStack)
            {
                card.button.SuperSelect(false, false);
            }

            ClickedCardStack.Clear();

            if (cards.Count == 0)
                return;

            RefreshTransforms();

            for(int i = 0; i < cards.Count; i++)
            {
                var nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnDown = cards[i].button.navigation.selectOnDown,
                    selectOnUp = cards[i].button.navigation.selectOnUp
                };

                if (i == 0)
                {
                    if (previousSelectable != null)
                    {
                        nav.selectOnLeft = previousSelectable;

                        var selectOnUp = previousSelectable.navigation.selectOnUp;
                        var selectOnDown = previousSelectable.navigation.selectOnDown;

                        previousSelectable.navigation = new Navigation
                        {
                            mode = Navigation.Mode.Explicit,
                            selectOnRight = cards[i].button,
                            selectOnLeft = nextSelectable != null ? nextSelectable : cards.Last().button,
                            selectOnUp = selectOnUp,
                            selectOnDown = selectOnDown
                        };
                    }
                    else if(nextSelectable != null)
                    {
                        nav.selectOnLeft = nextSelectable;
                    }
                    else
                    {
                        nav.selectOnLeft = cards.Last().button;
                    }
                }
                else
                {
                    nav.selectOnLeft = cards[i - 1].button;
                }

                if (i == cards.Count - 1)
                {
                    if (nextSelectable != null)
                    {
                        nav.selectOnRight = nextSelectable;

                        var selectOnUp = nextSelectable.navigation.selectOnUp;
                        var selectOnDown = nextSelectable.navigation.selectOnDown;

                        if(selectOnUp != null)
                        {
                            selectOnUp.navigation = new Navigation
                            {
                                mode = Navigation.Mode.Explicit,
                                selectOnLeft = cards[i].button,
                                selectOnRight = previousSelectable != null ? previousSelectable : cards.First().button,
                                selectOnUp = selectOnUp.navigation.selectOnUp,
                                selectOnDown = selectOnUp.navigation.selectOnDown
                            };
                        }

                        nextSelectable.navigation = new Navigation
                        {
                            mode = Navigation.Mode.Explicit,
                            selectOnLeft = cards[i].button,
                            selectOnRight = previousSelectable != null ? previousSelectable : cards.First().button,
                            selectOnUp = selectOnUp,
                            selectOnDown = selectOnDown
                        };
                    }
                    else if(previousSelectable != null)
                    {
                        nav.selectOnRight = previousSelectable;
                    }
                    else
                    {
                        nav.selectOnRight = cards[0].button;
                    }
                }
                else
                {
                    nav.selectOnRight = cards[i + 1].button;
                }

                if (cards[i].extraButtons.Count > 0)
                {
                    nav.selectOnUp = cards[i].extraButtons.Last();
                    nav.selectOnDown = cards[i].extraButtons.First();
                }

                cards[i].button.navigation = nav;

                foreach(var extraButton in cards[i].extraButtons)
                {
                    var extraNav = extraButton.navigation;
                    extraNav.selectOnLeft = nav.selectOnLeft;
                    extraNav.selectOnRight= nav.selectOnRight;
                    extraButton.navigation = extraNav;
                }

                foreach (var extraButton in cards[i].extraButtons)
                    extraButton.gameObject.SetActive(false);
            }
        }
    }
}
