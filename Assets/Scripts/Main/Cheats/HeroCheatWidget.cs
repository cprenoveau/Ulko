using HotChocolate.Cheats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ulko.Cheats
{
    public class HeroCheatWidget : MonoBehaviour, ICustomWidget
    {
        public TMP_Text label;

        public Slider hpSlider;
        public TMP_Text hpLabel;

        public Slider levelSlider;
        public TMP_Text levelLabel;

        public Slider expSlider;
        public TMP_Text expLabel;

        public Toggle inPartyToggle;

        public event ICustomWidget.ValueChanged OnValueChanged;

        public Persistence.Hero Data { get; private set; }

        public void Init(string name, object value)
        {
            if (value == null)
                return;

            Data = value as Persistence.Hero;

            label.text = HotChocolate.Utils.TextFormat.ToDisplayName(name);

            Refresh();

            hpSlider.onValueChanged.AddListener(OnHpChanged);
            levelSlider.onValueChanged.AddListener(OnLevelChanged);
            expSlider.onValueChanged.AddListener(OnExpChanged);
            inPartyToggle.onValueChanged.AddListener(OnInPartyChanged);
        }

        private void Refresh()
        {
            hpLabel.text = "HP: " + Data.hp + "/" + PlayerProfile.GetHeroMaxHP(Data.id);

            hpSlider.maxValue = PlayerProfile.GetHeroMaxHP(Data.id);
            hpSlider.value = Data.hp;

            var hero = Database.Heroes[Data.id];
            var level = hero.GetLevelDataFromExp(Data.exp);

            levelLabel.text = "Level: " + level.level + "/" + hero.MaxLevel;

            levelSlider.minValue = 1;
            levelSlider.maxValue = hero.MaxLevel;
            levelSlider.value = level.level;

            int nextLevelExp = !hero.IsMaxLevel(level.level) ? hero.GetLevelData(level.level + 1).exp : hero.GetLevelData(level.level).exp;
            expLabel.text = "Exp: " + Data.exp + "/" + nextLevelExp;

            expSlider.minValue = hero.GetLevelData(level.level).exp;
            expSlider.maxValue = nextLevelExp;
            expSlider.value = Data.exp;

            inPartyToggle.isOn = Data.isActive;
        }

        private void OnHpChanged(float value)
        {
            Data.hp = (int)value;
            Refresh();

            OnValueChanged?.Invoke(Data);
        }

        private void OnLevelChanged(float value)
        {
            var hero = Database.Heroes[Data.id];
            var level = hero.GetLevelData((int)value);

            Data.exp = level.exp;
            Refresh();

            OnValueChanged?.Invoke(Data);
        }

        private void OnExpChanged(float value)
        {
            Data.exp = (int)value;
            Refresh();

            OnValueChanged?.Invoke(Data);
        }

        private void OnInPartyChanged(bool value)
        {
            if (value)
                PlayerProfile.AddPartyMember(Data.id);
            else
                PlayerProfile.RemovePartyMember(Data.id);

            Data.isActive = value;
            Refresh();

            OnValueChanged?.Invoke(Data);
        }
    }
}