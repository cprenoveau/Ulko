using System.Collections;
using UnityEngine;

namespace Ulko.UI
{
    public class MoveTween : MonoBehaviour
    {
        public bool playOnStart;
        public bool useUnscaledTime = true;
        public MoveTweenConfig config;

        private Vector2 originalPos;
        private void Awake()
        {
            originalPos = GetComponent<RectTransform>().anchoredPosition;
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
            UpdatePosition(originalPos, 1f);
        }

        public IEnumerator PlayAsync()
        {
            var inAnimation = new HotChocolate.Motion.Tween<Vector2>(
                config.inDuration,
                originalPos,
                originalPos + config.endPositionOffset,
                Vector2.Lerp,
                HotChocolate.Motion.EasingUtil.EasingFunction(config.inEasing));

            inAnimation.OnUpdate += UpdatePosition;

            while(inAnimation.Play(useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime))
            {
                yield return null;
            }

            var backAnimation = new HotChocolate.Motion.Tween<Vector2>(
                config.backDuration,
                originalPos + config.endPositionOffset,
                originalPos + config.startPositionOffset,
                Vector2.Lerp,
                HotChocolate.Motion.EasingUtil.EasingFunction(config.backEasing));

            backAnimation.OnUpdate += UpdatePosition;

            var forthAnimation = new HotChocolate.Motion.Tween<Vector2>(
                config.forthDuration,
                originalPos + config.startPositionOffset,
                originalPos + config.endPositionOffset,
                Vector2.Lerp,
                HotChocolate.Motion.EasingUtil.EasingFunction(config.forthEasing));

            forthAnimation.OnUpdate += UpdatePosition;

            var clip = new HotChocolate.Motion.ClipSequence();
            clip.Append(backAnimation);
            clip.Append(forthAnimation);

            while(true)
            {
                while (clip.Play(useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime))
                {
                    yield return null;
                }

                clip.Seek(0);
            }
        }

        private void UpdatePosition(Vector2 localPos, float progress)
        {
            GetComponent<RectTransform>().anchoredPosition = localPos;
        }
    }
}
