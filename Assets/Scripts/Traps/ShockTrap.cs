using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockTrap : MonoBehaviour
{
    private readonly List<Tile>       _affectedTiles = new();
    private readonly HashSet<Vector2Int> _usedPositions = new();
    private Coroutine _activateRoutine;

    public void Activate(Vector2Int playerPos, TrapConfig config, float speedMultiplier)
    {
        StopActivateRoutine();
        _activateRoutine = StartCoroutine(ActivateRoutine(playerPos, config, speedMultiplier));
    }

    private IEnumerator ActivateRoutine(Vector2Int playerPos, TrapConfig config, float speedMultiplier)
    {
        int totalTiles  = TileGrid.Size * TileGrid.Size;
        int maxTiles    = Mathf.FloorToInt(totalTiles * config.shockMaxArenaPercent);
        int tilesPlaced = 0;

        // first star appears near the player, the rest are random
        Vector2Int firstCenter = GetPositionNearPlayer(playerPos, config.shockStartDistance);

        for (int star = 0; star < config.shockStarCount; star++)
        {
            if (tilesPlaced >= maxTiles) break;

            Vector2Int center = star == 0
                ? firstCenter
                : GetRandomNonOverlappingCenter(config.shockRadius);

            if (center == Vector2Int.one * -1) break; // no valid position found

            // place all 8 direction arms of the star
            var starTiles = GetStarTiles(center, config.shockRadius);

            foreach (var pos in starTiles)
            {
                if (tilesPlaced >= maxTiles) break;
                if (_usedPositions.Contains(pos)) continue;

                var tile = TileGrid.GetTile(pos);
                if (tile == null) continue;

                _usedPositions.Add(pos);
                _affectedTiles.Add(tile);
                tile.StartCrack();
                tilesPlaced++;
            }

            // short pause before the next star appears
            yield return new WaitForSeconds(config.shockStarDelay / speedMultiplier);
        }
    }

    // calculates all tile positions that form a star shape from a center point
    // a star = the center tile + tiles extending in all 8 diagonal/cardinal directions
    private List<Vector2Int> GetStarTiles(Vector2Int center, int radius)
    {
        var tiles = new List<Vector2Int> { center };

        // 8 directions: up, down, left, right, and all 4 diagonals
        var directions = new Vector2Int[]
        {
            new( 1,  0), new(-1,  0),  // up, down
            new( 0,  1), new( 0, -1),  // right, left
            new( 1,  1), new( 1, -1),  // up-right, up-left
            new(-1,  1), new(-1, -1)   // down-right, down-left
        };

        foreach (var dir in directions)
        {
            for (int step = 1; step <= radius; step++)
            {
                var pos = center + dir * step;
                if (TileGrid.IsInBounds(pos))
                    tiles.Add(pos);
            }
        }

        return tiles;
    }

    private Vector2Int GetPositionNearPlayer(Vector2Int playerPos, int maxDistance)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            int row = playerPos.x + Random.Range(-maxDistance, maxDistance + 1);
            int col = playerPos.y + Random.Range(-maxDistance, maxDistance + 1);
            var pos = new Vector2Int(row, col);

            if (TileGrid.IsInBounds(pos))
                return pos;
        }
        return playerPos;
    }

    private Vector2Int GetRandomNonOverlappingCenter(int radius)
    {
        // try random positions until we find one whose star won't overlap existing stars
        for (int attempt = 0; attempt < 20; attempt++)
        {
            int row = Random.Range(radius, TileGrid.Size - radius);
            int col = Random.Range(radius, TileGrid.Size - radius);
            var candidate = new Vector2Int(row, col);

            // check none of this star's tiles are already used
            bool overlaps = false;
            foreach (var pos in GetStarTiles(candidate, radius))
            {
                if (_usedPositions.Contains(pos))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps) return candidate;
        }

        // signal that no valid position was found
        return Vector2Int.one * -1;
    }

    public void Deactivate()
    {
        StopActivateRoutine();

        foreach (var tile in _affectedTiles)
        {
            if (tile != null)
                tile.StartRespawn();
        }

        _affectedTiles.Clear();
        _usedPositions.Clear();
    }

    private void StopActivateRoutine()
    {
        if (_activateRoutine == null) return;
        StopCoroutine(_activateRoutine);
        _activateRoutine = null;
    }

    private void OnDestroy() => StopActivateRoutine();
}
