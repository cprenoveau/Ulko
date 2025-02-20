using UnityEngine;

namespace Ulko
{
    public class Line : MonoBehaviour
    {
        public Transform secondPoint;

        private void OnDrawGizmos()
        {
            if(secondPoint != null)
                Debug.DrawLine(transform.position, secondPoint.position, Color.blue);
        }

        public Vector3 Lerp(float t)
        {
            var v = (secondPoint.position - transform.position);
            return transform.position + v.normalized * v.magnitude * t;
        }
    }
}
