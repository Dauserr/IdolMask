using UnityEngine;

public class TrapManager : MonoBehaviour
{
    [SerializeField] private TrapConfig       _config;
    [SerializeField] private PlayerController _player;

    // assign these in Inspector — one MonoBehaviour per trap on a child GameObject
    [SerializeField] private AngerTrap   _angerTrap;
    [SerializeField] private JoyTrap     _joyTrap;
    [SerializeField] private SadnessTrap _sadnessTrap;

    private IdolState _currentState   = IdolState.Peaceful;
    private float     _speedMultiplier = 1f;

    public float SpeedMultiplier => _speedMultiplier;

    private void OnEnable()
    {
        GameEvents.OnGameStarted       += OnGameStarted;
        GameEvents.OnGameWon           += OnGameEnded;
        GameEvents.OnGameLost          += OnGameEnded;
        GameEvents.OnIdolStateChanged  += OnIdolStateChanged;
        GameEvents.OnTimerTick         += OnTimerTick;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted       -= OnGameStarted;
        GameEvents.OnGameWon           -= OnGameEnded;
        GameEvents.OnGameLost          -= OnGameEnded;
        GameEvents.OnIdolStateChanged  -= OnIdolStateChanged;
        GameEvents.OnTimerTick         -= OnTimerTick;
    }

    private void OnGameStarted()
    {
        _currentState    = IdolState.Peaceful;
        _speedMultiplier = 1f;
    }

    private void OnGameEnded()
    {
        DeactivateCurrentTrap();
        _currentState    = IdolState.Peaceful;
        _speedMultiplier = 1f;
    }

    private void OnTimerTick(float normalizedTime)
    {
        _speedMultiplier = CalculateSpeedMultiplier(normalizedTime);
    }

    // traps run at normal speed for most of the game, then accelerate near the end
    private float CalculateSpeedMultiplier(float normalizedTime)
    {
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
        var playerPos = _player != null ? _player.GridPosition : Vector2Int.zero;

        switch (_currentState)
        {
            case IdolState.Peaceful:
                break;
            case IdolState.Anger:
                _angerTrap?.Activate(playerPos, _config, _speedMultiplier);
                break;
            case IdolState.Joy:
                _joyTrap?.Activate(_config, _speedMultiplier);
                break;
            case IdolState.Sadness:
                _sadnessTrap?.Activate(_config, _speedMultiplier);
                break;
            case IdolState.Fear:
                // FearTrap added in Batch 6
                Debug.Log("FearTrap not yet implemented");
                break;
            case IdolState.Shock:
                // ShockTrap added in Batch 6
                Debug.Log("ShockTrap not yet implemented");
                break;
        }
    }

    private void DeactivateCurrentTrap()
    {
        switch (_currentState)
        {
            case IdolState.Anger:
                _angerTrap?.Deactivate();
                break;
            case IdolState.Joy:
                _joyTrap?.Deactivate();
                break;
            case IdolState.Sadness:
                _sadnessTrap?.Deactivate();
                break;
        }
    }
}
