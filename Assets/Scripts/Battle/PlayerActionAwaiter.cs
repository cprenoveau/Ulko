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

        public List<BattleAction> PossibleActions { get; private set; }
        public BattleAction SelectedAction { get; private set; }

        public PlayerAction(List<BattleAction> possibleActions)
        {
            PossibleActions = possibleActions;
        }

        public void DeclareAction(BattleAction selectedAction)
        {
            SelectedAction = selectedAction;
            OnActionSelected?.Invoke();
        }

        public PlayerActionAwaiter GetAwaiter()
        {
            return new PlayerActionAwaiter(this);
        }

        public static List<BattleAction> GetPossibleActions(BattleInstance instance)
        {
            var actions = new List<BattleAction>();

            var characters = instance.GetAllCharacters(BattleInstance.FetchCondition.All).Select(c => c.CaptureState()).ToList();

            foreach(var hero in instance.GetHeroes(BattleInstance.FetchCondition.AliveOnly))
            {
                var action = hero.Attack;
                var targetCandidates = instance.GetTargetCandidates(action.AbilityTarget, hero.CaptureState());

                if (action.AbilityTarget.targetSize == AbilityTarget.TargetSize.One)
                {
                    foreach (var target in targetCandidates)
                    {
                        var characterAction = new CharacterAction(hero.Id, new List<string> { target.Id }, action.AbilityNodes.First().effects.effects);
                        actions.Add(new BattleAction(new ActionState(characterAction, characters), action.AbilityNodes.First().applySequence));
                    }
                }
                else
                {
                    var enemies = targetCandidates.Where(c => c.CharacterSide == CharacterSide.Enemies).ToList();
                    var heroes = targetCandidates.Where(c => c.CharacterSide == CharacterSide.Heroes).ToList();

                    if (enemies.Count > 0)
                    {
                        var characterAction = new CharacterAction(hero.Id, enemies.Select(e => e.Id).ToList(), action.AbilityNodes.First().effects.effects);
                        actions.Add(new BattleAction(new ActionState(characterAction, characters), action.AbilityNodes.First().applySequence));
                    }

                    if (heroes.Count > 0)
                    {
                        var characterAction = new CharacterAction(hero.Id, heroes.Select(e => e.Id).ToList(), action.AbilityNodes.First().effects.effects);
                        actions.Add(new BattleAction(new ActionState(characterAction, characters), action.AbilityNodes.First().applySequence));
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