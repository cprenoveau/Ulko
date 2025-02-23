using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ulko.Battle
{
    public class PlayerAction
    {
        public delegate void ActionSelected();
        public event ActionSelected OnActionSelected;

        public bool Completed { get; private set; }

        public PlayerAction()
        {
        }

        public void DeclareAction()
        {
            Completed = true;
            OnActionSelected?.Invoke();
        }

        public PlayerActionAwaiter GetAwaiter()
        {
            return new PlayerActionAwaiter(this);
        }
    }

    public class PlayerActionAwaiter : INotifyCompletion
    {
        public bool IsCompleted => playerAction.Completed;

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