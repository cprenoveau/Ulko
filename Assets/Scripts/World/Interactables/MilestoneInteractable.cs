﻿using System;

namespace Ulko.World
{
    public class MilestoneInteractable : Interactable
    {
        internal static event Action OnInteract;

        public override bool CanInteract => true;
        public override void Interact(Player player)
        {
            OnInteract?.Invoke();
        }
    }
}
