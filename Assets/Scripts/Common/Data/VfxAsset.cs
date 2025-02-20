using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ulko
{
    [CreateAssetMenu(fileName = "VfxAsset", menuName = "Ulko/Vfx Asset", order = 1)]
    public class VfxAsset : ScriptableObject
    {
        public string id;
        public AssetReferenceT<GameObject> prefab;
        public AudioDefinition sound;
    }
}
