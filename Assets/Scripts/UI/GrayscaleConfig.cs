using UnityEngine;
using UnityEngine.UI;

namespace Ulko.UI
{
    [CreateAssetMenu(fileName = "UI_Grayscale", menuName = "Ulko/UI/UI Grayscale", order = 1)]
    public class GrayscaleConfig : ScriptableObject
    {
        public float grayscaleAmount = 0.5f;
        public float alpha = 0.5f;

        private Material defaultMat;
        private Material grayscaleMat;

        private void OnEnable()
        {
            defaultMat = new Material(Shader.Find("UI/Grayscale"));
            grayscaleMat = new Material(Shader.Find("UI/Grayscale"));
            grayscaleMat.SetFloat("_DesaturationAmount", grayscaleAmount);
        }

        public void Set(Image image)
        {
            image.material = grayscaleMat;

            var color = image.color;
            color.a = alpha;
            image.color = color;
        }

        public void Reset(Image image)
        {
            image.material = defaultMat;

            var color = image.color;
            color.a = 1f;
            image.color = color;
        }
    }
}
