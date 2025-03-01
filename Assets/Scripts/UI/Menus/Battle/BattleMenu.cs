using Ulko.Battle;
using HotChocolate.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

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
        }

        private void OnDestroy()
        {
            handOfCardsView.OnCardSelected -= OnCardSelected;
            handOfCardsView.OnCardClicked -= OnCardClicked;
        }

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as BattleMenuData;

            Refresh();
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

        private void Refresh()
        {
            handOfCardsView.RemoveAllCards();

            var possibleActions = data.playerAction.PossibleActions;

            int maxCardIndex = data.playerAction.PossibleActions.Max(a => a.cardIndex);
            for(int i = 0; i <= maxCardIndex; ++i)
            {
                var actions = possibleActions.Where(a => a.cardIndex == i).ToList();

                if (actions.Count() > 0)
                {
                    var owner = data.battleInstance.FindCharacter(actions.First().actorId);

                    var abilityCard = handOfCardsView.AddCard(abilityPrefab);
                    abilityCard.Init(actions.First().ability, owner, actions);

                    abilityCard.OnThrow += OnThrowClicked;
                }
            }

            Select(handOfCardsView.GetCard(0).button.gameObject);
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
                    data.uiRoot.SetInfo(battleAbility.abilityView.AbilityAsset.Description());
            }
        }

        private void OnThrowClicked(CardView cardView)
        {
            if (cardView is BattleAbilityCardView battleAbility)
            {
                var possibleActions = data.playerAction.PossibleActions.Where(a => a.isCardThrow);

                ShowTargetMenu(
                    possibleActions,
                    null,
                    (BattleActions actions) => DeclareAction(actions, battleAbility));
            }
        }

        private void OnCardClicked(CardView cardView)
        {
            if (cardView is BattleAbilityCardView battleAbility)
            {
                ShowTargetMenu(
                    battleAbility.Actions,
                    null,
                    (BattleActions actions) => DeclareAction(actions, battleAbility));

            }
        }

        private void ShowTargetMenu(
            IEnumerable<BattleActions> possibleActions,
            BattleTargetMenuData.OnTargetSelected onTargetSelected,
            BattleTargetMenuData.OnTargetSelected onTargetConfirmed)
        {
            var targetData = new BattleTargetMenuData
            {
                gameState = data.gameState,
                uiRoot = data.uiRoot,
                battleInstance = data.battleInstance,
                possibleActions = possibleActions,
                onTargetSelected = onTargetSelected,
                onTargetConfirmed = onTargetConfirmed
            };

            stack.Push(battleTargetMenu.asset, battleTargetMenu.id, targetData);
        }

        private void DeclareAction(BattleActions action, BattleAbilityCardView cardView)
        {
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
