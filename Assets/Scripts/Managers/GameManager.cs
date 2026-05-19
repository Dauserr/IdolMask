using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private enum GameState { WaitingForPickup, Playing, Won, Lost }

    [SerializeField] private Arena arena;
    [SerializeField] private PlayerController player;
    [SerializeField] private TrapConfig config;
    [SerializeField] private HintUI hintUI;
    [SerializeField] private Vector2Int maskGridPosition = new Vector2Int(7, 7);
    [SerializeField] private Vector2Int playerStartPosition = new Vector2Int(0, 7);

    private GameState state = GameState.WaitingForPickup;
    private bool _firstMoveDone = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(InitAfterGrid());
        GameEvents.OnPlayerMoved += OnPlayerMoved;
        GameEvents.OnAllHeartsLost += OnAllHeartsLost;
        GameEvents.OnTimerEnded += OnTimerEnded;
    }

    private IEnumerator InitAfterGrid()
    {
        yield return null; // wait one frame for TileGrid to build
        player.SetStartPosition(playerStartPosition);
    }
    private void OnDestroy()
    {
        GameEvents.OnPlayerMoved -= OnPlayerMoved;
        GameEvents.OnAllHeartsLost -= OnAllHeartsLost;
        GameEvents.OnTimerEnded -= OnTimerEnded;
    }

    private void OnPlayerMoved(Vector2Int position)
    {
        if (!_firstMoveDone)
        {
            _firstMoveDone = true;
            GameEvents.TriggerPlayerFirstMoved();
        }

        if (state != GameState.WaitingForPickup) return;
        if (position == maskGridPosition) StartGame();
    }

    private void StartGame()
    {
        state = GameState.Playing;
        arena.StartGame();
        GameEvents.TriggerGameStarted();
    }

    private void OnAllHeartsLost()
    {
        if (state != GameState.Playing) return;
        state = GameState.Lost;
        SaveManager.Instance?.SaveRecord(
            TimerController.Instance != null ? TimerController.Instance.ElapsedTime : 0f);
        arena.DestroyAllTiles();
        GameEvents.TriggerGameLost();
    }

    private void OnTimerEnded()
    {
        if (state != GameState.Playing) return;
        state = GameState.Won;
        GameEvents.TriggerGameWon();
        StartCoroutine(WinSequence());
    }

    private IEnumerator WinSequence()
    {
        // Wait for idol to reach Peaceful (handled by StopAndPeaceful via TriggerGameWon)
        float peacefulClipLength = 1f; // match your longest transition clip
        yield return new WaitForSeconds(peacefulClipLength + 0.2f);

        // Now trigger close eyes
        GameEvents.TriggerIdolDestroyed();
    }

    public void RestartGame()
    {
        state = GameState.WaitingForPickup;
        _firstMoveDone = false;
        arena.ResetForNewGame();
        GameEvents.TriggerGameRestarted();
        hintUI?.ResetHint();
        StartCoroutine(InitAfterGrid()); // wait one frame for TileGrid to build
    }
}