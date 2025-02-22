using UnityEngine;

namespace Ulko.World
{
    [RequireComponent(typeof(BoxCollider))]
    public class NpcWanderer : MonoBehaviour, INpcBehaviour
    {
        public Rigidbody rigidBody;
        public CharacterAnimation character;
        public Limits limits;
        public float speed = 2f;
        public float changeDirectionMinTime = 1f;
        public float changeDirectionMaxTime = 2f;
        public float walkMinTime = 4f;
        public float walkMaxTime = 10f;

        private enum State
        {
            Idle,
            WalkDown,
            WalkLeft,
            WalkRight,
            WalkUp
        }

        private State state;
        private Vector2 direction;
        private float walkTime;
        private float changeDirectionAtTime;
        private float idleAtTime;

        public void Pause()
        {
            character.Walk(Vector2.zero);
            enabled = false;
        }

        public void Resume()
        {
            enabled = true;
        }

        private void OnEnable()
        {
            character.Stand(direction);
            character.Walk(direction);
        }

        private void FixedUpdate()
        {
            walkTime += Time.fixedDeltaTime;

            if (walkTime >= idleAtTime)
            {
                walkTime = 0;
                idleAtTime = float.MaxValue;
                changeDirectionAtTime = Random.Range(changeDirectionMinTime, changeDirectionMaxTime);
                state = State.Idle;
            }
            else if (walkTime >= changeDirectionAtTime)
            {
                state = (State)Random.Range(1, 5);
                walkTime = 0;
                changeDirectionAtTime = float.MaxValue;
            }
            else if (walkTime >= changeDirectionMinTime)
            {
                changeDirectionAtTime = Random.Range(changeDirectionMinTime, changeDirectionMaxTime);
                idleAtTime = Random.Range(walkMinTime, walkMaxTime);
            }

            switch (state)
            {
                case State.Idle:
                    direction = Vector2.zero;
                    break;

                case State.WalkDown:
                    direction = new Vector2(0, -1);
                    break;

                case State.WalkLeft:
                    direction = new Vector2(-1, 0);
                    break;

                case State.WalkRight:
                    direction = new Vector2(1, 0);
                    break;

                case State.WalkUp:
                    direction = new Vector2(0, 1);
                    break;
            }

            if (state != State.Idle)
            {
                Vector3 futurePosition = rigidBody.position + (new Vector3(direction.x, 0, direction.y) * speed * Time.fixedDeltaTime);
                if ((limits == null || limits.IsIn(futurePosition)) && !RaycastHit())
                {
                    rigidBody.MovePosition(futurePosition);
                }
                else
                {
                    direction = Vector2.zero;
                }
            }

            character.Walk(direction);
        }

        private bool RaycastHit()
        {
            Vector3[] raycastOrigins = new Vector3[3];
            var collider = GetComponent<BoxCollider>();

            raycastOrigins[0] = collider.bounds.center;
            raycastOrigins[0].x += direction.x * (collider.size.x / 2f + 0.001f);
            raycastOrigins[0].z += direction.y * (collider.size.z / 2f + 0.001f);
            raycastOrigins[2] = raycastOrigins[1] = raycastOrigins[0];

            if (direction.x == 0)
            {
                raycastOrigins[1].x += collider.size.x / 2f + 0.01f;
                raycastOrigins[2].x -= collider.size.x / 2f + 0.01f;
            }
            else
            {
                raycastOrigins[1].z += collider.size.z / 2f + 0.01f;
                raycastOrigins[2].z -= collider.size.z / 2f + 0.01f;
            }

            foreach (var origin in raycastOrigins)
            {
                Debug.DrawRay(origin, new Vector3(direction.x, 0, direction.y) * 0.5f, Color.red);
            }

            foreach (var origin in raycastOrigins)
            {
                var hits = Physics.RaycastAll(origin, new Vector3(direction.x, 0, direction.y), 0.5f, ~LayerMask.GetMask("Npc", "Ghost"));
                if (hits.Length > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
