using Ulko.Battle;
using Ulko.Data.Characters;
using UnityEngine;

namespace Ulko.UI
{
    public class BattleHeroView : MonoBehaviour
    {
        public HeroView heroView;
        public GameObject selectedObject;
        public GameObject unselectedObject;

        public Character Hero { get; private set; }

        public void Init(Character hero, bool selected)
        {
            Hero = hero;

            heroView.Init(hero.Asset as HeroAsset);
            Select(selected);
        }

        public void Select(bool selected)
        {
            selectedObject.SetActive(selected);
            unselectedObject.SetActive(!selected);
        }
    }
}
