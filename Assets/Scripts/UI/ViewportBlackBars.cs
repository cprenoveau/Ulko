using System;
using UnityEngine;

namespace Ulko.UI
{
    public class ViewportBlackBars : MonoBehaviour
    {
        public RectTransform topBar;
        public RectTransform bottomBar;
        public RectTransform leftBar;
        public RectTransform rightBar;

        private void Update()
        {
            var cam = Camera.main;

            if(cam != null)
            {
                topBar.anchorMin = new Vector2(0, 1f - cam.rect.y);
                bottomBar.anchorMax = new Vector2(1f, cam.rect.y);
                leftBar.anchorMax = new Vector2(cam.rect.x, 1f);
                rightBar.anchorMin = new Vector2(1f - cam.rect.x, 0);
            }
        }
    }
}
