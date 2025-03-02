using Ulko.Battle;
using Ulko.Data.Abilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace Ulko.UI
{
    public class AbilityView : MonoBehaviour
    {
        public UiConfig config;
        public TMP_Text nameText;
        public Image abilityTypeIcon;
        public TMP_Text ownerNameText;
        public EffectsView effectsView;

        public AbilityAsset AbilityAsset { get; private set; }
        public Character Owner { get; private set; }

        private void Start()
        {
            Localization.LocaleChanged += Refresh;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= Refresh;
        }

        public void Init(AbilityAsset abilityAsset, Character owner)
        {
            AbilityAsset = abilityAsset;
            Owner = owner;

            var effects = new List<Effect>();
            foreach (var node in AbilityAsset.nodes)
            {
                effects.AddRange(node.effects.effects);
            }

            effectsView.Init(effects, owner, abilityAsset.target.targetSize);

            Refresh();
        }

        private void Refresh()
        {
            nameText.text = Localization.Localize(AbilityAsset.id);
            ownerNameText.text = Owner.Name;

            abilityTypeIcon.sprite = config.FindStatIcon(AbilityAsset.mainStat);
            abilityTypeIcon.color = config.FindStatColor(AbilityAsset.mainStat);
        }
    }
}