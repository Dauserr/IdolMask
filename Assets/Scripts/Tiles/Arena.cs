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

    private void Start()
    {
        SpawnTiles();
    }

    private void SpawnTiles()
    {
        _tiles = new Tile[TileGrid.Size, TileGrid.Size];

        // centre the whole grid around Arena's position in the scene
        float offset = (TileGrid.Size - 1) * _tileSize * 0.5f;
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

    // called by GameManager when player grabs the mask
    public void StartGame()
    {
        _goldenMask.SetActive(false);
        _pedestalAnimator.SetTrigger(AnimLower);
        _doorAnimator.SetTrigger(AnimClose);
    }

    // player survived the timer — let them out
    public void OpenDoor()
    {
        _doorAnimator.SetTrigger(AnimOpen);
    }

    // game over — the whole floor collapses
    public void DestroyAllTiles()
    {
        foreach (var tile in _tiles)
            tile.StartDestroy();
    }
}
