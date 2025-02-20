using UnityEngine;

namespace Ulko
{
    public class AudioSourceHandle : MonoBehaviour
    {
        public AudioSource audioSource;

        private HotChocolate.Motion.Tween<float> fadeTween;

        public void Play(float volume, float fadeInDuration = 2f)
        {
            FadeIn(volume, fadeInDuration);
        }

        public void Stop(float fadeOutDuration = 2f)
        {
            FadeOut(fadeOutDuration);
        }

        private void Update()
        {
            if(fadeTween != null && !fadeTween.Play(Time.deltaTime))
            {
                fadeTween = null;
            }
        }

        private void FadeIn(float volume, float duration)
        {
            if(!audioSource.isPlaying)
                audioSource.Play();

            if (duration > 0)
            {
                fadeTween = new HotChocolate.Motion.Tween<float>(duration, audioSource.volume, volume, Mathf.Lerp, HotChocolate.Motion.Easing.SineEaseInOut);
                fadeTween.OnUpdate += UpdateVolume;
            }
            else
            {
                fadeTween = null;
                audioSource.volume = volume;
            }
        }

        private void FadeOut(float duration)
        {
            if (duration > 0)
            {
                fadeTween = new HotChocolate.Motion.Tween<float>(duration, audioSource.volume, 0, Mathf.Lerp, HotChocolate.Motion.Easing.SineEaseInOut);
                fadeTween.OnUpdate += UpdateVolume;
                fadeTween.OnFinish += () => { audioSource.Stop(); };
            }
            else
            {
                fadeTween = null;
                audioSource.volume = 0;
            }
        }

        private void UpdateVolume(float value, float progress)
        {
            audioSource.volume = value;
        }
    }
}
