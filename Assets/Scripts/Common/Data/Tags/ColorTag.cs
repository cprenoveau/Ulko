using UnityEngine;

namespace Ulko.Data
{
    [CreateAssetMenu(fileName = "ColorTag", menuName = "Ulko/Color Tag", order = 1)]
    public class ColorTag : ScriptableObject
    {
        public Color color = Color.white;
    }
}
