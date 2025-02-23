using UnityEngine;

namespace Ulko.World
{
    [CreateAssetMenu(fileName = "WorldConfig", menuName = "Ulko/World Config", order = 1)]
    public class WorldConfig : ScriptableObject
    {
        public Vector2 playerSpeed = new(5f, 5f);
    }
}
