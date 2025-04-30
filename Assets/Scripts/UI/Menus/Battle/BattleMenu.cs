using Ulko.Battle;
using HotChocolate.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using TMPro;
using Ulko.Data;

namespace Ulko.UI
{
    public class BattleMenuData
    {
        public IGameState gameState;
        public UIRoot uiRoot;
        public BattleInstance battleInstance;
        public PlayerAction playerAction;
    }

    public class BattleMenu : Menu
    {
        public MenuDefinition battleMenu;
        public MenuDefinition battleTargetMenu;
        public MenuDefinition drawPileMenu;
        public MenuDefinition discardPileMenu;

        public TMP_Text redrawLabel;
        public PointedButton redrawButton;
        public PointedButton drawPileButton;
        public PointedButton discardPileButton;

        public HandOfCardsView handOfCardsView;
        public BattleAbilityCardView abilityPrefab;
        public RectTransform cardThrowParent;

        public Vector3 cardThrowAcceleration = new(0.1f, 0.1f, 0);
        public Vector3 cardThrowVelocity = new(-1, -1, 0);
        public Vector3 cardThrowRotation = new(1000f, 500f, 0f);

        private MenuStack stack;
        private BattleMenuData data;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        private void Start()
        {
            handOfCardsView.OnCardSelected += OnCardSelected;
            handOfCardsView.OnCardClicked += OnCardClicked;

            redrawButton.OnSelected += OnRedrawSelected;
            redrawButton.onClick.AddListener(RedrawHand);

            drawPileButton.OnSelected += OnDrawPileSelected;
            drawPileButton.onClick.AddListener(ShowDrawPileMenu);

            discardPileButton.OnSelected += OnDiscardPileSelected;
            discardPileButton.onClick.AddListener(ShowDiscardPileMenu);

            Localization.LocaleChanged += RefreshLabels;
        }

        private void OnDestroy()
        {
            handOfCardsView.OnCardSelected -= OnCardSelected;
            handOfCardsView.OnCardClicked -= OnCardClicked;

            redrawButton.OnSelected -= OnRedrawSelected;
            drawPileButton.OnSelected -= OnDrawPileSelected;
            discardPileButton.OnSelected -= OnDiscardPileSelected;

            Localization.LocaleChanged -= RefreshLabels;
        }

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as BattleMenuData;

            RefreshCards();

            if (this.data.battleInstance.CurrentHand.Count() > 0)
            {
                Select(handOfCardsView.GetCard(0).button.gameObject);
            }
            else
            {
                RedrawHand();
                Select(handOfCardsView.GetCard(0).button.gameObject);
            }
        }

        protected override void _OnPop() { }
        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        public override bool CanClose()
        {
            return false;
        }

        protected override bool _Cancel()
        {
            return handOfCardsView.Cancel();
        }

        private void RefreshCards()
        {
            handOfCardsView.RemoveAllCards();

            var possibleActions = data.playerAction.PossibleActions;

            int cardIndex = 0;
            foreach(var card in data.battleInstance.CurrentHand)
            {
                var abilityCard = card as Card<AbilityCardData>;

                var actions = possibleActions.Where(a => a.cardIndex == cardIndex).ToList();
                var owner = data.battleInstance.FindCharacter(abilityCard.Data.ownerId);

                var abilityCardView = handOfCardsView.AddCard(abilityPrefab);
                abilityCardView.Init(cardIndex, abilityCard.Data.abilityAsset, owner, actions);

                abilityCardView.OnThrow += OnThrowClicked;

                cardIndex++;
            }

            RefreshLabels();
        }

        private void RefreshLabels()
        {
            redrawLabel.text = GetRedrawLabel();
        }

        private void RedrawHand()
        {
            data.battleInstance.RedrawHand(data.playerAction);
            RefreshCards();
        }

        private void OnRedrawSelected()
        {
            data.uiRoot.SetInfo(GetRedrawLabel());
        }

        private string GetRedrawLabel()
        {
            string str = Localization.Localize("redraw");
            if (data.battleInstance.FreeRedrawInTurns <= 0)
            {
                str += " (free)";
            }
            else
            {
                str += " <nobr>(free in " + data.battleInstance.FreeRedrawInTurns + ")</nobr>";
            }

            return str;
        }

        private void OnDrawPileSelected()
        {
            data.uiRoot.SetInfo(Localization.Localize("draw_pile"));
        }

        private void OnDiscardPileSelected()
        {
            data.uiRoot.SetInfo(Localization.Localize("discard_pile"));
        }

