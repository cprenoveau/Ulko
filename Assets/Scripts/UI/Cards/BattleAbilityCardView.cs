using Ulko.Battle;
using Ulko.Data.Abilities;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class BattleAbilityCardView : CardView
    {
        public AbilityView abilityView;
        public Image deadIcon;

        public int CardIndex { get; private set; }
        public IEnumerable<BattleActions> Actions { get; private set; } = new List<BattleActions>();

        public event Action<CardView> OnThrow;
        public bool ThrowSelected => SelectedExtraButton == extraButtons[0];

        public void Init(int cardIndex, AbilityAsset abilityAsset, Character owner, IEnumerable<BattleActions> actions)
        {
            CardIndex = cardIndex;
            Actions = actions;

            abilityView.Init(abilityAsset, owner.CaptureState());

            deadIcon.gameObject.SetActive(owner.IsDead);

            extraButtons[0].onClick.RemoveAllListeners();
            extraButtons[0].onClick.AddListener(() => { extraButtons[0].SuperSelect(true); OnThrow?.Invoke(this); });
        }
    }
}