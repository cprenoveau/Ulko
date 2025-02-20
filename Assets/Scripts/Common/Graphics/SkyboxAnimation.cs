using UnityEngine;

namespace Ulko
{
    public class SkyboxAnimation : MonoBehaviour
    {
        public string parameterName = "_Rotation";
        public float speed = 30f;

        public void Update()
        {
            if (RenderSettings.skybox != null)
                RenderSettings.skybox.SetFloat(parameterName, RenderSettings.skybox.GetFloat(parameterName) + speed * Time.deltaTime);
        }

        private void OnDestroy()
        {
            if(RenderSettings.skybox != null)
                RenderSettings.skybox.SetFloat(parameterName, 0);
        }
    }
}