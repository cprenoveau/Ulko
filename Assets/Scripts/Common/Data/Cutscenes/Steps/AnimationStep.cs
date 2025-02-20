using System.Collections;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class AnimationStep : StepAction
    {
        public SpriteRenderer actor;
        public SpriteAnimation anim;
        public bool loop;
        public float speed = 1f;
        public float duration = float.PositiveInfinity;

        public override IEnumerator Play()
        {
            yield return anim.Play(this, actor, loop, speed, duration);
        }
    }
}
