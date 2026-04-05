using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngerTrap : MonoBehaviour
{
    private readonly List<Tile> _affectedTiles = new();
    private Coroutine _activateRoutine;

    public void Activate(Vector2Int playerPos, TrapConfig config, float speedMultiplier)
    {
        StopActivateRoutine();
        _activateRoutine = StartCoroutine(ActivateRoutine(playerPos, config, speedMultiplier));
    }

    private IEnumerator ActivateRoutine(Vector2Int playerPos, TrapConfig config, float speedMultiplier)
    {
        // pick a random start point near the player
        int startRow = playerPos.x + Random.Range(-config.angerStartDistance, config.angerStartDistance + 1);
        int startCol = playerPos.y + Random.Range(-config.angerStartDistance, config.angerStartDistance + 1);
        startRow = Mathf.Clamp(startRow, 0, TileGrid.Size - 1);
        startCol = Mathf.Clamp(startCol, 0, TileGrid.Size - 1);

        int radius = Random.Range(config.angerRadiusMin, config.angerRadiusMax + 1);

        var tiles = TileGrid.GetNeighbors(startRow, startCol, radius);

        // crack tiles one by one with a tiny stagger so the spread looks organic
        foreach (var tile in tiles)
        {
            if (tile == null) continue;
            _affectedTiles.Add(tile);
            tile.StartCrack();
            yield return new WaitForSeconds(0.05f / speedMultiplier);
        }
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
    }

    private void StopActivateRoutine()
    {
        if (_activateRoutine == null) return;
        StopCoroutine(_activateRoutine);
        _activateRoutine = null;
    }

    private void OnDestroy() => StopActivateRoutine();
}
