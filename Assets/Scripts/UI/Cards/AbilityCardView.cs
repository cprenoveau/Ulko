using Ulko.Battle;
using Ulko.Data.Abilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class AbilityCardView : CardView
    {
        public UiConfig config;
        public TMP_Text nameText;
        public TMP_Text targetSizeText;
        public Image abilityTypeIcon;
        public TMP_Text ownerNameText;
        public EffectsView effectsView;

        public event Action<CardView> OnAttack;
        public bool AttackSelected => SelectedExtraButton == extraButtons[0];

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

            effectsView.Init(effects);

            extraButtons[0].onClick.RemoveAllListeners();
            extraButtons[0].onClick.AddListener(() => { extraButtons[0].SuperSelect(true); OnAttack?.Invoke(this); });

            Refresh();
        }

        private void Refresh()
        {
            nameText.text = Localization.Localize(AbilityAsset.id);
            targetSizeText.text = Localization.Localize("size_" + AbilityAsset.target.targetSize.ToString().ToLower());
            ownerNameText.text = Owner.Name;
            abilityTypeIcon.sprite = config.FindStatIcon(AbilityAsset.mainStat);
        }
    }
}