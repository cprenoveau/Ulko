using UnityEngine;

namespace Ulko
{
    [CreateAssetMenu(fileName = "GradientColor", menuName = "Ulko/Graphics/Gradient Color", order = 1)]
    public class GradientColorConfig : ScriptableObject
    {
        public Color color;
        public float alpha;
    }
}
