using System.Collections;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class MoveObjectStep : StepAction
    {
        public Transform actor;
        public Transform toPosition;
        public float speed = 3f;

        public override IEnumerator Play()
        {
            Vector3 startPos = actor.transform.position;
            Vector3 v = (toPosition.position - startPos);
            float duration = v.magnitude / speed;

            float elapsed = 0;
            while (elapsed <= duration)
            {
                elapsed += Time.deltaTime;
                actor.transform.position = Vector3.Lerp(startPos, toPosition.position, Mathf.Clamp01(elapsed / duration));

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
