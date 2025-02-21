using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ulko
{
    [CreateAssetMenu(fileName = "MenuDefinition", menuName = "Ulko/Menu Definition", order = 1)]
    public class MenuDefinition : ScriptableObject
    {
        public AssetReference asset;
        public string id;
    }
}
