using HotChocolate.Motion;
using UnityEngine;

namespace Ulko.UI
{
    [CreateAssetMenu(fileName = "FlashTween", menuName = "Ulko/Animation/Flash Tween", order = 1)]
    public class FlashTweenConfig : ScriptableObject
    {
        public float startAlpha = 1f;
        public float endAlpha = 0f;

        public float inDuration = 0.25f;
        public float backDuration = 0.5f;
        public float forthDuration = 0.5f;

        public EasingType inEasing = EasingType.SineEaseOut;
        public EasingType backEasing = EasingType.SineEaseInOut;
        public EasingType forthEasing = EasingType.SineEaseInOut;
    }
}
