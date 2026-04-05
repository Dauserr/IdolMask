using UnityEngine;

public class TimerController : MonoBehaviour
{
    public static TimerController Instance { get; private set; }

    [SerializeField] private TrapConfig _config;

    private float _timeRemaining;
    private bool  _isRunning;

    // 1.0 = full time left, 0.0 = time is up — used by TimerUI and TrapManager
    public float NormalizedTime => _timeRemaining / _config.gameDuration;
    public float TimeRemaining  => _timeRemaining;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        GameEvents.OnGameStarted += StartTimer;
        GameEvents.OnGameWon     += StopTimer;
        GameEvents.OnGameLost    += StopTimer;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted -= StartTimer;
        GameEvents.OnGameWon     -= StopTimer;
        GameEvents.OnGameLost    -= StopTimer;
    }

    private void Update()
    {
        if (!_isRunning) return;

        _timeRemaining -= Time.deltaTime;

        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            _isRunning     = false;
            GameEvents.TriggerTimerEnded();
            return;
        }

        // broadcast normalised time every frame so UI and TrapManager stay in sync
        GameEvents.TriggerTimerTick(NormalizedTime);
    }

    private void StartTimer()
    {
        _timeRemaining = _config.gameDuration;
        _isRunning     = true;
    }

    private void StopTimer()
    {
        _isRunning = false;
    }
}
