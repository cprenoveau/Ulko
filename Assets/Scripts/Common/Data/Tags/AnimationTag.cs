using UnityEngine;

namespace Ulko.Data
{
    [CreateAssetMenu(fileName = "AnimationTag", menuName = "Ulko/Tags/Animation Tag", order = 1)]
    public class AnimationTag : ScriptableObject
    {
        public string id;
    }
}
