using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private enum GameState { WaitingForPickup, Playing, Won, Lost }

    [SerializeField] private Arena            _arena;
    [SerializeField] private PlayerController _player;
    [SerializeField] private TrapConfig       _config;
    [SerializeField] private HintUI           _hintUI;

    [SerializeField] private Vector2Int _maskGridPosition    = new(7, 7);
    [SerializeField] private Vector2Int _playerStartPosition = new(0, 7);

    private GameState _state = GameState.WaitingForPickup;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _player.SetStartPosition(_playerStartPosition);
        GameEvents.OnPlayerMoved   += OnPlayerMoved;
        GameEvents.OnAllHeartsLost += OnAllHeartsLost;
        GameEvents.OnTimerEnded    += OnTimerEnded;
    }

    private void OnDestroy()
    {
        GameEvents.OnPlayerMoved   -= OnPlayerMoved;
        GameEvents.OnAllHeartsLost -= OnAllHeartsLost;
        GameEvents.OnTimerEnded    -= OnTimerEnded;
    }

    private void OnPlayerMoved(Vector2Int position)
    {
        if (_state != GameState.WaitingForPickup) return;
        if (position == _maskGridPosition)
            StartGame();
    }

    private void StartGame()
    {
        _state = GameState.Playing;
        _arena.StartGame();
        GameEvents.TriggerGameStarted();
    }

    private void OnAllHeartsLost()
    {
        if (_state != GameState.Playing) return;
        _state = GameState.Lost;

        // save the record BEFORE firing OnGameLost so LoseScreenUI
        // always reads the freshly saved entry
        SaveManager.Instance?.SaveRecord(
            TimerController.Instance != null ? TimerController.Instance.ElapsedTime : 0f
        );

        _arena.DestroyAllTiles();
        GameEvents.TriggerGameLost();
    }

    private void OnTimerEnded()
    {
        if (_state != GameState.Playing) return;
        _state = GameState.Won;
        _arena.OpenDoor();
        GameEvents.TriggerGameWon();
    }

    /// <summary>
    /// Resets the whole game in-place without reloading the scene.
    /// Called by LoseScreenUI and WinScreenUI try/play-again buttons.
    /// </summary>
    public void RestartGame()
    {
        _state = GameState.WaitingForPickup;

        // reset player to start
        _player.SetStartPosition(_playerStartPosition);

        // re-show the golden mask on its pedestal
        _arena.ResetForNewGame();

        // notify all systems — HeartSystem resets hearts,
        // Arena respawns tiles, TimerController stops its countdown
        GameEvents.TriggerGameRestarted();

        // show the walk hint again
        _hintUI?.ResetHint();
    }
}
