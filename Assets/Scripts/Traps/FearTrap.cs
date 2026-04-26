using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FearTrap : MonoBehaviour
{
    private readonly List<Tile> _traceTiles = new();
    private Coroutine _chaseRoutine;

    // max tiles the trace can cover before the trap stops
    private int _maxTraceTiles;

    private void OnEnable()
    {
        // subscribe so the trap always knows where the player is
        GameEvents.OnPlayerMoved += OnPlayerMoved;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerMoved -= OnPlayerMoved;
    }

    public void Activate(Vector2Int playerPos, TrapConfig config, float speedMultiplier)
    {
        StopChaseRoutine();
        _traceTiles.Clear();

        // calculate how many tiles the trace is allowed to cover
        int totalTiles = TileGrid.Size * TileGrid.Size;
        _maxTraceTiles = Mathf.FloorToInt(totalTiles * config.fearMaxTracePercent);

        _currentPlayerPos = playerPos;
        _chaseRoutine = StartCoroutine(ChaseRoutine(playerPos, config, speedMultiplier));
    }

    private IEnumerator ChaseRoutine(Vector2Int startPos, TrapConfig config, float speedMultiplier)
    {
        // start somewhere near the player, not exactly on them
        var currentPos = GetStartPosition(startPos, config.fearStartDistance);

        while (_traceTiles.Count < _maxTraceTiles)
        {
            // destroy the tile at current position and track it
            var tile = TileGrid.GetTile(currentPos);
            if (tile != null && !_traceTiles.Contains(tile))
            {
                _traceTiles.Add(tile);
                tile.StartCrack();
            }

            // move one step toward the player
            currentPos = StepToward(currentPos, _currentPlayerPos);

            yield return new WaitForSeconds(config.fearMoveInterval / speedMultiplier);
        }
    }

    // find a valid start position near but not on the player
    private Vector2Int GetStartPosition(Vector2Int playerPos, int maxDistance)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            int row = playerPos.x + Random.Range(-maxDistance, maxDistance + 1);
            int col = playerPos.y + Random.Range(-maxDistance, maxDistance + 1);

            if (TileGrid.IsInBounds(row, col) && new Vector2Int(row, col) != playerPos)
                return new Vector2Int(row, col);
        }
        // fallback — just offset by one tile
        return new Vector2Int(
            Mathf.Clamp(playerPos.x + 1, 0, TileGrid.Size - 1),
            Mathf.Clamp(playerPos.y + 1, 0, TileGrid.Size - 1)
        );
    }

    // move one step in the direction of the target
    private Vector2Int StepToward(Vector2Int current, Vector2Int target)
    {
        int rowDiff = target.x - current.x;
        int colDiff = target.y - current.y;

        // move along whichever axis has the bigger gap
        if (Mathf.Abs(rowDiff) >= Mathf.Abs(colDiff))
            return new Vector2Int(current.x + (int)Mathf.Sign(rowDiff), current.y);
        else
            return new Vector2Int(current.x, current.y + (int)Mathf.Sign(colDiff));
    }

    public void Deactivate()
    {
        StopChaseRoutine();

        foreach (var tile in _traceTiles)
        {
            if (tile != null)
                tile.ForceReset();
        }
        _traceTiles.Clear();
    }

    private void StopChaseRoutine()
    {
        if (_chaseRoutine == null) return;
        StopCoroutine(_chaseRoutine);
        _chaseRoutine = null;
    }

    // keep track of player position via Observer event
    private Vector2Int _currentPlayerPos;
    private void OnPlayerMoved(Vector2Int newPos) => _currentPlayerPos = newPos;

    private void OnDestroy()
    {
        StopChaseRoutine();
        GameEvents.OnPlayerMoved -= OnPlayerMoved;
    }
}
