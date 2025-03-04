using Ulko.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class StatView : MonoBehaviour
    {
        public UiConfig config;

        public TMP_Text statText;
        public TMP_Text valueText;
        public TMP_Text diffText;
        public Slider slider;
        public Slider lessDiffSlider;
        public Slider greaterDiffSlider;

        public Stat? Stat { get; private set; }
        public int Level { get; private set; }
        public int Value { get; private set; }
        public int Diff { get; private set; }

        private void Awake()
        {
            Localization.LocaleChanged += Refresh;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= Refresh;
        }

        public void Init(Stat? stat, int value, int diff)
        {
            Stat = stat;
            Value = value;
            Diff = diff;

            Refresh();
        }

        private void Refresh()
        {
            if(Stat.HasValue)
                statText.text = TextFormat.Localize(Stat.Value);

            valueText.text = Value.ToString();

            if (slider != null)
            {
                (int min, int max) = Stat.HasValue ? config.FindMinMaxStat(Stat.Value) : (0, 10);

                slider.maxValue = max - min;
                slider.value = Value - min;

                lessDiffSlider.value = 0;
                greaterDiffSlider.value = 0;

                lessDiffSlider.maxValue = slider.maxValue;
                greaterDiffSlider.maxValue = slider.maxValue;
            }

            diffText.gameObject.SetActive(Diff != 0);
            if(Diff > 0)
            {
                diffText.text = "+" + Diff;
                diffText.color = config.positiveDiffColor;
                greaterDiffSlider.value = slider.value;
                slider.value -= Diff;
            }
            else if(Diff < 0)
            {
                diffText.text = Diff.ToString();
                diffText.color = config.negativeDiffColor;
                lessDiffSlider.value = slider.value - Diff;
            }
        }
    }
}
