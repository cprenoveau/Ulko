using System;
using TMPro;
using Ulko.Data;
using Ulko.Data.Abilities;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class HudStatsView : MonoBehaviour
    {
        public TMP_Text hpText;
        public Slider hpSlider;
        public RectTransform statsParent;
        public HudStatView statPrefab;

        private CharacterState currentState;
        private Level originalStats;
        private Transform worldParent;
        private Camera worldCamera;

        private void Start()
        {
            Localization.LocaleChanged += Refresh;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= Refresh;
        }

        public void Init(CharacterState currentState, Level originalStats, Transform worldParent, Camera worldCamera)
        {
            this.currentState = currentState;
            this.originalStats = originalStats;
            this.worldParent = worldParent;
            this.worldCamera = worldCamera;

            Refresh();
            UpdatePosition();
        }

        private void Refresh()
        {
            hpText.text = Localization.LocalizeFormat("hp_value", currentState.hp, currentState.stats.maxHP);
            hpSlider.maxValue = currentState.stats.maxHP;
            hpSlider.value = currentState.hp;

            foreach (Transform child in statsParent)
            {
                Destroy(child.gameObject);
            }

            foreach (Stat stat in Enum.GetValues(typeof(Stat)))
            {
                if (originalStats.GetStat(stat) <= 0)
                    continue;

                var statInstance = Instantiate(statPrefab, statsParent);
                statInstance.Init(stat, currentState.stats, originalStats);
            }
        }

        private void UpdatePosition()
        {
            if (worldParent != null)
            {
                Vector2 viewportPoint = worldCamera.WorldToViewportPoint(worldParent.position);
                GetComponent<RectTransform>().anchorMin = viewportPoint;
                GetComponent<RectTransform>().anchorMax = viewportPoint;
            }
        }

        private void Update()
        {
            UpdatePosition();
        }
    }
}
