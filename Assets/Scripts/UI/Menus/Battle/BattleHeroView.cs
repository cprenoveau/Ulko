using System;
using Ulko.Battle;
using Ulko.Data;
using Ulko.Data.Characters;
using UnityEngine;

namespace Ulko.UI
{
    public class BattleHeroView : MonoBehaviour
    {
        public HeroView heroView;
        public RectTransform statsParent;
        public HudStatView statPrefab;
        public HudStatusView statusPrefab;
        public GameObject selectedObject;
        public GameObject unselectedObject;

        public Character Hero { get; private set; }

        public void Init(Character hero, bool selected)
        {
            Hero = hero;

            heroView.Init(hero.Asset as HeroAsset);

            foreach (Transform child in statsParent)
            {
                Destroy(child.gameObject);
            }

            var characterState = hero.CaptureState();
            float shield = characterState.CurrentStats.GetStat(Stat.Shield);
            if(shield > 0)
            {
                var statInstance = Instantiate(statPrefab, statsParent);
                statInstance.Init(Stat.Shield, characterState.CurrentStats, hero.CurrentStats);
            }

            foreach(var status in hero.StatusState)
            {
                var statusInstance = Instantiate(statusPrefab, statsParent);
                statusInstance.Init(status);
            }

            Select(selected);
        }

        public void Select(bool selected)
        {
            selectedObject.SetActive(selected);
            unselectedObject.SetActive(!selected);
        }
    }
}
