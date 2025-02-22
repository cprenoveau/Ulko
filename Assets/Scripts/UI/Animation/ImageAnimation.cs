using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    [RequireComponent(typeof(Image))]
    public class ImageAnimation : MonoBehaviour
    {
        public bool playOnStart = true;
        public bool loop = true;
        public float speed = 1f;
        public float duration = float.PositiveInfinity;
        public SpriteAnimation anim;

        private void OnEnable()
        {
            if (playOnStart) Play();
        }

        public void Play()
        {
            StartCoroutine(anim.Play(GetComponent<Image>(), loop, speed, duration));
        }
    }
}
