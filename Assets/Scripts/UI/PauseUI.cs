using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup _pausePanel;

    [Header("Buttons")]
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _resumeButton;

    [SerializeField] private float _fadeDuration = 0.25f;

    private bool      _isPaused  = false;
    private Coroutine _fadeCo;

    private void Start()
    {
        _pausePanel.alpha          = 0f;
        _pausePanel.interactable   = false;
        _pausePanel.blocksRaycasts = false;

        _pauseButton?.onClick.AddListener(TogglePause);
        _resumeButton?.onClick.AddListener(Resume);
    }

    private void OnEnable()
    {
        GameEvents.OnGameLost += ForceHide;
        GameEvents.OnGameWon  += ForceHide;
    }

    private void OnDisable()
    {
        GameEvents.OnGameLost -= ForceHide;
        GameEvents.OnGameWon  -= ForceHide;
    }

    // called by PauseButton OnClick
    public void TogglePause()
    {
        if (_isPaused) Resume();
        else           Pause();
    }

    // called by ResumeButton OnClick
    public void Resume()
    {
        _isPaused      = false;
        Time.timeScale = 1f;
        FadeTo(0f, false);
    }

    private void Pause()
    {
        _isPaused      = true;
        Time.timeScale = 0f;
        FadeTo(1f, true);
    }

    // auto-hide when game ends so pause screen doesn't sit on top of lose/win
    private void ForceHide()
    {
        _isPaused      = false;
        Time.timeScale = 1f;

        _pausePanel.alpha          = 0f;
        _pausePanel.interactable   = false;
        _pausePanel.blocksRaycasts = false;
    }

    private void FadeTo(float target, bool interactive)
    {
        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = StartCoroutine(FadeRoutine(target, interactive));
    }

    private IEnumerator FadeRoutine(float target, bool interactive)
    {
        if (interactive)
        {
            _pausePanel.interactable   = true;
            _pausePanel.blocksRaycasts = true;
        }

        float start   = _pausePanel.alpha;
        float elapsed = 0f;

        // use unscaledDeltaTime so fade works even when timeScale = 0
        while (elapsed < _fadeDuration)
        {
            _pausePanel.alpha = Mathf.Lerp(start, target, elapsed / _fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        _pausePanel.alpha = target;

        if (!interactive)
        {
            _pausePanel.interactable   = false;
            _pausePanel.blocksRaycasts = false;
        }
    }
}
