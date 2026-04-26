using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timerText;

    private void OnEnable()
    {
        GameEvents.OnTimerTick   += OnTimerTick;
        GameEvents.OnGameStarted += OnGameStarted;
    }

    private void OnDisable()
    {
        GameEvents.OnTimerTick   -= OnTimerTick;
        GameEvents.OnGameStarted -= OnGameStarted;
    }

    private void OnGameStarted()
    {
        UpdateDisplay(1f);
    }

    private void OnTimerTick(float normalizedTime)
    {
        UpdateDisplay(normalizedTime);
    }

    private void UpdateDisplay(float normalizedTime)
    {
        if (_timerText == null) return;

        // convert normalized (0-1) back to seconds using TimerController's max time
        float maxTime = TimerController.Instance != null
            ? TimerController.Instance.MaxTime
            : 60f;

        float secondsLeft = normalizedTime * maxTime;

        // format as  0:59  or  1:23
        int minutes = Mathf.FloorToInt(secondsLeft / 60f);
        int seconds = Mathf.FloorToInt(secondsLeft % 60f);

        _timerText.text = $"{minutes}:{seconds:D2}";

        // turn red when under 10 seconds
        _timerText.color = secondsLeft <= 10f
            ? new Color(0.9f, 0.2f, 0.2f)
            : Color.white;
    }
}
