using Ulko.Battle;
using Ulko.Data.Abilities;
using HotChocolate.UI;
using HotChocolate.Utils;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Ulko.UI
{
    public class BattleTargetMenuData
    {
        public IGameState gameState;
        public UIRoot uiRoot;
        public BattleInstance battleInstance;
        public Character cardOwner;
        public IEnumerable<BattleActions> possibleActions;

        public delegate void OnTargetSelected(BattleActions action);
        public OnTargetSelected onTargetSelected;
        public OnTargetSelected onTargetConfirmed;
    }

    public class BattleTargetMenu : Menu
    {
        public PointedButton buttonPrefab;
        public PointedButton enemiesButton;
        public PointedButton heroesButton;

        public RectTransform arrowPrefab;
        public RectTransform arrowParent;

        private MenuStack stack;
        private BattleTargetMenuData data;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as BattleTargetMenuData;

            Refresh();
        }

        protected override void _OnPop() { }
        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        private void Refresh()
        {
            enemiesButton.interactable = false;
            heroesButton.interactable = false;

            if (data.possibleActions.Count() == 0)
            {
                data.uiRoot.SetInfo(Localization.Localize("no_valid_target"));
                return;
            }

            if(data.cardOwner != null && data.cardOwner.IsDead)
            {
                data.uiRoot.SetInfo(Localization.Localize("owner_is_dead"));
                return;
            }

            for (int i = 0; i < data.possibleActions.Count(); ++i)
            {
                var possibleAction = data.possibleActions.ElementAt(i);

                var targets = data.battleInstance.FindCharacters(possibleAction.targetIds);
                if (targets.Count == 0)
                    continue;

                if(possibleAction.ability.target.targetSize == AbilityTarget.TargetSize.Group)
                {
                    if (targets[0].CharacterSide == CharacterSide.Enemies)
                    {
                        enemiesButton.interactable = true;

                        enemiesButton.OnSelected += () => { SelectTarget(possibleAction, targets); };
                        enemiesButton.onClick.AddListener(() => { data.onTargetConfirmed?.Invoke(possibleAction); });

                        if (possibleAction.ability.target.targetType == AbilityTarget.TargetType.Enemies)
                        {
                            Select(enemiesButton.gameObject);
                        }
                    }
                    else
                    {
                        heroesButton.interactable = true;

                        heroesButton.OnSelected += () => { SelectTarget(possibleAction, targets); };
                        heroesButton.onClick.AddListener(() => { data.onTargetConfirmed?.Invoke(possibleAction); });

                        if (possibleAction.ability.target.targetType == AbilityTarget.TargetType.Allies)
                        {
                            Select(heroesButton.gameObject);
                        }
                    }
                }
                else
                {
                    var candidate = targets[0];
                    var button = Instantiate(buttonPrefab, transform);

                    var modelVertices = Positions.GetVertices(candidate.CharacterInstance.spriteRenderer.bounds);
                    var (min, max) = Positions.FindViewMinMax(modelVertices, data.gameState.Camera);

                    button.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    button.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

                    button.GetComponent<RectTransform>().anchorMin = min;
                    button.GetComponent<RectTransform>().anchorMax = max;

                    button.OnSelected += () => { SelectTarget(possibleAction, targets); };
                    button.onClick.AddListener(() => { data.onTargetConfirmed?.Invoke(possibleAction); });

                    if (i == 0)
                    {
                        Select(button.gameObject);
                    }
                }
            }
        }

        private void SelectTarget(BattleActions action, List<Character> targets)
        {
            foreach (Transform child in arrowParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var target in targets)
            {
                var instance = Instantiate(arrowPrefab, arrowParent);

                var arrowPos = target.GetComponentInChildren<ArrowAnchor>().transform.position;
                Vector2 viewportPoint = data.gameState.Camera.WorldToViewportPoint(arrowPos);
                instance.anchorMin = viewportPoint;
                instance.anchorMax = viewportPoint;
            }

            if (targets.Count == 1)
            {
                data.uiRoot.SetInfo(targets[0].Description());
            }
            else
            {
                if (targets[0].CharacterSide == CharacterSide.Heroes)
                {
                    data.uiRoot.SetInfo(Localization.Localize("heroes"));
                }
                else
                {
                    data.uiRoot.SetInfo(Localization.Localize("enemies"));
                }
            }

            data.onTargetSelected?.Invoke(action);
        }
    }
}
