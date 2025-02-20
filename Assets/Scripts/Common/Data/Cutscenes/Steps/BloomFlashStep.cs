using HotChocolate.Motion;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Ulko.Data.Cutscenes
{
    public class BloomFlashStep : StepAction
    {
        public PostProcessProfile profile;
        public float intensity = 8f;
        public float threshold = 0.5f;
        public float duration = 2f;
        public EasingType easingType = EasingType.Linear;

        float startThreshold;
        float startIntensity;
        Bloom bloom;
        PostProcessVolume volume;

        public override IEnumerator Play()
        {
            startThreshold = profile.GetSetting<Bloom>().threshold;
            startIntensity = profile.GetSetting<Bloom>().intensity;

            bloom = ScriptableObject.CreateInstance<Bloom>();
            bloom.enabled.Override(true);
            bloom.intensity.Override(1f);
            bloom.threshold.Override(1f);

            volume = PostProcessManager.instance.QuickVolume(LayerMask.NameToLayer("PostProcess"), 100f, bloom);
            volume.isGlobal = true;

            var tween = new Tween<float>(duration, 1f, 0f, Mathf.Lerp, EasingUtil.EasingFunction(easingType));
            tween.OnUpdate += UpdateAlpha;

            while (tween.Play(Time.deltaTime))
            {
                yield return null;
            }
        }

        private void UpdateAlpha(float value, float progress)
        {
            bloom.threshold.value = Mathf.Lerp(startThreshold, threshold, value);
            bloom.intensity.value = Mathf.Lerp(startIntensity, intensity, value);
        }

        void OnDestroy()
        {
            if(volume != null)
                RuntimeUtilities.DestroyVolume(volume, true, true);
        }
    }
}
