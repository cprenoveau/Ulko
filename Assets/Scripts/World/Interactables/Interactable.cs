using UnityEngine;

namespace Ulko.World
{
    [RequireComponent(typeof(BoxCollider))]
    public abstract class Interactable : MonoBehaviour
    {
        protected Color gizmoColor = Color.red;

        public abstract bool CanInteract { get; }
        public abstract void Interact(Player player);

        protected virtual void OnDrawGizmos()
        {
            var collider = GetComponent<BoxCollider>();

            Vector3 bottomLeft = new Vector3(collider.bounds.min.x, transform.position.y, collider.bounds.min.z);
            Vector3 bottomRight = bottomLeft + new Vector3(collider.bounds.size.x, 0f, 0f);
            Vector3 topRight = bottomRight + new Vector3(0f, 0f, collider.bounds.size.z);
            Vector3 topLeft = topRight + new Vector3(-collider.bounds.size.x, 0f, 0f);

            Debug.DrawLine(bottomLeft, bottomRight, gizmoColor);
            Debug.DrawLine(bottomRight, topRight, gizmoColor);
            Debug.DrawLine(topRight, topLeft, gizmoColor);
            Debug.DrawLine(topLeft, bottomLeft, gizmoColor);
        }
    }
}
