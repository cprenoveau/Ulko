using UnityEngine;

namespace Ulko
{
    public class Limits : MonoBehaviour
    {
        public Rect rect;

        private void OnDrawGizmos()
        {
            Vector3 bottomLeft = new Vector3(rect.position.x, 0, rect.position.y) + transform.position;
            Vector3 bottomRight = bottomLeft + new Vector3(rect.width, 0f, 0f);
            Vector3 topRight = bottomRight + new Vector3(0f, 0f, rect.height);
            Vector3 topLeft = topRight + new Vector3(-rect.width, 0f, 0f);

            Debug.DrawLine(bottomLeft, bottomRight, Color.blue);
            Debug.DrawLine(bottomRight, topRight, Color.blue);
            Debug.DrawLine(topRight, topLeft, Color.blue);
            Debug.DrawLine(topLeft, bottomLeft, Color.blue);
        }

        public float Height => transform.position.y;

        public Rect Absolute()
        {
            return new Rect(
                transform.position.x + rect.x,
                transform.position.z + rect.y,
                rect.width,
                rect.height);
        }

        public bool IsIn(Vector3 point)
        {
            var limits = Absolute();

            if (point.x >= limits.x
                && point.x <= limits.x + limits.width
                && point.z >= limits.y
                && point.z <= limits.y + limits.height)
            {
                return true;
            }

            return false;
        }
    }
}
