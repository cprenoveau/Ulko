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
            float shield = hero.Stats.GetStat(Stat.Shield);
            if(shield > 0)
            {
                var statInstance = Instantiate(statPrefab, statsParent);
                statInstance.Init(Stat.Shield, characterState.stats, hero.Stats);
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
