using HotChocolate.Motion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ulko
{
    public class AmbientFlashes : MonoBehaviour
    {
        public float minDelay = 2f;
        public float maxDelay = 10f;

        [Serializable]
        public class AmbientFlash
        {
            public float delay;
            public float duration = 2f;
            public EasingType easingType = EasingType.Linear;
            public float intensity = 0.1f;
            public float lightIntensity = 0.1f;
            public List<Light> lights = new List<Light>();

            Color startColor;
            List<float> startLightIntensity = new List<float>();

            public IEnumerator Play()
            {
                startColor = RenderSettings.ambientLight;

                startLightIntensity.Clear();
                foreach(var light in lights)
                {
                    startLightIntensity.Add(light.intensity);
                }

                yield return new WaitForSeconds(delay);

                var tween = new Tween<float>(duration, 1f, 0f, Mathf.Lerp, EasingUtil.EasingFunction(easingType));
                tween.OnUpdate += UpdateColor;

                while (tween.Play(Time.deltaTime))
                {
                    yield return null;
                }

                RenderSettings.ambientLight = startColor;

                for(int i = 0; i < lights.Count; ++i)
                {
                    lights[i].intensity = startLightIntensity[i];
                }
            }

            private void UpdateColor(float value, float progress)
            {
                RenderSettings.ambientLight = Color.Lerp(startColor, startColor + new Color(intensity, intensity, intensity), value);
                for (int i = 0; i < lights.Count; ++i)
                {
                    lights[i].intensity = Mathf.Lerp(startLightIntensity[i], startLightIntensity[i] + lightIntensity, value);
                }
            }
        }

        [Serializable]
        public class Flashes
        {
            public List<AmbientFlash> flashes = new List<AmbientFlash>();
        }

        public List<Flashes> flashes = new List<Flashes>();

        private Coroutine flashCoroutine;

        private void Update()
        {
            if (flashCoroutine != null)
                return;

            float next = UnityEngine.Random.Range(minDelay, maxDelay);

            int flashIndex = UnityEngine.Random.Range(0, flashes.Count);

            StopAllCoroutines();
            flashCoroutine = StartCoroutine(PlayAnimation(next, flashes[flashIndex].flashes));
        }

        private IEnumerator PlayAnimation(float delay, List<AmbientFlash> flashes)
        {
            yield return new WaitForSeconds(delay);

            foreach(var flash in flashes)
            {
                yield return flash.Play();
            }

            flashCoroutine = null;
        }
    }
}
