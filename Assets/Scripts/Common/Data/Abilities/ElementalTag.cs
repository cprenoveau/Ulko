using UnityEngine;

namespace Ulko.Data
{
    [CreateAssetMenu(fileName = "ElementalTag", menuName = "Ulko/Elemental Tag", order = 1)]
    public class ElementalTag : ScriptableObject
    {
        public string id;
        public Color color = Color.white;
        public Sprite icon;
    }
}
