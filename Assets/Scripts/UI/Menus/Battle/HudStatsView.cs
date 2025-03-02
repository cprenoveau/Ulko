using System;
using Ulko.Data;
using Ulko.Data.Abilities;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class HudStatsView : MonoBehaviour
    {
        public Slider hpSlider;
        public RectTransform statsParent;
        public HudStatView statPrefab;

        private Transform worldParent;
        private Camera worldCamera;

        public void Init(CharacterState currentState, Level originalStats, Transform worldParent, Camera worldCamera)
        {
            this.worldParent = worldParent;
            this.worldCamera = worldCamera;

            hpSlider.maxValue = currentState.stats.maxHP;
            hpSlider.value = currentState.hp;

            foreach(Transform child in statsParent)
            {
                Destroy(child.gameObject);
            }

            foreach(Stat stat in Enum.GetValues(typeof(Stat)))
            {
                var statInstance = Instantiate(statPrefab, statsParent);
                statInstance.Init(stat, currentState.stats, originalStats);
            }

            UpdatePosition();
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
