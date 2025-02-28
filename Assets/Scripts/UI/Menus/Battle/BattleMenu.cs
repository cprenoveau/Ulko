using Ulko.Battle;
using Ulko.Data.Abilities;
using HotChocolate.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;
using UnityEngine.UIElements;

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

            var abilities = new HashSet<AbilityAsset>();
            foreach (var action in possibleActions)
            {
                if(!action.isCardThrow)
                    abilities.Add(action.ability);
            }

            var actorIds = new HashSet<string>();
            foreach (var action in possibleActions)
            {
                actorIds.Add(action.actorId);
            }

            foreach(var actorId in actorIds)
            {
                var owner = data.battleInstance.FindCharacter(actorId);

                foreach (var ability in abilities)
                {
                    var actions = possibleActions.Where(a => a.ability.id == ability.id);

                    var abilityCard = handOfCardsView.AddCard(abilityPrefab);
                    abilityCard.Init(ability, owner, actions);

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
            var targets = data.battleInstance.FindCharacters(action.targetIds);

            cardView.transform.SetParent(cardThrowParent);
            cardView.transform.localPosition = Vector3.zero;

            float elapsed = 0;
            while (elapsed < 0.5f)
            {
                var headPos = targets[0].GetComponentInChildren<HeadAnchor>().transform.position;
                Vector2 viewportPoint = data.gameState.Camera.WorldToViewportPoint(headPos);

                cardView.transform.localScale -= Vector3.one * Time.deltaTime;

                cardView.GetComponent<RectTransform>().anchorMin = viewportPoint;
                cardView.GetComponent<RectTransform>().anchorMax = viewportPoint;

                elapsed += Time.deltaTime;

                yield return null;
            }

            data.playerAction.DeclareAction(action);
        }
    }
}
