using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator   _animator;
    [SerializeField] private TrapConfig _config;

    private static readonly int AnimDirX = Animator.StringToHash("DirX");
    private static readonly int AnimDirY = Animator.StringToHash("DirY");
    private static readonly int AnimMove = Animator.StringToHash("Move");
    private static readonly int AnimFall = Animator.StringToHash("Fall");

    private Vector2Int _gridPosition;
    private bool       _isMoving;
    private bool       _isActive = true;   // false after game won/lost

    // the tile the player is currently standing on
    private Tile _standingTile;

    public event Action<Vector2Int> OnPlayerMoved;

    public Vector2Int GridPosition => _gridPosition;
    public bool       IsMoving     => _isMoving;

    // ── Lifecycle ────────────────────────────────────────────────

    private void OnEnable()
    {
        GameEvents.OnGameWon  += OnGameEnded;
        GameEvents.OnGameLost += OnGameEnded;
        GameEvents.OnGameStarted   += OnGameBegan;
        GameEvents.OnGameRestarted += OnGameRestarted;
    }

    private void OnDisable()
    {
        GameEvents.OnGameWon  -= OnGameEnded;
        GameEvents.OnGameLost -= OnGameEnded;
        GameEvents.OnGameStarted   -= OnGameBegan;
        GameEvents.OnGameRestarted -= OnGameRestarted;
        UnsubscribeFromStandingTile();
    }

    private void OnGameBegan()
    {
        _isActive = true;
        // subscribe to the tile we're already standing on when the game starts
        SubscribeToStandingTile(TileGrid.GetTile(_gridPosition));
    }

    private void OnGameEnded()
    {
        _isActive = false;
        UnsubscribeFromStandingTile();
    }

    private void OnGameRestarted()
    {
        _isActive  = true;
        _isMoving  = false;
        UnsubscribeFromStandingTile();
    }

    // ── Setup ────────────────────────────────────────────────────

    public void SetStartPosition(Vector2Int startPos)
    {
        _gridPosition      = startPos;
        transform.position = TileGrid.GridToWorld(_gridPosition);
        // don't subscribe here — TileGrid may not be initialized yet.
        // subscription happens in OnGameBegan instead.
    }

    // ── Input ────────────────────────────────────────────────────

    public void TryMove(Vector2Int direction)
    {
        if (_isMoving || !_isActive) return;

        var destination = _gridPosition + direction;
        if (!TileGrid.IsInBounds(destination)) return;

        var tile = TileGrid.GetTile(destination);
        if (tile == null) return;

        if (!tile.IsWalkable)
        {
            StartCoroutine(FallRoutine(_gridPosition));
            return;
        }

        StartCoroutine(MoveRoutine(destination, direction));
    }

    // ── Movement ─────────────────────────────────────────────────

    private IEnumerator MoveRoutine(Vector2Int destination, Vector2Int direction)
    {
        _isMoving = true;

        _animator.SetFloat(AnimDirX, direction.y);
        _animator.SetFloat(AnimDirY, direction.x);
        _animator.SetTrigger(AnimMove);

        var   from    = transform.position;
        var   to      = TileGrid.GridToWorld(destination);
        float elapsed = 0f;

        while (elapsed < _config.playerMoveSpeed)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / _config.playerMoveSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = to;
        _gridPosition      = destination;
        _isMoving          = false;

        // track which tile we're on so we detect if it collapses under us
        SubscribeToStandingTile(TileGrid.GetTile(_gridPosition));

        OnPlayerMoved?.Invoke(_gridPosition);
        GameEvents.TriggerPlayerMoved(_gridPosition);
    }

    // ── Tile-Under-Player Collapse ────────────────────────────────

    private void SubscribeToStandingTile(Tile tile)
    {
        UnsubscribeFromStandingTile();
        _standingTile = tile;
        if (_standingTile != null)
            _standingTile.OnDestroyed += OnStandingTileDestroyed;
    }

    private void UnsubscribeFromStandingTile()
    {
        if (_standingTile == null) return;
        _standingTile.OnDestroyed -= OnStandingTileDestroyed;
        _standingTile = null;
    }

    /// <summary>
    /// Fires when the tile the player is standing on collapses.
    /// Triggers a fall in place — the most common source of heart loss.
    /// </summary>
    private void OnStandingTileDestroyed()
    {
        if (_isMoving || !_isActive) return;
        UnsubscribeFromStandingTile(); // prevent double-trigger
        StartCoroutine(FallRoutine(_gridPosition));
    }

    // ── Fall ─────────────────────────────────────────────────────

    private IEnumerator FallRoutine(Vector2Int holePosition)
    {
        _isMoving = true;
        _animator.SetTrigger(AnimFall);

        var   from    = transform.position;
        var   to      = TileGrid.GridToWorld(holePosition);
        float elapsed = 0f;

        while (elapsed < _config.playerMoveSpeed)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / _config.playerMoveSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        GameEvents.TriggerPlayerFell();

        yield return new WaitForSeconds(0.8f);

        // only respawn if the game is still running
        if (_isActive)
            RespawnAtSafeTile();

        _isMoving = false;
    }

    private void RespawnAtSafeTile()
    {
        var centre = new Vector2Int(TileGrid.Size / 2, TileGrid.Size / 2);

        for (int radius = 0; radius < TileGrid.Size; radius++)
        {
            for (int r = centre.x - radius; r <= centre.x + radius; r++)
            {
                for (int c = centre.y - radius; c <= centre.y + radius; c++)
                {
                    var tile = TileGrid.GetTile(r, c);
                    // only land on fully Normal tiles — not Cracking ones
                    // which would cause an immediate second fall
                    if (tile != null && tile.State == Tile.TileState.Normal)
                    {
                        _gridPosition      = new Vector2Int(r, c);
                        transform.position = TileGrid.GridToWorld(_gridPosition);
                        SubscribeToStandingTile(tile);
                        GameEvents.TriggerPlayerMoved(_gridPosition);
                        return;
                    }
                }
            }
        }
    }
}
