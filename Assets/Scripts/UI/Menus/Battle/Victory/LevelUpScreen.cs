using Ulko.Data;
using HotChocolate.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using Ulko.Data.Abilities;
using System.Collections.Generic;

namespace Ulko.UI
{
    public class LevelUpScreenData
    {
        public IGameState gameState;
        public UIRoot uiRoot;

        public string heroId;
        public int oldLevel;
        public Action onClose;
    }

    public class LevelUpScreen : Menu
    {
        public UiConfig config;
        public MenuDefinition newAbilityPopup;

        public TMP_Text levelUpText;
        public StatView levelUpStat;

        public StatView statPrefab;
        public RectTransform statParent;
        public Button nextButton;

        private MenuStack stack;
        private LevelUpScreenData data;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        private void Start()
        {
            nextButton.onClick.AddListener(ShowNewAbilities);
            Localization.LocaleChanged += Refresh;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= Refresh;
        }

        protected override void _OnPush(MenuStack stack, object data)
        {
            this.stack = stack;
            this.data = data as LevelUpScreenData;

            Refresh();
        }

        protected override void _OnPop()
        {
            data.onClose?.Invoke();
        }

        protected override void _OnFocusIn(bool fromPush, string previousMenu) { }
        protected override void _OnFocusOut(bool fromPop, string nextMenu) { }

        private void Refresh()
        {
            foreach (Transform child in statParent.transform)
            {
                HotChocolate.UI.Menu.DisposeAllAnimations(child.gameObject);
                Destroy(child.gameObject);
            }

            levelUpText.text = Localization.LocalizeFormat("main", "level_up", Localization.Localize(data.heroId));

            int level = PlayerProfile.GetHeroLevel(data.heroId);
            levelUpStat.Init(null, level, level - data.oldLevel);

            foreach (Stat stat in Enum.GetValues(typeof(Stat)))
            {
                if (stat == Stat.Shield)
                    continue;

                var instance = Instantiate(statPrefab, statParent.transform);

                int statValue = PlayerProfile.GetHeroStat(data.heroId, stat);
                int oldStatValue = PlayerProfile.GetHeroStat(data.heroId, data.oldLevel, stat);
                instance.Init(stat, statValue, statValue - oldStatValue);
            }
        }

        public override bool CanClose()
        {
            return false;
        }

        private void ShowNewAbilities()
        {
            StartCoroutine(ShowNewAbilitiesAsync());
        }

        private IEnumerator ShowNewAbilitiesAsync()
        {
            var heroAsset = PlayerProfile.FindHero(PlayerProfile.CurrentStory, PlayerProfile.GetProgression(), data.heroId);

            var oldAbilities = heroAsset.AbilitiesForLevel(data.oldLevel);
            var newAbilities = heroAsset.Abilities;

            if (newAbilities.Count() > oldAbilities.Count())
            {
                var hero = PlayerProfile.GetPartyMember(data.heroId);
                var character = new CharacterState(data.heroId, heroAsset.displayName, hero.hp, CharacterSide.Heroes, PlayerProfile.GetHeroStats(data.heroId), new Level(), new List<StatusState>());

                foreach (var newAbilty in newAbilities)
                {
                    if (!oldAbilities.Contains(newAbilty))
                    {
                        yield return ShowNewAbilityAsync(newAbilty, character);
                    }
                }
            }

            stack.Pop();
        }

        private IEnumerator ShowNewAbilityAsync(AbilityAsset ability, CharacterState character)
        {
            stack.Push(newAbilityPopup.asset, newAbilityPopup.id, new NewAbilityPopupData
            {
                ability = ability,
                character = character
            });

            while(stack.PendingCount > 0 || stack.Top.Id != Id)
            {
                yield return null;
            }
        }
    }
}
