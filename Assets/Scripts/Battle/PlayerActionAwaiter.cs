using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ulko.Battle
{
    public class PlayerAction
    {
        public delegate void ActionSelected(BattleActions action);
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
            OnActionSelected?.Invoke(selectedAction);
        }

        public PlayerActionAwaiter GetAwaiter()
        {
            return new PlayerActionAwaiter(this);
        }

        public static List<BattleActions> GetPossibleActions(BattleInstance instance)
        {
            return instance.GetPossibleActions(instance.GetHeroes(BattleInstance.FetchCondition.AliveOnly));
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
                playerAction.OnActionSelected += (BattleActions actions) =>
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