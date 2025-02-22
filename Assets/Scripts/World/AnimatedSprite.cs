using UnityEngine;

namespace Ulko.World
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class AnimatedSprite : MonoBehaviour
    {
        public SpriteAnimation anim;
        public bool loop = true;

        private void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(anim.Play(this, GetComponent<SpriteRenderer>(), loop));
        }
    }
}
