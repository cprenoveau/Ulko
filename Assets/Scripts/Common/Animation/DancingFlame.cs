using HotChocolate.Motion;
using UnityEngine;

namespace Ulko
{
    public class DancingFlame : MonoBehaviour
    {
        public Vector3 minRotation;
        public Vector3 maxRotation;
        public float minDuration;
        public float maxDuration;

        private Vector3 defaultRot;
        private float flip = 1f;
        private Tween<Quaternion> rotationTween;

        private void Awake()
        {
            defaultRot = transform.localEulerAngles;
        }

        private void Update()
        {
            if(rotationTween != null)
            {
                if(!rotationTween.Play(Time.deltaTime))
                {
                    rotationTween = null;
                }

                return;
            }

            float intensity = Random.Range(0, 1f);
            Vector3 rot = Vector3.Lerp(minRotation, maxRotation, intensity);
            rot *= flip;
            flip *= -1;

            rot += defaultRot;

            float duration = Random.Range(minDuration, maxDuration);

            rotationTween = new Tween<Quaternion>(duration, transform.localRotation, Quaternion.Euler(rot), Quaternion.Slerp, Easing.QuadEaseInOut);
            rotationTween.OnUpdate += UpdateRotation;
        }

        private void UpdateRotation(Quaternion value, float progress)
        {
            transform.localRotation = value;
        }
    }
}
