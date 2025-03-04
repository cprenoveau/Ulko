using System.Collections;
using System.Linq;
using Ulko.Data.Abilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ulko.World
{
    public class PlayerFollower : MonoBehaviour, ICharacterCosmetics
    {
        public float maxDistance = 1f;
        public Transform visualAnchor;

        public Player Target { get; private set; }
        public int Rank { get; private set; }
        public CharacterAnimation CharacterInstance { get; private set; }

        public Vector2 FacingDirection => CharacterInstance.CurrentDirection;
        public Transform Transform => transform;

        private void OnDestroy()
        {
            if (CharacterInstance != null)
            {
                Addressables.ReleaseInstance(CharacterInstance.gameObject);
            }
        }

        public void Init(Player target, int rank)
        {
            Target = target;
            Rank = rank;
            transform.position = Target.transform.position + new Vector3(0, 0, 0.2f);
        }

        public void SetCharacter(GameObject instance)
        {
            var dir = Vector2.zero;
            if (CharacterInstance != null)
            {
                dir = CharacterInstance.CurrentDirection;
                Addressables.ReleaseInstance(CharacterInstance.gameObject);
            }

            CharacterInstance = instance.GetComponent<CharacterAnimation>();
            CharacterInstance.transform.localPosition = Vector3.zero;

            CharacterInstance.Stand(dir);
        }

        private float lastGroundPos = 0;
        private bool isFalling = false;

        public void Move()
        {
            if (Target.Trace.Count < 1)
                return;

            float maxDist = maxDistance * Rank;
            float dist = 0;
            int index = 0;

            var targetPos = Target.transform.position;
            dist += ManhattanLengthNoHeight(targetPos, Target.Trace.Last());

            if (Target.Trace.Count > 1)
            {
                int i;
                for (i = Target.Trace.Count - 1; i > 0; --i)
                {
                    if (dist >= maxDist)
                    {
                        break;
                    }

                    dist += ManhattanLengthNoHeight(Target.Trace[i], Target.Trace[i - 1]);
                }

                index = i;
            }

            var newPos = Target.Trace[index];
            var nextPos = index + 1 < Target.Trace.Count ? Target.Trace[index + 1] : targetPos;

            if (dist > maxDist)
            {
                float segLength = ManhattanLengthNoHeight(nextPos, newPos);
                float distBeforeMax = dist - segLength;

                float ratio = (maxDist - distBeforeMax) / (dist - distBeforeMax);

                newPos = Vector3.Lerp(nextPos, newPos, ratio);
            }

            if (ManhattanLengthNoHeight(newPos, transform.position) < 0.01f)
            {
                CharacterInstance.Walk(Vector2.zero);
            }
            else
            {
                var realDir = newPos - transform.position;
                CharacterInstance.Walk(Normalize(new Vector2(realDir.x, realDir.z)));
            }

            float y = isFalling || IsGrounded() ? transform.position.y : newPos.y;
            transform.position = new Vector3(newPos.x, y, newPos.z);
        }

        private bool IsGrounded()
        {
            return lastGroundPos == transform.position.y;
        }

        private void Update()
        {
            lastGroundPos = GroundPosition();

            if (transform.position.y > lastGroundPos)
            {
                isFalling = true;

                float y = transform.position.y - 5f * Time.deltaTime;
                if (y <= lastGroundPos)
                {
                    y = lastGroundPos;
                    isFalling = false;
                }

                transform.position = new Vector3(transform.position.x, y, transform.position.z);
            }
            else
            {
                isFalling = false;
            }
        }

        private Vector2 Normalize(Vector2 dir)
        {
            if (dir == Vector2.zero)
                return dir;

            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                dir.y = 0;

                if (dir.x < 0) dir.x = -1;
                else dir.x = 1;
            }
            else
            {
                dir.x = 0;

                if (dir.y < 0) dir.y = -1;
                else dir.y = 1;
            }

            return dir;
        }

        private float GroundPosition()
        {
            Debug.DrawRay(transform.position + Vector3.up * 0.5f, -Vector3.up * 100f, Color.blue);

            if(Physics.Raycast(
                transform.position + Vector3.up * 0.5f,
                -Vector3.up,
                out RaycastHit hitInfo,
                100f,
                ~LayerMask.GetMask("Player", "NPC", "Ghost"),
                QueryTriggerInteraction.Ignore))
            {
                return hitInfo.point.y;
            }

            return transform.position.y;
        }

        private float ManhattanLengthNoHeight(Vector3 a, Vector3 b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
        }

        public IEnumerator PlayAnimationAsync(string id, bool loop, bool holdPose, float speed, float duration)
        {
            yield break;
        }
    }
}
