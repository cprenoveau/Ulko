using UnityEngine;

namespace Ulko
{
    [RequireComponent(typeof(Renderer))]
    public class GradientColor : MonoBehaviour
    {
        public GradientColorConfig config;

        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
#if UNITY_EDITOR
            Refresh();
#endif
        }

        private void Refresh()
        {
            if (config != null)
            {
                GetComponent<Renderer>().material.SetColor("_GradientColor", config.color);
                GetComponent<Renderer>().material.SetFloat("_GradientAlpha", config.alpha);
            }
        }
    }
}
