using System;
using Ulko.Data.Abilities;

namespace Ulko.World
{
    public class HealingPoint : Interactable
    {
        internal static event Action<HealingPoint> OnInteract;

        public AbilitySequence healSequence;

        public override bool CanInteract => true;
        public override void Interact(Player player)
        {
            OnInteract?.Invoke(this);
        }
    }
}
