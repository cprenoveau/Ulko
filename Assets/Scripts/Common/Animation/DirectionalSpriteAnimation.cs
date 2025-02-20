using System.Collections;
using UnityEngine;

namespace Ulko
{
    public enum Direction
    {
        Down,
        Up,
        Left,
        Right
    }

    [CreateAssetMenu(fileName = "DirectionalSpriteAnimation", menuName = "Ulko/Animation/Directional Sprite Animation", order = 1)]
    public class DirectionalSpriteAnimation : ScriptableObject
    {
        public SpriteAnimation down;
        public SpriteAnimation up;
        public SpriteAnimation left;
        public SpriteAnimation right;

        public SpriteAnimation CurrentAnimation { get; private set; }
        public int CurrentFrameIndex => CurrentAnimation != null ? CurrentAnimation.CurrentFrameIndex : -1;

        public IEnumerator Play(MonoBehaviour holder, SpriteRenderer renderer, Vector2 direction, bool loop, float speed = 1f)
        {
            yield return Play(holder, renderer, GetDirection(direction), loop, speed);
        }

        public IEnumerator Play(MonoBehaviour holder, SpriteRenderer renderer, Direction direction, bool loop, float speed = 1f)
        {
            CurrentAnimation = GetAnimation(direction);
            yield return CurrentAnimation.Play(holder, renderer, loop, speed);
        }

        public Direction GetDirection(Vector2 direction)
        {
            if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                if (direction.x > 0)
                    return Direction.Right;
                else
                    return Direction.Left;
            }
            else
            {
                if (direction.y > 0)
                    return Direction.Up;
                else
                    return Direction.Down;
            }
        }

        public SpriteAnimation GetAnimation(Vector2 direction)
        {
            return GetAnimation(GetDirection(direction));
        }

        public SpriteAnimation GetAnimation(Direction direction)
        {
            switch (direction)
            {
                case Direction.Down: return down;
                case Direction.Up: return up;
                case Direction.Left: return left;
                case Direction.Right: return right;
                default: return down;
            }
        }
    }
}
