using UnityEngine;

namespace Ulko.World
{
    public class DoorTrigger : Trigger
    {
        public GameObject closed;
        public GameObject opened;

        private void Awake()
        {
            closed.SetActive(true);
            opened.SetActive(false);

            CosmeticOnly = true;
        }

        public override void OnEnter(Player player)
        {
            closed.SetActive(false);
            opened.SetActive(true);
        }

        public override void OnExit(Player player)
        {
            closed.SetActive(true);
            opened.SetActive(false);
        }

        protected override void OnDrawGizmos()
        {
            gizmoColorDown = gizmoColorLeft = gizmoColorRight = gizmoColorUp = Color.blue;
            base.OnDrawGizmos();
        }
    }
}
