using System.Collections;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class PlayCharacterWalkStep : StepAction
    {
        public CharacterAnimation actor;
        public Transform toPosition;
        public float speed = 3f;
        public float animSpeed = 1f;
        public bool flip;

        public override IEnumerator Play()
        {
            var t = actor.spriteRenderer.transform;

            if (flip)
                t.eulerAngles = new Vector3(t.eulerAngles.x, 180, t.eulerAngles.z);
            else
                t.eulerAngles = new Vector3(t.eulerAngles.x, 0, t.eulerAngles.z);

            Vector2 dir = actor.CurrentDirection;
            while (true)
            {
                Vector3 v = (toPosition.position - actor.transform.position);
                var d = v.normalized;

                if(d.x != 0 || d.z != 0)
                    dir = new Vector2(d.x, d.z);

                actor.transform.position += d * speed * Time.deltaTime;

                actor.Walk(dir, animSpeed);

                if (speed * Time.deltaTime >= v.magnitude)
                {
                    actor.transform.position = toPosition.position;
                    actor.Stand(dir, animSpeed);

                    yield break;
                }

                yield return null;
            }
        }

        private void OnDrawGizmos()
        {
            if (toPosition != null)
            {
                Vector3 center = toPosition.position;
                Debug.DrawLine(center - Vector3.right * 0.5f, center + Vector3.right * 0.5f, Color.blue);
                Debug.DrawLine(center - Vector3.up * 0.5f, center + Vector3.up * 0.5f, Color.blue);
            }
        }
    }
}
