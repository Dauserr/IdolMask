using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator   _animator;
    [SerializeField] private TrapConfig _config;

    // animator parameter hashes — faster than passing strings every frame
    private static readonly int AnimDirX = Animator.StringToHash("DirX");
    private static readonly int AnimDirY = Animator.StringToHash("DirY");
    private static readonly int AnimMove = Animator.StringToHash("Move");
    private static readonly int AnimFall = Animator.StringToHash("Fall");

    private Vector2Int _gridPosition;
    private bool       _isMoving;

    // InputManager and FearTrap both need to know where the player is
    public event Action<Vector2Int> OnPlayerMoved;

    public Vector2Int GridPosition => _gridPosition;
    public bool       IsMoving     => _isMoving;

    public void SetStartPosition(Vector2Int startPos)
    {
        _gridPosition      = startPos;
        transform.position = TileGrid.GridToWorld(_gridPosition);
    }

    public void TryMove(Vector2Int direction)
    {
        if (_isMoving) return;

        var destination = _gridPosition + direction;

        if (!TileGrid.IsInBounds(destination)) return;

        var tile = TileGrid.GetTile(destination);
        if (tile == null) return;

        if (!tile.IsWalkable)
        {
            // player stepped on a destroyed tile — fall and lose a heart
            StartCoroutine(FallRoutine(destination));
            return;
        }

        StartCoroutine(MoveRoutine(destination, direction));
    }

    private IEnumerator MoveRoutine(Vector2Int destination, Vector2Int direction)
    {
        _isMoving = true;

        // direction.y = col delta = world X, direction.x = row delta = world Y
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

        OnPlayerMoved?.Invoke(_gridPosition);
        GameEvents.TriggerPlayerMoved(_gridPosition);
    }

    private IEnumerator FallRoutine(Vector2Int holePosition)
    {
        _isMoving = true;
        _animator.SetTrigger(AnimFall);

        // slide the player into the hole before triggering heart loss
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

        // short pause so the fall animation can finish
        yield return new WaitForSeconds(0.8f);

        RespawnAtSafeTile();
        _isMoving = false;
    }

    private void RespawnAtSafeTile()
    {
        // start from grid centre and spiral outward until we find a walkable tile
        var centre = new Vector2Int(TileGrid.Size / 2, TileGrid.Size / 2);

        for (int radius = 0; radius < TileGrid.Size; radius++)
        {
            for (int r = centre.x - radius; r <= centre.x + radius; r++)
            {
                for (int c = centre.y - radius; c <= centre.y + radius; c++)
                {
                    var tile = TileGrid.GetTile(r, c);
                    if (tile != null && tile.IsWalkable)
                    {
                        _gridPosition      = new Vector2Int(r, c);
                        transform.position = TileGrid.GridToWorld(_gridPosition);
                        return;
                    }
                }
            }
        }
    }
}
