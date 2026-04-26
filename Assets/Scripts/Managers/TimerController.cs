using UnityEngine;

public class TimerController : MonoBehaviour
{
    public static TimerController Instance { get; private set; }

    [SerializeField] private TrapConfig _config;

    private float _timeRemaining;
    private float _elapsedTime;
    private bool  _isRunning;

    public float NormalizedTime => _timeRemaining / _config.gameDuration;
    public float TimeRemaining  => _timeRemaining;
    public float MaxTime        => _config.gameDuration;
    public float ElapsedTime    => _elapsedTime;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        GameEvents.OnGameStarted   += StartTimer;
        GameEvents.OnGameWon       += StopTimer;
        GameEvents.OnGameLost      += StopTimer;
        GameEvents.OnGameRestarted += StopTimer;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted   -= StartTimer;
        GameEvents.OnGameWon       -= StopTimer;
        GameEvents.OnGameLost      -= StopTimer;
        GameEvents.OnGameRestarted -= StopTimer;
    }

    private void Update()
    {
        if (!_isRunning) return;

        _timeRemaining -= Time.deltaTime;
        _elapsedTime   += Time.deltaTime;

        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            _isRunning     = false;
            GameEvents.TriggerTimerEnded();
            return;
        }

        GameEvents.TriggerTimerTick(NormalizedTime);
    }

    private void StartTimer()
    {
        _timeRemaining = _config.gameDuration;
        _elapsedTime   = 0f;
        _isRunning     = true;
    }

    private void StopTimer()
    {
        _isRunning = false;
    }
}