        private void ShowDrawPileMenu()
        {
            drawPileButton.SuperSelect(true);

            stack.Push(drawPileMenu.asset, drawPileMenu.id, new DrawPileMenuData
            {
                gameState = data.gameState,
                uiRoot = data.uiRoot,
                battleInstance = data.battleInstance
            });
        }

        private void ShowDiscardPileMenu()
        {
            discardPileButton.SuperSelect(true);

            stack.Push(discardPileMenu.asset, discardPileMenu.id, new DiscardPileMenuData
            {
                gameState = data.gameState,
                uiRoot = data.uiRoot,
                battleInstance = data.battleInstance
            });
        }

        private void OnCardSelected(CardView cardView)
        {
            UpdateInfo(cardView);
        }

        private void UpdateInfo(CardView cardView)
        {
            if (cardView is BattleAbilityCardView battleAbility)
            {
                if (battleAbility.ThrowSelected)
                    data.uiRoot.SetInfo(Localization.Localize("discard_desc"));
                else
                    data.uiRoot.SetInfo(battleAbility.abilityView.AbilityAsset.FlavorText);
            }
        }

        private void OnThrowClicked(CardView cardView)
        {
            if (cardView is BattleAbilityCardView battleAbility)
            {
                var possibleActions = data.playerAction.PossibleActions.Where(a => a.isCardThrow);

                ShowTargetMenu(
                    null,
                    possibleActions,
                    null,
                    (BattleActions actions) => DeclareAction(battleAbility.CardIndex, actions, battleAbility));
            }
        }

        private void OnCardClicked(CardView cardView)
        {
            if (cardView is BattleAbilityCardView battleAbility)
            {
                ShowTargetMenu(
                    data.battleInstance.FindCharacter(battleAbility.abilityView.Owner.Id),
                    battleAbility.Actions,
                    null,
                    (BattleActions actions) => DeclareAction(battleAbility.CardIndex, actions, battleAbility));

            }
        }

        private void ShowTargetMenu(
            Character cardOwner,
            IEnumerable<BattleActions> possibleActions,
            BattleTargetMenuData.OnTargetSelected onTargetSelected,
            BattleTargetMenuData.OnTargetSelected onTargetConfirmed)
        {
            var targetData = new BattleTargetMenuData
            {
                gameState = data.gameState,
                uiRoot = data.uiRoot,
                battleInstance = data.battleInstance,
                cardOwner = cardOwner,
                possibleActions = possibleActions,
                onTargetSelected = onTargetSelected,
                onTargetConfirmed = onTargetConfirmed
            };

            stack.Push(battleTargetMenu.asset, battleTargetMenu.id, targetData);
        }

        private void DeclareAction(int cardIndex, BattleActions action, BattleAbilityCardView cardView)
        {
            action.cardIndex = cardIndex;

            if (action.isCardThrow)
            {
                StartCoroutine(ThrowCardAsync(action, cardView));
            }
            else
            {
                data.playerAction.DeclareAction(action);
            }
        }

        private IEnumerator ThrowCardAsync(BattleActions action, BattleAbilityCardView cardView)
        {
            using (new UIRoot.BlockInputScope(data.uiRoot))
            {
                cardView.SelectedExtraButton.gameObject.SetActive(false);

                var targets = data.battleInstance.FindCharacters(action.targetIds);

                cardView.transform.SetParent(cardThrowParent);

                Vector2 startPoint = data.gameState.UICamera.WorldToViewportPoint(cardView.transform.position);

                var headPos = targets[0].GetComponentInChildren<HeadAnchor>().transform.position;
                Vector2 targetPoint = data.gameState.Camera.WorldToViewportPoint(headPos);

                cardView.transform.localPosition = Vector3.zero;

                float elapsed = 0;
                while (elapsed < 0.5f)
                {
                    cardView.GetComponent<RectTransform>().anchorMin = Vector3.Lerp(startPoint, targetPoint, elapsed * 2f + 0.5f);
                    cardView.GetComponent<RectTransform>().anchorMax = Vector3.Lerp(startPoint, targetPoint, elapsed * 2f + 0.5f);

                    Vector3 v = cardThrowAcceleration * elapsed + cardThrowVelocity;
                    Vector3 s = 0.5f * elapsed * elapsed * cardThrowAcceleration + v * elapsed + Vector3.one;

                    cardView.transform.localScale = s;
                    if (cardView.transform.localScale.x < 0) cardView.transform.localScale = Vector3.zero;

                    cardView.transform.Rotate(cardThrowRotation * Time.deltaTime);

                    elapsed += Time.deltaTime;

                    yield return null;
                }

                data.playerAction.DeclareAction(action);
            }
        }
    }
}
