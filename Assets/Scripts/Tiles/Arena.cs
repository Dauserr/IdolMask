using UnityEngine;

public class Arena : MonoBehaviour
{
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private Animator   _doorAnimator;
    [SerializeField] private Animator   _pedestalAnimator;
    [SerializeField] private GameObject _goldenMask;
    [SerializeField] private float      _tileSize = 1f;
    [SerializeField] private TrapConfig _config;

    private static readonly int AnimClose = Animator.StringToHash("Close");
    private static readonly int AnimOpen  = Animator.StringToHash("Open");
    private static readonly int AnimLower = Animator.StringToHash("Lower");

    private Tile[,] _tiles;

    public static Arena Instance { get; private set; }

    private void Awake() => Instance = this;

    private void Start() => SpawnTiles();

    private void OnEnable()
    {
        GameEvents.OnGameStarted   += OnGameStarted;
        GameEvents.OnGameRestarted += OnGameRestarted;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted   -= OnGameStarted;
        GameEvents.OnGameRestarted -= OnGameRestarted;
    }

    private void OnGameStarted()  => ForceRespawnAll();
    private void OnGameRestarted() => ForceRespawnAll();

    private void SpawnTiles()
    {
        _tiles = new Tile[TileGrid.Size, TileGrid.Size];

        float   offset = (TileGrid.Size - 1) * _tileSize * 0.5f;
        Vector3 origin = transform.position - new Vector3(offset, offset, 0f);

        for (int row = 0; row < TileGrid.Size; row++)
        {
            for (int col = 0; col < TileGrid.Size; col++)
            {
                var worldPos = origin + new Vector3(col * _tileSize, row * _tileSize, 0f);
                var tileObj  = Instantiate(_tilePrefab, worldPos, Quaternion.identity, transform);
                tileObj.name = $"Tile_{row}_{col}";

                var tile = tileObj.GetComponent<Tile>();
                tile.Initialize(_config);
                _tiles[row, col] = tile;
            }
        }

        TileGrid.Initialize(_tiles, origin, _tileSize);
    }

    public void ForceRespawnAll()
    {
        foreach (var tile in _tiles)
            if (tile.State != Tile.TileState.Normal)
                tile.ForceReset();
    }

    /// <summary>
    /// Resets the arena for a new run — respawns tiles, shows mask, opens door.
    /// Pedestal stays down (no Raise animation needed).
    /// </summary>
    public void ResetForNewGame()
    {
        ForceRespawnAll();

        if (_goldenMask != null)
            _goldenMask.SetActive(true);

        if (_doorAnimator != null)
            _doorAnimator.SetTrigger(AnimOpen);
    }

    public void StartGame()
    {
        _goldenMask.SetActive(false);
        _pedestalAnimator.SetTrigger(AnimLower);
        _doorAnimator.SetTrigger(AnimClose);
    }

    public void OpenDoor() => _doorAnimator.SetTrigger(AnimOpen);

    public void DestroyAllTiles()
    {
        foreach (var tile in _tiles)
            tile.StartDestroy();
    }
}
