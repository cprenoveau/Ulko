using Ulko.Data.Abilities;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ulko.Battle
{
    public class PlayerAction
    {
        public delegate void ActionSelected();
        public event ActionSelected OnActionSelected;

        public List<BattleActions> PossibleActions { get; private set; }
        public BattleActions SelectedAction { get; private set; }

        public PlayerAction(List<BattleActions> possibleActions)
        {
            PossibleActions = possibleActions;
        }

        public void DeclareAction(BattleActions selectedAction)
        {
            SelectedAction = selectedAction;
            OnActionSelected?.Invoke();
        }

        public PlayerActionAwaiter GetAwaiter()
        {
            return new PlayerActionAwaiter(this);
        }

        public static List<BattleActions> GetPossibleActions(BattleInstance instance)
        {
            var actions = new List<BattleActions>();
            var characters = instance.CaptureCharacterStates();

            foreach(var hero in instance.GetHeroes(BattleInstance.FetchCondition.AliveOnly))
            {
                var ability = hero.Attack;
                var targetCandidates = instance.GetTargetCandidates(ability.AbilityTarget, hero.CaptureState());

                if (ability.AbilityTarget.targetSize == AbilityTarget.TargetSize.One)
                {
                    foreach (var target in targetCandidates)
                    {
                        actions.Add(new BattleActions(ability, hero.Id, new List<string> { target.Id }, characters));
                    }
                }
                else
                {
                    var enemies = targetCandidates.Where(c => c.CharacterSide == CharacterSide.Enemies).ToList();
                    var heroes = targetCandidates.Where(c => c.CharacterSide == CharacterSide.Heroes).ToList();

                    if (enemies.Count > 0)
                    {
                        actions.Add(new BattleActions(ability, hero.Id, enemies.Select(e => e.Id).ToList(), characters));
                    }

                    if (heroes.Count > 0)
                    {
                        actions.Add(new BattleActions(ability, hero.Id, heroes.Select(e => e.Id).ToList(), characters));
                    }
                }
            }

            return actions;
        }
    }

    public class PlayerActionAwaiter : INotifyCompletion
    {
        public bool IsCompleted => playerAction.SelectedAction != null;

        private readonly PlayerAction playerAction;

        public PlayerActionAwaiter(PlayerAction playerAction)
        {
            this.playerAction = playerAction;
        }

        public void OnCompleted(System.Action continuation)
        {
            if (IsCompleted)
            {
                continuation?.Invoke();
            }
            else
            {
                playerAction.OnActionSelected += () =>
                {
                    continuation?.Invoke();
                };
            }
        }

        public PlayerAction GetResult()
        {
            return playerAction;
        }
    }
}