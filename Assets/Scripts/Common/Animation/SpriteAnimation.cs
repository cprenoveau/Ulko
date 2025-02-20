using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko
{
    [CreateAssetMenu(fileName = "SpriteAnimation", menuName = "Ulko/Animation/Sprite Animation", order = 1)]
    public class SpriteAnimation : ScriptableObject
    {
        public float fps = 8;
        public List<Sprite> frames = new List<Sprite>();
        public SpriteFlash flash;
        public SpriteFade fade;

        public int CurrentFrameIndex { get; private set; }

        public void SetFirstFrame(SpriteRenderer renderer)
        {
            renderer.sprite = frames[0];
            CurrentFrameIndex = 0;
        }

        public IEnumerator Play(MonoBehaviour holder, SpriteRenderer renderer, bool loop, float speed = 1f, float duration = float.PositiveInfinity, Action onComplete = null)
        {
            Coroutine flashCoroutine = null;
            if (flash != null)
                flashCoroutine = holder.StartCoroutine(flash.Play(renderer));

            Coroutine fadeCoroutine = null;
            if (fade != null)
                fadeCoroutine = holder.StartCoroutine(fade.Play(renderer));

            CurrentFrameIndex = 0;
            float elapsed = 0;

            while(elapsed < duration)
            {
                renderer.sprite = frames[CurrentFrameIndex];
                CurrentFrameIndex = (CurrentFrameIndex + 1) % frames.Count;

                yield return new WaitForSeconds((1 / fps) / speed);

                if (!loop && CurrentFrameIndex == 0)
                    break;
            }

            if(flashCoroutine != null)
            {
                holder.StopCoroutine(flashCoroutine);
                flash.Reset(renderer);
            }

            if(fadeCoroutine != null)
            {
                holder.StopCoroutine(fadeCoroutine);
                fade.Reset(renderer);
            }

            onComplete?.Invoke();
        }

        public IEnumerator Play(Image image, bool loop, float speed = 1f, float duration = float.PositiveInfinity, Action onComplete = null)
        {
            CurrentFrameIndex = 0;
            float elapsed = 0;

            while (elapsed < duration)
            {
                image.sprite = frames[CurrentFrameIndex];
                CurrentFrameIndex = (CurrentFrameIndex + 1) % frames.Count;

                yield return new WaitForSeconds((1 / fps) / speed);

                if (!loop && CurrentFrameIndex == 0)
                    break;
            }

            onComplete?.Invoke();
        }
    }
}
