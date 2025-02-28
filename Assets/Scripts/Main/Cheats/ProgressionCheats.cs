using HotChocolate.Cheats;
using HotChocolate.Utils;
using UnityEngine;

namespace Ulko.Cheats
{
    [CreateAssetMenu(fileName = "ProgressionCheats", menuName = "Ulko/Cheats/Progression", order = 1)]
    public class ProgressionCheats : ScriptableObject
    {
        private GameInstance GameInstance => FindFirstObjectByType<GameInstance>(FindObjectsInactive.Include);

        [CheatMethod(ShortcutKeys = new KeyCode[] { KeyCode.F12 })]
        public void NextMilestone()
        {
            if (GameInstance != null)
                GameInstance.StartNextMilestone(default).FireAndForgetTask();
        }
    }
}