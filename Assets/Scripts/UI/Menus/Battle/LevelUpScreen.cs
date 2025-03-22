using Ulko.Battle;
using Ulko.Data;
using HotChocolate.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        public TMP_Text levelUpText;
        public StatView levelUpStat;

        public StatView statPrefab;
        public RectTransform statParent;
        public Button closeButton;

        private MenuStack stack;
        private LevelUpScreenData data;

        public override HotChocolate.UI.Menu.AfterFocusOut AfterFocusOut => HotChocolate.UI.Menu.ActivateInstance;

        private void Start()
        {
            closeButton.onClick.AddListener(Close);
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

        private void Close()
        {
            stack.Pop();
        }
    }
}
