using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private Image _progressBar;  // Image with Fill type set to Horizontal

    private void OnEnable()
    {
        GameEvents.OnTimerTick  += OnTimerTick;
        GameEvents.OnGameStarted += OnGameStarted;
    }

    private void OnDisable()
    {
        GameEvents.OnTimerTick   -= OnTimerTick;
        GameEvents.OnGameStarted -= OnGameStarted;
    }

    private void OnGameStarted()
    {
        // reset bar to full when the game starts
        SetFill(1f);
    }

    private void OnTimerTick(float normalizedTime)
    {
        SetFill(normalizedTime);
    }

    private void SetFill(float value)
    {
        if (_progressBar != null)
            _progressBar.fillAmount = Mathf.Clamp01(value);
    }
}
