using System;
using TMPro;
using Ulko.Battle;
using Ulko.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class HudStatsView : MonoBehaviour
    {
        public TMP_Text hpText;
        public Slider hpSlider;
        public TMP_Text turnText;
        public RectTransform statsParent;
        public HudStatView statPrefab;
        public HudStatusView statusPrefab;

        private Character character;
        private Camera worldCamera;

        private void Start()
        {
            Localization.LocaleChanged += Refresh;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= Refresh;
        }

        public void Init(Character character, Camera worldCamera)
        {
            this.character = character;
            this.worldCamera = worldCamera;

            Refresh();
            UpdatePosition();
        }

        private void Refresh()
        {
            hpText.text = Localization.LocalizeFormat("hp_value", character.HP, character.Stats.maxHP);
            hpSlider.maxValue = character.Stats.maxHP;
            hpSlider.value = character.HP;
            turnText.text = character.TurnCooldown().ToString();

            foreach (Transform child in statsParent)
            {
                Destroy(child.gameObject);
            }

            var characterState = character.CaptureState();
            foreach (Stat stat in Enum.GetValues(typeof(Stat)))
            {
                if (stat == Stat.MaxHP || character.Stats.GetStat(stat) <= 0)
                    continue;

                var statInstance = Instantiate(statPrefab, statsParent);
                statInstance.Init(stat, characterState.stats, character.Stats);
            }

            foreach (var status in characterState.statuses)
            {
                if (status.statusAsset.icon == null)
                    continue;

                var statusInstance = Instantiate(statusPrefab, statsParent);
                statusInstance.Init(status);
            }
        }

        private void UpdatePosition()
        {
            if (character != null)
            {
                Vector2 viewportPoint = worldCamera.WorldToViewportPoint(character.transform.position);
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
