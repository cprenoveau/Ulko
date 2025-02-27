using Ulko.Battle;
using Ulko.Data.Abilities;
using HotChocolate.UI;
using System.Collections.Generic;
using System.Linq;

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

                    abilityCard.OnAttack += OnAttackClicked;
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
                if (battleAbility.AttackSelected)
                    data.uiRoot.SetInfo(Localization.Localize("discard_desc"));
                else
                    data.uiRoot.SetInfo(battleAbility.abilityView.AbilityAsset.Description());
            }
        }

        private void OnAttackClicked(CardView cardView)
        {
            //todo
        }

        private void OnCardClicked(CardView cardView)
        {
            if (cardView is BattleAbilityCardView battleAbility)
            {
                ShowTargetMenu(
                    battleAbility.Actions,
                    null,
                    DeclareAction);

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

        private void DeclareAction(BattleActions action)
        {
            data.playerAction.DeclareAction(action);
        }
    }
}
