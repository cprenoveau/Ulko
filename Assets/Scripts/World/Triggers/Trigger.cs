using UnityEngine;

namespace Ulko.World
{
    [RequireComponent(typeof(BoxCollider))]
    public abstract class Trigger : MonoBehaviour
    {
        public bool CosmeticOnly { get; protected set; }

        protected Color gizmoColorLeft = Color.yellow;
        protected Color gizmoColorRight = Color.yellow;
        protected Color gizmoColorDown = Color.yellow;
        protected Color gizmoColorUp = Color.yellow;

        public abstract void OnEnter(Player player);
        public abstract void OnExit(Player player);

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        protected virtual void OnDrawGizmos()
        {
            var collider = GetComponent<BoxCollider>();

            Vector3 bottomLeft = new Vector3(collider.bounds.min.x, transform.position.y, collider.bounds.min.z);
            Vector3 bottomRight = bottomLeft + new Vector3(collider.bounds.size.x, 0f, 0f);
            Vector3 topRight = bottomRight + new Vector3(0f, 0f, collider.bounds.size.z);
            Vector3 topLeft = topRight + new Vector3(-collider.bounds.size.x, 0f, 0f);

            Debug.DrawLine(bottomLeft, bottomRight, gizmoColorDown);
            Debug.DrawLine(bottomRight, topRight, gizmoColorRight);
            Debug.DrawLine(topRight, topLeft, gizmoColorUp);
            Debug.DrawLine(topLeft, bottomLeft, gizmoColorLeft);
        }
    }
}
