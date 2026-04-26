using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Button")]
    [SerializeField] private Button _playAgainButton;

    private void Awake()
    {
        _canvasGroup.alpha          = 0f;
        _canvasGroup.interactable   = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()  => GameEvents.OnGameWon += Show;
    private void OnDisable() => GameEvents.OnGameWon -= Show;

    private void Start()
    {
        _playAgainButton.onClick.AddListener(OnPlayAgain);
    }

    private void Show()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        _canvasGroup.interactable   = true;
        _canvasGroup.blocksRaycasts = true;

        float elapsed  = 0f;
        float duration = 0.6f;

        while (elapsed < duration)
        {
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _canvasGroup.alpha = 1f;
    }

    private void Hide()
    {
        _canvasGroup.alpha          = 0f;
        _canvasGroup.interactable   = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void OnPlayAgain()
    {
        Hide();
        GameManager.Instance.RestartGame();
    }
}
