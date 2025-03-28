﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ulko.Battle
{
    public class PlayerAction
    {
        public delegate void ActionSelected(BattleActions action);
        public event ActionSelected OnActionSelected;

        public List<BattleActions> PossibleActions { get; set; }
        public BattleActions SelectedAction { get; private set; }

        public bool IsCancelled { get; private set; }

        public PlayerAction(List<BattleActions> possibleActions)
        {
            PossibleActions = possibleActions;
        }

        public void DeclareAction(BattleActions selectedAction)
        {
            SelectedAction = selectedAction;
            OnActionSelected?.Invoke(selectedAction);
        }

        public void CancelAction()
        {
            IsCancelled = true;
            OnActionSelected?.Invoke(null);
        }

        public PlayerActionAwaiter GetAwaiter()
        {
            return new PlayerActionAwaiter(this);
        }
    }

    public class PlayerActionAwaiter : INotifyCompletion
    {
        public bool IsCompleted => playerAction.SelectedAction != null;
        public bool IsCancelled => playerAction.IsCancelled;

        private readonly PlayerAction playerAction;

        public PlayerActionAwaiter(PlayerAction playerAction)
        {
            this.playerAction = playerAction;
        }

        public void OnCompleted(System.Action continuation)
        {
            if (IsCompleted || IsCancelled)
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