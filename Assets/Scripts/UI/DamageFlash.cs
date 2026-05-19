using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Image _flashImage;
    [SerializeField] private float _flashDuration = 0.3f;
    [SerializeField] private Color _flashColor = new Color(1f, 0f, 0f, 0.4f);

    private void Awake()
    {
        GameEvents.OnPlayerFell += Flash;
    }

    private void OnDestroy()
    {
        GameEvents.OnPlayerFell -= Flash;
    }

    private void Flash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        _flashImage.color = _flashColor;
        float elapsed = 0f;

        while (elapsed < _flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(_flashColor.a, 0f, elapsed / _flashDuration);
            _flashImage.color = new Color(_flashColor.r, _flashColor.g, _flashColor.b, alpha);
            yield return null;
        }

        _flashImage.color = new Color(_flashColor.r, _flashColor.g, _flashColor.b, 0f);
    }
}