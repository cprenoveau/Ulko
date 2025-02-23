using UnityEngine;

namespace Ulko.Data
{
    [CreateAssetMenu(fileName = "ColorTag", menuName = "Ulko/Tags/Color Tag", order = 1)]
    public class ColorTag : ScriptableObject
    {
        public Color color = Color.white;
    }
}
