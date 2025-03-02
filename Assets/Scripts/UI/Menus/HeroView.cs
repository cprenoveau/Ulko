using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    public class HeroView : MonoBehaviour
    {
        public UiConfig config;

        public TMP_Text nameText;
        public TMP_Text lvlText;
        public TMP_Text expText;
        public Slider expBar;
        public TMP_Text nextLevelText;
        public TMP_Text hpText;
        public TMP_Text mpText;
        public Slider hpSlider;
        public Slider mpSlider;
        public Image portrait;
        public GrayscaleConfig inactiveGrayscale;

        public Data.Characters.HeroAsset Data { get; private set; }
        public Persistence.Hero SavedData => PlayerProfile.GetPartyMember(Data.id);

        private void Awake()
        {
            Localization.LocaleChanged += Refresh;
            PlayerProfile.OnHeroChanged += Refresh;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= Refresh;
            PlayerProfile.OnHeroChanged -= Refresh;
        }

        public void Init(Data.Characters.HeroAsset data)
        {
            Data = data;
            Refresh();
        }

        private void Refresh()
        {
            Refresh(SavedData);
        }

        private void Refresh(Persistence.Hero saved)
        {
            if (saved.id != Data.id)
                return;

            var hero = Database.Heroes[Data.id];
            int level = PlayerProfile.GetHeroLevel(saved.id);

            nameText.text = Localization.Localize(Data.displayName);
            lvlText.text = Localization.LocalizeFormat("lvl_value", level);

            int currentExp = saved.exp;
            int requiredExp = hero.IsMaxLevel(level) ? currentExp : hero.GetLevelData(level + 1).exp;

            if (expText != null)
            {
                if (!hero.IsMaxLevel(level))
                    expText.text = Localization.LocalizeFormat("exp_value", currentExp, requiredExp);
                else
                    expText.text = Localization.Localize("exp_max");
            }

            if(nextLevelText != null)
            {
                nextLevelText.gameObject.SetActive(!hero.IsMaxLevel(level));
                nextLevelText.text = Localization.LocalizeFormat("to_next_level", requiredExp - currentExp);
            }

            if (expBar != null)
            {
                expBar.minValue = hero.GetLevelData(level).exp;
                expBar.maxValue = requiredExp;
                expBar.value = currentExp;
            }

            hpText.text = Localization.LocalizeFormat("hp_value", saved.hp, PlayerProfile.GetHeroStats(saved.id).maxHP);

            hpSlider.maxValue = PlayerProfile.GetHeroStats(saved.id).maxHP;
            hpSlider.value = saved.hp;

            portrait.sprite = Data.portrait;

            if (!saved.isActive) inactiveGrayscale.Set(portrait);
            else inactiveGrayscale.Reset(portrait);
        }
    }
}
