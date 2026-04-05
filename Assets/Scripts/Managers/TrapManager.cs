using UnityEngine;

public class TrapManager : MonoBehaviour
{
    [SerializeField] private TrapConfig _config;
    [SerializeField] private PlayerController _player;

    private IdolState _currentState = IdolState.Peaceful;
    private float _speedMultiplier = 1f;

    public IdolState CurrentState => _currentState;
    public float SpeedMultiplier => _speedMultiplier;

    private void OnEnable()
    {
        GameEvents.OnGameStarted += OnGameStarted;
        GameEvents.OnGameWon += OnGameEnded;
        GameEvents.OnGameLost += OnGameEnded;
        GameEvents.OnIdolStateChanged += OnIdolStateChanged;
        GameEvents.OnTimerTick += OnTimerTick;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted -= OnGameStarted;
        GameEvents.OnGameWon -= OnGameEnded;
        GameEvents.OnGameLost -= OnGameEnded;
        GameEvents.OnIdolStateChanged -= OnIdolStateChanged;
        GameEvents.OnTimerTick -= OnTimerTick;
    }

    private void OnGameStarted()
    {
        _currentState = IdolState.Peaceful;
        _speedMultiplier = 1f;
    }

    private void OnGameEnded()
    {
        DeactivateCurrentTrap();
        _currentState = IdolState.Peaceful;
        _speedMultiplier = 1f;
    }

    private void OnTimerTick(float normalizedTime)
    {
        _speedMultiplier = CalculateSpeedMultiplier(normalizedTime);
    }

    private float CalculateSpeedMultiplier(float normalizedTime)
    {
        // traps stay normal for most of the round, then speed up near the end
        if (normalizedTime > _config.trapSpeedScaleStart)
            return 1f;

        float endgameProgress = 1f - (normalizedTime / _config.trapSpeedScaleStart);
        return Mathf.Lerp(1f, _config.trapSpeedScaleMax, endgameProgress);
    }

    private void OnIdolStateChanged(IdolState newState)
    {
        DeactivateCurrentTrap();
        _currentState = newState;
        ActivateCurrentTrap();
    }

    private void ActivateCurrentTrap()
    {
        Vector2Int playerGridPosition = _player != null ? _player.GridPosition : Vector2Int.zero;

        switch (_currentState)
        {
            case IdolState.Peaceful:
                break;

            case IdolState.Anger:
                Debug.Log($"Activate AngerTrap at {playerGridPosition}, speed x{_speedMultiplier:0.00}");
                break;

            case IdolState.Joy:
                Debug.Log($"Activate JoyTrap, speed x{_speedMultiplier:0.00}");
                break;

            case IdolState.Sadness:
                Debug.Log($"Activate SadnessTrap, speed x{_speedMultiplier:0.00}");
                break;

            case IdolState.Fear:
                Debug.Log($"Activate FearTrap near {playerGridPosition}, speed x{_speedMultiplier:0.00}");
                break;

            case IdolState.Shock:
                Debug.Log($"Activate ShockTrap near {playerGridPosition}, speed x{_speedMultiplier:0.00}");
                break;
        }
    }

    private void DeactivateCurrentTrap()
    {
        switch (_currentState)
        {
            case IdolState.Anger:
                Debug.Log("Deactivate AngerTrap");
                break;
            case IdolState.Joy:
                Debug.Log("Deactivate JoyTrap");
                break;
            case IdolState.Sadness:
                Debug.Log("Deactivate SadnessTrap");
                break;
            case IdolState.Fear:
                Debug.Log("Deactivate FearTrap");
                break;
            case IdolState.Shock:
                Debug.Log("Deactivate ShockTrap");
                break;
        }
    }
}