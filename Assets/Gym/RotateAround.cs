using UnityEngine;

namespace Ulko
{
    public class RotateAround : MonoBehaviour
    {
        public float speed = 5f;
        public float radius = 1f;
        public Transform target;
        public Vector3 offset;

        private void Update()
        {
            float x = radius * Mathf.Cos(Time.time * speed);
            float z = radius * Mathf.Sin(Time.time * speed);

            transform.position = target.position + offset + new Vector3(x, 0, z);
        }
    }
}