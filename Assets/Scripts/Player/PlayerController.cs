using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator   _animator;
    [SerializeField] private TrapConfig _config;
    [SerializeField] private float      _verticalOffset = 0f;

    private static readonly int AnimDirX = Animator.StringToHash("DirX");
    private static readonly int AnimDirY = Animator.StringToHash("DirY");
    private static readonly int AnimMove = Animator.StringToHash("Move");
    private static readonly int AnimFall = Animator.StringToHash("Fall");

    private Vector2Int _gridPosition;
    private bool       _isMoving;
    private bool       _isActive = true;   // false after game won/lost

    private bool _isSuperJumping; // true while super jump arc is playing

    private Tile _standingTile;

    public event Action<Vector2Int> OnPlayerMoved;

    public Vector2Int GridPosition => _gridPosition;
    public bool       IsMoving     => _isMoving;


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


    public void SetStartPosition(Vector2Int startPos)
    {
        _gridPosition      = startPos;
        var wp = TileGrid.GridToWorld(_gridPosition);
        transform.position = new Vector3(wp.x, wp.y + _verticalOffset, wp.z);
    }


    public void TryMove(Vector2Int direction)
    {
        if (_isMoving || !_isActive) return;

        var destination = _gridPosition + direction;
        if (!TileGrid.IsInBounds(destination)) return;

        var tile = TileGrid.GetTile(destination);
        if (tile == null) return;
        
        if (TileGrid.IsBlocked(destination)) return;
        
        if (!tile.IsWalkable)
        {
            StartCoroutine(FallRoutine(_gridPosition));
            return;
        }
        
        StartCoroutine(MoveRoutine(destination, direction));
    }

    public void TrySuperJump(Vector2Int direction)
    {
        // Block input if already moving, jumping, or game is not active
        if (_isMoving || _isSuperJumping || !_isActive) return;

        // Calculate destination 3 tiles away in the given direction
        var destination = _gridPosition + direction * 3;

        // If destination is out of grid bounds, clamp to the furthest valid tile
        // so jumping toward a wall still does something useful
        while (!TileGrid.IsInBounds(destination) && destination != _gridPosition)
        {
            destination -= direction; // step back one tile at a time until in bounds
        }

        // If we couldn't move at all (already at edge), cancel the jump
        if (destination == _gridPosition) return;

        var tile = TileGrid.GetTile(destination);
        if (tile == null) return;
        
        // NEW: super jump also cannot land on a boulder
        if (TileGrid.IsBlocked(destination)) return;
        
        if (!tile.IsWalkable)
        {
            StartCoroutine(FallRoutine(destination));
            return;
        }
        
        StartCoroutine(SuperJumpRoutine(destination, direction));
    }


    private IEnumerator MoveRoutine(Vector2Int destination, Vector2Int direction)
    {
        _isMoving = true;

        float dirXForAnim = direction.y;
        float dirYForAnim = direction.x;

        if (direction.y < 0)
        {
            dirXForAnim = 1f;
            dirYForAnim = 0f;
        }

        _animator.SetFloat(AnimDirX, dirXForAnim);
        _animator.SetFloat(AnimDirY, dirYForAnim);
        _animator.SetTrigger(AnimMove);

        GetComponent<SpriteRenderer>().flipX = (direction.y < 0);

        
        var   from    = transform.position;
        var   toRaw   = TileGrid.GridToWorld(destination);
        var   to      = new Vector3(toRaw.x, toRaw.y + _verticalOffset, toRaw.z);
        float elapsed = 0f;

        while (elapsed < _config.playerMoveSpeed)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / _config.playerMoveSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = to; // already offset
        _gridPosition      = destination;
        _isMoving          = false;

        // track which tile we're on so we detect if it collapses under us
        SubscribeToStandingTile(TileGrid.GetTile(_gridPosition));

        OnPlayerMoved?.Invoke(_gridPosition);
        GameEvents.TriggerPlayerMoved(_gridPosition);
    }

    // Plays the super jump movement arc from current position to destination.
    // Uses the same animator and movement speed as a normal move.
    private IEnumerator SuperJumpRoutine(Vector2Int destination, Vector2Int direction)
    {
        _isSuperJumping = true;
        _isMoving = true; // block further input during jump

        // Reuse existing animation direction logic from MoveRoutine
        float dirXForAnim = direction.y;
        float dirYForAnim = direction.x;

        if (direction.y < 0)
        {
            dirXForAnim = 1f;
            dirYForAnim = 0f;
        }

        _animator.SetFloat(AnimDirX, dirXForAnim);
        _animator.SetFloat(AnimDirY, dirYForAnim);
        _animator.SetTrigger(AnimMove); // uses same move animation for now

        GetComponent<SpriteRenderer>().flipX = (direction.y < 0);

        // Move from current world position to destination world position
        // We use a slightly faster speed for the jump to feel snappier (0.6x normal time)
        // Change this multiplier if the jump feels too fast or too slow
        var   from        = transform.position;
        var   toRaw       = TileGrid.GridToWorld(destination);
        var   to          = new Vector3(toRaw.x, toRaw.y + _verticalOffset, toRaw.z);
        float jumpSpeed   = _config.playerMoveSpeed * 0.6f; // faster than a normal step
        float elapsed     = 0f;

        while (elapsed < jumpSpeed)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / jumpSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = to;
        _gridPosition      = destination;
        _isMoving          = false;
        _isSuperJumping    = false;

        // Subscribe to new tile so we detect if it collapses under us after landing
        SubscribeToStandingTile(TileGrid.GetTile(_gridPosition));

        OnPlayerMoved?.Invoke(_gridPosition);
        GameEvents.TriggerPlayerMoved(_gridPosition);
    }


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

    private void OnStandingTileDestroyed()
    {
        if (_isMoving || !_isActive) return;
        UnsubscribeFromStandingTile(); // prevent double-trigger
        StartCoroutine(FallRoutine(_gridPosition));
    }

    private IEnumerator FallRoutine(Vector2Int holePosition)
    {
        _isMoving = true;
        _animator.SetTrigger(AnimFall);

        var   from    = transform.position;
        var   toRaw   = TileGrid.GridToWorld(holePosition);
        var   to      = new Vector3(toRaw.x, toRaw.y + _verticalOffset, toRaw.z);
        float elapsed = 0f;

        while (elapsed < _config.playerMoveSpeed)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / _config.playerMoveSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        GameEvents.TriggerPlayerFell();

        yield return new WaitForSeconds(0.8f);

        if (_isActive)
        {
            GameEvents.TriggerPlayerRespawned();
            RespawnAtSafeTile();
            StartCoroutine(BlinkRoutine());
        }

        _isMoving = false;
    }

    private IEnumerator BlinkRoutine()
    {
        var sr = GetComponent<SpriteRenderer>();
        float blinkInterval = 0.1f;
        float totalDuration = 1f;
        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        sr.enabled = true;
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
                        var wp = TileGrid.GridToWorld(_gridPosition);
        transform.position = new Vector3(wp.x, wp.y + _verticalOffset, wp.z);
                        SubscribeToStandingTile(tile);
                        GameEvents.TriggerPlayerMoved(_gridPosition);
                        return;
                    }
                }
            }
        }
    }
}
