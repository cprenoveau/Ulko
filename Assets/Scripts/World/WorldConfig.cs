using UnityEngine;

namespace Ulko.World
{
    [CreateAssetMenu(fileName = "World", menuName = "Ulko/World", order = 1)]
    public class WorldConfig : ScriptableObject
    {
        public Vector2 playerSpeed = new(5f, 5f);
    }
}
