using UnityEngine;

namespace Ulko
{
    public class BirdFollowBehaviour : MonoBehaviour
    {
        public Transform target;
        public Vector3 targetOffset;
        public float maxSpeed = 10f;
        public float maxForce = 0.1f;
        public float slowDownDistance = 5f;

        private Vector3 velocity = Vector3.zero;

        private void Update()
        {
            Vector3 targetPos = target.position + targetOffset;

            float distance = (targetPos - transform.position).magnitude;
            Vector3 direction = (targetPos - transform.position).normalized;

            Vector3 desiredVelocity;
            if (distance < slowDownDistance)
            {
                desiredVelocity = (distance / slowDownDistance) * maxSpeed * Time.deltaTime * direction;
            }
            else
            {
                desiredVelocity = maxSpeed * Time.deltaTime * direction;
            }

            Vector3 steering = Vector3.ClampMagnitude(desiredVelocity - velocity, maxForce);
            velocity += steering;

            transform.transform.position += velocity;
            transform.LookAt(targetPos);
        }
    }
}