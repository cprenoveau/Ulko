﻿using System;

namespace Ulko.World
{
    public class SavePoint : Interactable
    {
        internal static event Action<SavePoint> OnInteract;

        public override bool CanInteract => true;
        public override void Interact(Player player)
        {
            OnInteract?.Invoke(this);
        }
    }
}
