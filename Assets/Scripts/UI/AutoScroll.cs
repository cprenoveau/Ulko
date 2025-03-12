using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ulko.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class AutoScroll : MonoBehaviour
    {
        public float scrollDuration = 0.2f;

        private ScrollRect scrollRect;
        private HotChocolate.Motion.Tween<Vector2> scroll;

        private void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        public void Reset(Vector2 anchoredPosition)
        {
            scroll = null;
            scrollRect.content.anchoredPosition = anchoredPosition;
        }

        private void Update()
        {
            if(scroll != null)
            {
                if (!scroll.Play(Time.unscaledDeltaTime))
                    scroll = null;

                return;
            }

            var pos = ScrollToPosition();
            if(pos.HasValue)
            {
                ScrollTo(pos.Value);
            }
        }

        private Transform FindSelectedRoot(Transform parent)
        {
            while(parent != null)
            {
                if (parent.parent == scrollRect.content)
                    return parent;

                parent = parent.parent;
            }

            return null;
        }

        private Vector2? ScrollToPosition()
        {
            var selected = EventSystem.current.currentSelectedGameObject;

            if (selected == null)
                return null;

            var selectedRoot = FindSelectedRoot(selected.transform);

            if(selectedRoot == null)
                return null;

            var rt = selectedRoot.GetComponent<RectTransform>();

            if (scrollRect.vertical)
            {
                float minPosY = -rt.anchoredPosition.y;
                float maxPosY = minPosY + rt.rect.height;

                float viewMinY = scrollRect.content.anchoredPosition.y;
                float viewMaxY = viewMinY + scrollRect.viewport.rect.height;

                //scroll down
                if (maxPosY > viewMaxY)
                {
                    int childCount = scrollRect.content.childCount;
                    if (rt == scrollRect.content.GetChild(childCount - 1))
                    {
                        maxPosY = scrollRect.content.rect.height;
                    }

                    var pos = new Vector2(scrollRect.content.anchoredPosition.x, maxPosY - scrollRect.viewport.rect.height);
                    return pos;
                }
                //scroll up
                else if (minPosY < viewMinY)
                {
                    if (rt == scrollRect.content.GetChild(0))
                    {
                        minPosY = 0;
                    }

                    var pos = new Vector2(scrollRect.content.anchoredPosition.x, minPosY);
                    return pos;
                }
            }

            return null;
        }

        private void ScrollTo(Vector2 pos)
        {
            scroll = new HotChocolate.Motion.Tween<Vector2>(
                scrollDuration,
                scrollRect.content.anchoredPosition,
                pos, 
               Vector2.Lerp,
              HotChocolate.Motion.Easing.SineEaseInOut);

            scroll.OnUpdate += UpdatePosition;
        }

        private void UpdatePosition(Vector2 value, float progress)
        {
            scrollRect.content.anchoredPosition = value;
        }
    }
}
