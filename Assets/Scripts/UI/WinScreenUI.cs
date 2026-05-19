using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _survivedText;

    [Header("Button")]
    [SerializeField] private Button _playAgainButton;

    [SerializeField] private float _fadeDuration = 0.6f;

    private void Awake()
    {
        _canvasGroup.alpha          = 0f;
        _canvasGroup.interactable   = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()  => GameEvents.OnShowWinScreen += Show;
    private void OnDisable() => GameEvents.OnShowWinScreen -= Show;

    private void Start() => _playAgainButton.onClick.AddListener(OnPlayAgain);

    private void Show()
    {
        float survived = TimerController.Instance != null
            ? TimerController.Instance.ElapsedTime : 0f;
        _survivedText.text = $"Survived  {survived:F1}s";

        StartCoroutine(Fade(0f, 1f, true));
    }

    private void Hide() => StartCoroutine(Fade(1f, 0f, false));

    private void OnPlayAgain()
    {
        Hide();
        GameManager.Instance.RestartGame();
    }

    private IEnumerator Fade(float from, float to, bool interactive)
    {
        // set interactable at start of fade-in, end of fade-out
        if (interactive)
        {
            _canvasGroup.interactable   = true;
            _canvasGroup.blocksRaycasts = true;
        }

        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / _fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        _canvasGroup.alpha = to;

        if (!interactive)
        {
            _canvasGroup.interactable   = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}
