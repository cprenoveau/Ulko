using Ulko.Contexts;
using HotChocolate.Cheats;
using UnityEngine;

namespace Ulko.Cheats
{
    [CreateAssetMenu(fileName = "BattleCheats", menuName = "Ulko/Cheats/Battle", order = 1)]
    public class BattleCheats : ScriptableObject
    {
        private BattleContext BattleContext => FindFirstObjectByType<BattleContext>(FindObjectsInactive.Include);

        [CheatMethod(ShortcutKeys = new KeyCode[] { KeyCode.F2 })]
        public void WinBattle()
        {
            if(BattleContext != null)
                BattleContext.ForceWinBattle();
        }

        [CheatMethod(ShortcutKeys = new KeyCode[] { KeyCode.F3 })]
        public void LoseBattle()
        {
            if(BattleContext != null)
                BattleContext.ForceLoseBattle();
        }
    }
}