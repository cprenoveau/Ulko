using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class CharacterAnimationStep : StepAction
    {
        public CharacterAnimation actor;
        public List<SpriteAnimation> animations = new();
        public bool loop;
        public float speed = 1f;
        public float duration = float.PositiveInfinity;
        public bool flip;
        public AudioDefinition playSound;
        public float soundVolume = 1f;

        public override IEnumerator Play()
        {
            var t = actor.spriteRenderer.transform;

            if (flip)
                t.eulerAngles = new Vector3(t.eulerAngles.x, 180, t.eulerAngles.z);
            else
                t.eulerAngles = new Vector3(t.eulerAngles.x, 0, t.eulerAngles.z);

            if (playSound != null)
                Audio.Player.Play(playSound, soundVolume, 0);

            actor.Play(animations, loop, speed, duration);

            if (loop && duration == float.PositiveInfinity)
                yield break;

            while (actor.IsPlaying)
                yield return null;
        }
    }
}
