using HotChocolate.Motion;
using System.Collections;
using UnityEngine;

namespace Ulko.Data.Cutscenes
{
    public class FlashStep : StepAction
    {
        public Camera cam;
        public float duration = 2f;
        public EasingType easingType = EasingType.Linear;
        public Color color = Color.white;

        private Texture2D texture;
        public override IEnumerator Play()
        {
            texture = new Texture2D(1, 1);

            var tween = new Tween<float>(duration, 1f, 0f, Mathf.Lerp, EasingUtil.EasingFunction(easingType));
            tween.OnUpdate += UpdateAlpha;

            while(tween.Play(Time.deltaTime))
            {
                yield return null;
            }

            Destroy(texture);
            texture = null;
        }

        private void UpdateAlpha(float alpha, float progress)
        {
            texture.SetPixel(0, 0, new Color(color.r, color.g, color.b, alpha));
            texture.Apply();
        }

        private void OnGUI()
        {
            if(texture != null)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture);
            }
        }
    }
}
