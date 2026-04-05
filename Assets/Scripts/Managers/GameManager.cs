using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // all possible states the game can be in
    private enum GameState { WaitingForPickup, Playing, Won, Lost }

    [SerializeField] private Arena            _arena;
    [SerializeField] private PlayerController _player;
    [SerializeField] private TrapConfig       _config;

    // where the mask/pedestal sits on the grid — centre of 15x15 is (7,7)
    [SerializeField] private Vector2Int _maskGridPosition = new(7, 7);

    // where the player enters the arena from
    [SerializeField] private Vector2Int _playerStartPosition = new(0, 7);

    private GameState _state = GameState.WaitingForPickup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _player.SetStartPosition(_playerStartPosition);

        GameEvents.OnPlayerMoved   += OnPlayerMoved;
        GameEvents.OnAllHeartsLost += OnAllHeartsLost;
        GameEvents.OnTimerEnded    += OnTimerEnded;
    }

    // check every move — did the player just reach the mask?
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

    private void OnDestroy()
    {
        GameEvents.OnPlayerMoved   -= OnPlayerMoved;
        GameEvents.OnAllHeartsLost -= OnAllHeartsLost;
        GameEvents.OnTimerEnded    -= OnTimerEnded;
    }
}
