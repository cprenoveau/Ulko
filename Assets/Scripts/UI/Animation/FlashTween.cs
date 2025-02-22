using System.Collections;
using UnityEngine;

namespace Ulko.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FlashTween : MonoBehaviour
    {
        public bool playOnStart;
        public bool useUnscaledTime;
        public FlashTweenConfig config;

        private float originalAlpha;
        private void Awake()
        {
            originalAlpha = GetComponent<CanvasGroup>().alpha;
        }

        private void Start()
        {
            if (playOnStart)
                Play();
        }

        private void OnEnable()
        {
            if (playOnStart)
                Play();
        }

        public void Play()
        {
            StopAllCoroutines();
            StartCoroutine(PlayAsync());
        }

        public void Stop()
        {
            StopAllCoroutines();
            UpdateAlpha(originalAlpha, 1f);
        }

        public IEnumerator PlayAsync()
        {
            var inAnimation = new HotChocolate.Motion.Tween<float>(
                config.inDuration,
                originalAlpha,
                config.endAlpha,
                Mathf.Lerp,
                HotChocolate.Motion.EasingUtil.EasingFunction(config.inEasing));

            inAnimation.OnUpdate += UpdateAlpha;

            while (inAnimation.Play(useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime))
            {
                yield return null;
            }

            var backAnimation = new HotChocolate.Motion.Tween<float>(
                config.backDuration,
                config.endAlpha,
                config.startAlpha,
                Mathf.Lerp,
                HotChocolate.Motion.EasingUtil.EasingFunction(config.backEasing));

            backAnimation.OnUpdate += UpdateAlpha;

            var forthAnimation = new HotChocolate.Motion.Tween<float>(
                config.forthDuration,
                config.startAlpha,
                config.endAlpha,
                Mathf.Lerp,
                HotChocolate.Motion.EasingUtil.EasingFunction(config.forthEasing));

            forthAnimation.OnUpdate += UpdateAlpha;

            var clip = new HotChocolate.Motion.ClipSequence();
            clip.Append(backAnimation);
            clip.Append(forthAnimation);

            while (true)
            {
                while (clip.Play(useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime))
                {
                    yield return null;
                }

                clip.Seek(0);
            }
        }

        private void UpdateAlpha(float alpha, float progress)
        {
            GetComponent<CanvasGroup>().alpha = alpha;
        }
    }
}
