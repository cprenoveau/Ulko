using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/GlitchEffect")]
[RequireComponent(typeof(Camera))]
public class VHSPostProcessEffect : MonoBehaviour
{
	public Shader shader;

	private float _yScanline;
	private float _xScanline;
	private Material _material = null;

	void Start()
	{
		_material = new Material(shader);
		enabled = false;
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		_yScanline += Time.deltaTime * 0.01f;
		_xScanline -= Time.deltaTime * 0.1f;

		if (_yScanline >= 1)
		{
			_yScanline = Random.value;
		}
		if (_xScanline <= 0 || Random.value < 0.05)
		{
			_xScanline = Random.value;
		}
		_material.SetFloat("_yScanline", _yScanline);
		_material.SetFloat("_xScanline", _xScanline);
		Graphics.Blit(source, destination, _material);
	}
}
