using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is under CCO License (TL;DR do what you will with it ;-)
/// See https://creativecommons.org/share-your-work/public-domain/cc0/
/// 
/// This works best when you have a fire or candle particle effect on the same location as the light
///
/// Check out my You Tube channel https://youtube.com/c/WizardsCode
/// </summary>
namespace WizardsCode.VFX
{
    public class FlickeringLights : MonoBehaviour
    {
        [SerializeField, Tooltip("Minimum light intensity for the flicker effect.")]
        float _minIntensity = 0f;
        [SerializeField, Tooltip("Maximum light intensity for the flicker effect.")]
        float _maxIntensity = 1f;
        [SerializeField, Range(1, 100), Tooltip("How to smooth out the randomness; lower values = sparks (lots of flickering), mid = torch (some flickering), high = lantern (steady)")]
        int _smoothing = 5;
        [SerializeField, Range(0, 0.1f), Tooltip("The maximum amount of movement the light source should have.")]
        float _randomMovement = 0.01f;

        private Light[] _lights;
        private Renderer[] _renderers;
        private Color[] _materialEmmissionColors;
        private Vector3[] _originalPositions;
        private Queue<float> _smoothQueue;
        private float _lastSum;

        void Start()
        {
            _lights = gameObject.GetComponentsInChildren<Light>();
            _renderers = gameObject.GetComponentsInChildren<Renderer>();
            _materialEmmissionColors = new Color[_renderers.Length];

            _originalPositions = new Vector3[_lights.Length];
            for (int i = 0; i < _lights.Length; i++)
            {
                _originalPositions[i] = _lights[i].transform.position;
            }

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i].material.HasProperty("_EmissionColor"))
                {
                    _materialEmmissionColors[i] = _renderers[i].material.GetColor("_EmissionColor");
                }
                else
                {
                    _materialEmmissionColors[i] = Color.black;
                }
            }

            _smoothQueue = new Queue<float>(_smoothing);
        }

        public void Reset()
        {
            _smoothQueue.Clear();
            _lastSum = 0;
        }

        void Update()
        {
            while (_smoothQueue.Count >= _smoothing)
            {
                _lastSum -= _smoothQueue.Dequeue();
            }

            float newVal = Random.Range(_minIntensity, _maxIntensity);
            _smoothQueue.Enqueue(newVal);
            _lastSum += newVal;

            SetIntensity(_lastSum / (float)_smoothQueue.Count);
        }

        void SetIntensity(float intensity)
        {
            float value = ((_maxIntensity - _minIntensity) - (_maxIntensity - intensity)) / (_maxIntensity - _minIntensity);
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].material.SetColor("_EmissionColor", Color.Lerp(Color.black, _materialEmmissionColors[i], value));
            }

            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].intensity = intensity;
                float movement = intensity * _randomMovement;
                _lights[i].transform.position = _originalPositions[i] + (Vector3.up * movement);
            }
        }
    }
}
