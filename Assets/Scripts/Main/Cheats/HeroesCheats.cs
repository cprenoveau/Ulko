using HotChocolate.Cheats;
using UnityEngine;

namespace Ulko.Cheats
{
    [CreateAssetMenu(fileName = "HeroesCheats", menuName = "Ulko/Cheats/Heroes", order = 1)]
    public class HeroesCheats : ScriptableObject
    {
        [CheatProperty(CustomWidgetAddress = "Assets/Cheats/HeroCheatWidget.prefab")]
        public Persistence.Hero Ulko
        {
            get => GetHero("ulko");
            set
            {
                PlayerProfile.UpdatePartyMember(value);
            }
        }

        [CheatProperty(CustomWidgetAddress = "Assets/Cheats/HeroCheatWidget.prefab")]
        public Persistence.Hero Phoebey
        {
            get => GetHero("phoebey");
            set
            {
                PlayerProfile.UpdatePartyMember(value);
            }
        }

        [CheatProperty(CustomWidgetAddress = "Assets/Cheats/HeroCheatWidget.prefab")]
        public Persistence.Hero Ferlin
        {
            get => GetHero("ferlin");
            set
            {
                PlayerProfile.UpdatePartyMember(value);
            }
        }

        [CheatProperty(CustomWidgetAddress = "Assets/Cheats/HeroCheatWidget.prefab")]
        public Persistence.Hero Bob
        {
            get => GetHero("bob");
            set
            {
                PlayerProfile.UpdatePartyMember(value);
            }
        }

        private static Persistence.Hero GetHero(string heroId)
        {
            var hero = PlayerProfile.GetPartyMember(heroId);
            if (hero != null)
                return hero;

            var data = Database.Heroes[heroId];
            return new Persistence.Hero(data, PlayerProfile.GetHeroStats(heroId).MaxHP, PlayerProfile.GetHeroExp(heroId));
        }
    }
}