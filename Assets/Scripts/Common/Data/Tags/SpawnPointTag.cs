using UnityEngine;

namespace Ulko.Data
{
    [CreateAssetMenu(fileName = "SpawnPointTag", menuName = "Ulko/Spawn Point Tag", order = 1)]
    public class SpawnPointTag : ScriptableObject
    {
        public string id;
    }
}
