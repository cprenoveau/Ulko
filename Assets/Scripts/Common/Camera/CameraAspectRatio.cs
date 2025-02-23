using UnityEngine;

namespace Ulko
{
    [RequireComponent(typeof(Camera))]
    public class CameraAspectRatio : MonoBehaviour
    {
        public float width = 1920;
        public float height = 1080;

        private Camera cam;
        private float MinAspect => width / height;

        private void Start()
        {
            cam = GetComponent<Camera>();
            Refresh();
        }

        private void Refresh()
        {
            float targetAspect = (float)Screen.width / Screen.height;
            if (targetAspect < MinAspect) targetAspect = MinAspect;

            UpdateAspect(targetAspect);
        }

        private void UpdateAspect(float targetAspect)
        {
            // determine the game window's current aspect ratio
            float windowaspect = (float)Screen.width / Screen.height;

            // current viewport height should be scaled by this amount
            float scaleheight = windowaspect / targetAspect;

            // if scaled height is less than current height, add letterbox
            if (scaleheight < 1.0f)
            {
                Rect rect = cam.rect;

                rect.width = 1.0f;
                rect.height = scaleheight;
                rect.x = 0;
                rect.y = (1.0f - scaleheight) / 2.0f;

                cam.rect = rect;
            }
            else // add pillarbox
            {
                float scalewidth = 1.0f / scaleheight;

                Rect rect = cam.rect;

                rect.width = scalewidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scalewidth) / 2.0f;
                rect.y = 0;

                cam.rect = rect;
            }
        }
    }
}
