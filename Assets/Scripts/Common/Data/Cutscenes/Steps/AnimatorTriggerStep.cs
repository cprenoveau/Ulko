using System.Collections;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class AnimatorTriggerStep : StepAction
    {
        public Animator animator;
        public string trigger;
        public float duration;

        public override IEnumerator Play()
        {
            animator.SetTrigger(trigger);

            yield return new WaitForSeconds(duration);

            animator.ResetTrigger(trigger);
        }
    }
}
