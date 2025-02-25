using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Ulko.UI
{
    public class TextQueue : MonoBehaviour
    {
        public TMP_Text textPrefab;

        public class TextData
        {
            public string text;
            public Color color;
            public float speed;
            public float showNextDelay;
            public float duration;
            public Transform target;
        }

        private readonly Queue<TextData> textData = new();
        private Camera cam;

        public void Init(Camera camera)
        {
            cam = camera;
        }

        public void Enqueue(string text, Color color, float speed, float showNextDelay, float duration, Transform target)
        {
            textData.Enqueue(new TextData { text = text, color = color, speed = speed, showNextDelay = showNextDelay, duration = duration, target = target });
            if (textData.Count == 1)
                StartCoroutine(Play());
        }

        private IEnumerator Play()
        {
            while(textData.Count > 0)
            {
                var next = textData.Peek();

                var instance = Instantiate(textPrefab, transform);
                instance.text = next.text;
                instance.color = next.color;

                Vector2 viewportPoint = Camera.main.WorldToViewportPoint(next.target.position);

                instance.GetComponent<RectTransform>().anchorMin = viewportPoint;
                instance.GetComponent<RectTransform>().anchorMax = viewportPoint;

                StartCoroutine(ShowText(instance.gameObject, next.duration, next.speed));

                yield return new WaitForSeconds(next.showNextDelay);

                textData.Dequeue();
            }
        }

        private IEnumerator ShowText(GameObject instance, float duration, float speed)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                instance.GetComponent<RectTransform>().anchoredPosition += Vector2.up * speed * Time.deltaTime;
                elapsed += Time.deltaTime;

                yield return null;
            }

            Destroy(instance);
        }
    }
}
