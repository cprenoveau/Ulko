using System.Collections;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class RotateStep : StepAction
    {
        public Transform actor;
        public Vector3 toRotation;
        public float speed = 180f;

        public override IEnumerator Play()
        {
            Vector3 startRot = actor.transform.eulerAngles;
            Vector3 v = (toRotation - startRot);
            float duration = v.magnitude / speed;

            float elapsed = 0;
            while(elapsed <= duration)
            {
                elapsed += Time.deltaTime;
                actor.transform.eulerAngles = Vector3.Lerp(startRot, toRotation, Mathf.Clamp01(elapsed / duration));

                yield return null;
            }
        }
    }
}
