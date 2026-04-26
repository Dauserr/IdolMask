using UnityEngine;

public class TrapManager : MonoBehaviour
{
    [SerializeField] private TrapConfig       _config;
    [SerializeField] private PlayerController _player;

    [SerializeField] private AngerTrap   _angerTrap;
    [SerializeField] private JoyTrap     _joyTrap;
    [SerializeField] private SadnessTrap _sadnessTrap;
    [SerializeField] private FearTrap    _fearTrap;
    [SerializeField] private ShockTrap   _shockTrap;

    private IdolState _currentState    = IdolState.Peaceful;
    private float     _speedMultiplier = 1f;

    public float SpeedMultiplier => _speedMultiplier;

    private void OnEnable()
    {
        GameEvents.OnGameStarted      += OnGameStarted;
        GameEvents.OnGameWon          += OnGameEnded;
        GameEvents.OnGameLost         += OnGameEnded;
        GameEvents.OnIdolStateChanged += OnIdolStateChanged;
        GameEvents.OnTimerTick        += OnTimerTick;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted      -= OnGameStarted;
        GameEvents.OnGameWon          -= OnGameEnded;
        GameEvents.OnGameLost         -= OnGameEnded;
        GameEvents.OnIdolStateChanged -= OnIdolStateChanged;
        GameEvents.OnTimerTick        -= OnTimerTick;
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

    private float CalculateSpeedMultiplier(float normalizedTime)
    {
        if (normalizedTime > _config.trapSpeedScaleStart)
            return 1f;

        float endgameProgress = 1f - (normalizedTime / _config.trapSpeedScaleStart);
        return Mathf.Lerp(1f, _config.trapSpeedScaleMax, endgameProgress);
    }

    private void OnIdolStateChanged(IdolState newState)
    {
        // 1. stop old trap logic
        DeactivateCurrentTrap();

        // 2. clean up any tiles the old trap left behind
        Arena.Instance?.ForceRespawnAll();

        // 3. switch state and start new trap
        _currentState = newState;
        ActivateCurrentTrap();
    }

    private void ActivateCurrentTrap()
    {
        var playerPos = _player != null ? _player.GridPosition : Vector2Int.zero;

        switch (_currentState)
        {
            case IdolState.Peaceful: break;
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
                _fearTrap?.Activate(playerPos, _config, _speedMultiplier);
                break;
            case IdolState.Shock:
                _shockTrap?.Activate(playerPos, _config, _speedMultiplier);
                break;
        }
    }

    private void DeactivateCurrentTrap()
    {
        switch (_currentState)
        {
            case IdolState.Anger:   _angerTrap?.Deactivate();   break;
            case IdolState.Joy:     _joyTrap?.Deactivate();     break;
            case IdolState.Sadness: _sadnessTrap?.Deactivate(); break;
            case IdolState.Fear:    _fearTrap?.Deactivate();    break;
            case IdolState.Shock:   _shockTrap?.Deactivate();   break;
        }
    }
}
