using HotChocolate.Motion;
using UnityEngine;

namespace Ulko.UI
{
    [CreateAssetMenu(fileName = "MoveTween", menuName = "Ulko/Animation/Move Tween", order = 1)]
    public class MoveTweenConfig : ScriptableObject
    {
        public Vector2 startPositionOffset = new Vector2(-100, 0);
        public Vector2 endPositionOffset = new Vector2(100, 0);

        public float inDuration = 0.25f;
        public float backDuration = 0.5f;
        public float forthDuration = 0.5f;

        public EasingType inEasing = EasingType.SineEaseOut;
        public EasingType backEasing = EasingType.SineEaseInOut;
        public EasingType forthEasing = EasingType.SineEaseInOut;
    }
}
