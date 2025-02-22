using HotChocolate.Motion;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko
{
    public class SpeechBubble : MonoBehaviour
    {
        public List<string> speakerIds = new();
        public GameObject bubble;
        public float bounceDuration = 0.3f;
        public Vector3 scaleStart = new(0.2f, 0.2f, 0.2f);
        public Vector3 scaleMax = new(1.2f, 1.2f, 1.2f);

        private ClipSequence bounce;

        private void Awake()
        {
            Hide();
        }

        public bool HasSpeakerId(string speakerId) => speakerIds.Contains(speakerId);

        public void Show()
        {
            UpdateScale(scaleStart, 0);
            bubble.SetActive(true);

            bounce = new ClipSequence();

            var inAnimation = new Tween<Vector3>(bounceDuration / 2f, scaleStart, scaleMax, Vector3.Lerp, Easing.SineEaseInOut);
            inAnimation.OnUpdate += UpdateScale;

            var settleAnimation = new Tween<Vector3>(bounceDuration / 2f, scaleMax, Vector3.one, Vector3.Lerp, Easing.SineEaseInOut);
            settleAnimation.OnUpdate += UpdateScale;

            bounce.Append(inAnimation);
            bounce.Append(settleAnimation);
        }

        private void Update()
        {
            if(bounce != null && !bounce.Play(Time.deltaTime))
            {
                bounce = null;
            }
        }

        private void UpdateScale(Vector3 scale, float progress)
        {
            bubble.transform.localScale = scale;
        }

        public void Hide()
        {
            bounce = null;

            if(bubble != null)
                bubble.SetActive(false);
        }
    }
}