using System;
using TMPro;
using UnityEngine;

namespace Ulko.UI
{
    public class GameFileView : MonoBehaviour
    {
        public TMP_Text lvlText;
        public TMP_Text timeText;
        public TMP_Text locationText;
        public TMP_Text dateText;
        public ImageView portraitPrefab;
        public RectTransform portraitsParent;
        public GrayscaleConfig inactiveGrayscale;

        public Persistence.GameFile GameFile { get; private set; }
        public DateTime SaveDate { get; private set; }

        private void Awake()
        {
            Localization.LocaleChanged += RefreshText;
        }

        private void OnDestroy()
        {
            Localization.LocaleChanged -= RefreshText;
        }

        public void Init(Persistence.GameFile gameFile, DateTime saveDate)
        {
            GameFile = gameFile;
            SaveDate = saveDate;

            RefreshText();
            InstantiatePortraits();
        }

        private void RefreshText()
        {
            int highestLevel = 0;
            foreach(var hero in GameFile.party)
            {
                var heroData = Database.Heroes[hero.id];
                var levelData = heroData.GetLevelDataFromExp(hero.exp);

                if (levelData.Lvl > highestLevel)
                    highestLevel = levelData.Lvl;
            }

            lvlText.text = Localization.LocalizeFormat("level_value", highestLevel);
            timeText.text = TextFormat.Time(GameFile.playTime);
            locationText.text = Localization.Localize("locations", GameFile.location.area);
            dateText.text = TextFormat.Date(SaveDate);
        }

        private void InstantiatePortraits()
        {
            foreach(Transform child in portraitsParent)
            {
                Destroy(child.gameObject);
            }

            var progression = GameFile.stories.ContainsKey(GameFile.currentStory) ? GameFile.stories[GameFile.currentStory] : new Persistence.Progression();

            for (int i = GameFile.party.Count-1; i >= 0; --i)
            {
                var hero = GameFile.party[i];
                var heroRef = PlayerProfile.FindHero(GameFile.currentStory, progression, hero.id);

                if (heroRef == null)
                    continue;

                var instance = Instantiate(portraitPrefab, portraitsParent);
                instance.image.sprite = heroRef.portrait;

                if (!hero.isActive) inactiveGrayscale.Set(instance.image);
                else inactiveGrayscale.Reset(instance.image);
            }
        }
    }
}
