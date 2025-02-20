using System.Linq;
using UnityEngine;

namespace Ulko
{
    [CreateAssetMenu(fileName = "LightingAndWeatherConfig", menuName = "Ulko/Graphics/Lighting and Weather", order = 1)]
    public class LightingAndWeatherConfig : ScriptableObject
    {
        [Range(0, 24)]
        public float timeOfDay;
        public float sunIntensity = 1f;
        public Color sunColor = Color.white;
        public float shadowStrength = 0.5f;
        public Material skybox;
        public float skyboxRotationSpeed;
        [ColorUsage(true, true)]
        public Color ambientColor;
        public Color shadowColor;
        public bool fogEnabled;
        public Color fogColor;

        private static float dawnHour = 5f;
        private static float duskHour = 22f;

        public static LightingAndWeatherConfig Config { get;private set; }
        public static void SetCurrent(LightingAndWeatherConfig config, bool isInterior)
        {
            Config = config;

            RefreshCurrent();

            if (isInterior)
                RenderSettings.skybox = null;
        }

        public static void RefreshCurrent()
        {
            if (Config == null)
                return;

            var directionalLights = GameObject.FindObjectsOfType<Light>(true).Where(l => l.type == LightType.Directional);
            foreach (var light in directionalLights)
            {
                light.intensity = Config.sunIntensity;
                light.color = Config.sunColor;
                light.shadowStrength = Config.shadowStrength;

                float normalizedTime = (Config.timeOfDay - dawnHour) / (duskHour - dawnHour);
                float angle = normalizedTime * 180f;

                light.transform.rotation = Quaternion.AngleAxis(angle, Vector3.right);
                light.transform.Rotate(Vector3.up, (normalizedTime * 2f - 1f) * 35f);
            }

            var skyboxAnims = GameObject.FindObjectsOfType<SkyboxAnimation>(true);
            foreach (var skybox in skyboxAnims)
            {
                skybox.speed = Config.skyboxRotationSpeed;
            }

            if(RenderSettings.skybox != null)
            {
                RenderSettings.skybox = Config.skybox;
            }

            RenderSettings.ambientLight = Config.ambientColor;
            RenderSettings.subtractiveShadowColor = Config.shadowColor;
            RenderSettings.fog = Config.fogEnabled;
            RenderSettings.fogColor = Config.fogColor;
        }
    }
}
