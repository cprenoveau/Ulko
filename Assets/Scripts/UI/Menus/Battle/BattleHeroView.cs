using Ulko.Battle;
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

            var info = hero.CharacterType as Hero;

            heroView.Init(info.Asset);
            Select(selected);
        }

        public void Select(bool selected)
        {
            selectedObject.SetActive(selected);
            unselectedObject.SetActive(!selected);
        }
    }
}
