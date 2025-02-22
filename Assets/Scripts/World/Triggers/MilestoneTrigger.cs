using System;
using UnityEngine;

namespace Ulko.World
{
    public class MilestoneTrigger : Trigger
    {
        public enum Side
        {
            Left,
            Right,
            Bottom,
            Top
        }

        public bool triggerOnEnter;
        public Side exitSide;

        internal static event Action OnTrigger;

        public override void OnEnter(Player player)
        {
            if (triggerOnEnter)
                OnTrigger?.Invoke();
        }

        public override void OnExit(Player player)
        {
            if (!triggerOnEnter && OnExitSide(player))
            {
                OnTrigger?.Invoke();
            }
        }

        private bool OnExitSide(Player player)
        {
            var collider = GetComponent<BoxCollider>();

            switch (exitSide)
            {
                case Side.Left:
                    return player.transform.position.x <= collider.bounds.min.x;

                case Side.Right:
                    return player.transform.position.x >= collider.bounds.max.x;

                case Side.Bottom:
                    return player.transform.position.z <= collider.bounds.min.z;

                case Side.Top:
                    return player.transform.position.z >= collider.bounds.max.z;
            }

            return false;
        }

        protected override void OnDrawGizmos()
        {
            gizmoColorDown = exitSide == Side.Bottom ? Color.blue : Color.yellow;
            gizmoColorRight = exitSide == Side.Right ? Color.blue : Color.yellow;
            gizmoColorUp = exitSide == Side.Top ? Color.blue : Color.yellow;
            gizmoColorLeft = exitSide == Side.Left ? Color.blue : Color.yellow;

            base.OnDrawGizmos();
        }
    }
}