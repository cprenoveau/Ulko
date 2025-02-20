using HotChocolate.Motion;
using System.Collections;
using UnityEngine;

namespace Ulko
{
    [CreateAssetMenu(fileName = "SpriteFlash", menuName = "Ulko/Animation/Sprite Flash", order = 1)]
    public class SpriteFlash : ScriptableObject
    {
        public float duration = 1f;
        public bool loop;
        public float startValue = 1f;
        public float endValue = 0f;
        public EasingType easing;

        public void Reset(SpriteRenderer renderer)
        {
            UpdateFlash(renderer, 0, 0);
        }

        public IEnumerator Play(SpriteRenderer renderer)
        {
            var sequence = new ClipSequence();

            var inAnim = new Tween<float>(duration, startValue, endValue, Mathf.Lerp, EasingUtil.EasingFunction(easing));
            inAnim.OnUpdate += (float v, float p) => { UpdateFlash(renderer, v, p); };
            sequence.Append(inAnim);

            if(loop)
            {
                var outAnim = new Tween<float>(duration, endValue, startValue, Mathf.Lerp, EasingUtil.EasingFunction(easing));
                outAnim.OnUpdate += (float v, float p) => { UpdateFlash(renderer, v, p); };
                sequence.Append(outAnim);
            }

            while(true)
            {
                if(!sequence.Play(Time.deltaTime))
                {
                    if (loop) sequence.Seek(0);
                    else yield break;
                }

                yield return null;
            }
        }

        private void UpdateFlash(SpriteRenderer renderer, float value, float progress)
        {
            renderer.material.SetFloat("_FlashAmount", value);
        }
    }
}
