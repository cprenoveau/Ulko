using UnityEngine;

namespace Ulko
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class MinimapSprite : MonoBehaviour
    {
        void Awake()
        {
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}