using TMPro;
using Ulko.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class HudStatView : MonoBehaviour
    {
        public UiConfig config;
        public Image icon;
        public TMP_Text valueText;

        public void Init(Stat stat, Level currentStats, Level originalStats)
        {
            int currentValue = (int)currentStats.GetStat(stat);
            int originalValue = (int)originalStats.GetStat(stat);

            valueText.text = currentValue.ToString();
            icon.sprite = config.FindStatIcon(stat);
            icon.color = config.FindStatColor(stat);

            valueText.color = currentValue < originalValue ? Color.red : Color.white;  
        }
    }
}
