using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class BoulderTrap : MonoBehaviour
{
    [SerializeField] private BoulderTile _boulderPrefab;
 
    private readonly List<BoulderTile> _activeBoulders = new();
 
    private readonly List<BoulderTile> _pool = new();
 
    private Coroutine _spawnRoutine;
  
    public void Activate(Vector2Int playerPos, TrapConfig config, float speedMultiplier)
    {
        StopSpawnRoutine();
        _spawnRoutine = StartCoroutine(SpawnRoutine(playerPos, config, speedMultiplier));
    }
 
    public void Deactivate()
    {
        StopSpawnRoutine();
 
        foreach (var boulder in _activeBoulders)
        {
            if (boulder != null)
            {
                TileGrid.SetBlocked(boulder.GridPosition, false);
                boulder.Despawn();
            }
        }
        _activeBoulders.Clear();
    }
  
    private IEnumerator SpawnRoutine(Vector2Int playerPos, TrapConfig config, float speedMultiplier)
    {
        var positions = PickBoulderPositions(playerPos, config.boulderCount, config.boulderSafeRadius);
 
        foreach (var pos in positions)
        {
            var boulder = GetFromPool();
            boulder.Spawn(pos);
            _activeBoulders.Add(boulder);
 
            TileGrid.SetBlocked(pos, true);
 
            yield return new WaitForSeconds(config.boulderSpawnDelay / speedMultiplier);
        }
    }
 

    private List<Vector2Int> PickBoulderPositions(Vector2Int playerPos, int count, int safeRadius)
    {
        var result = new List<Vector2Int>();
        var used   = new HashSet<Vector2Int>();
 
        int attempts    = 0;
        int maxAttempts = count * 20; // prevent infinite loop
 
        while (result.Count < count && attempts < maxAttempts)
        {
            attempts++;
 
            int row = Random.Range(0, TileGrid.Size);
            int col = Random.Range(0, TileGrid.Size);
            var pos = new Vector2Int(row, col);
 
            if (used.Contains(pos)) continue;
 
            int dist = Mathf.Abs(pos.x - playerPos.x) + Mathf.Abs(pos.y - playerPos.y);
            if (dist <= safeRadius) continue;
 
            if (TileGrid.GetTile(pos) == null) continue;
 
            result.Add(pos);
            used.Add(pos);
        }
 
        return result;
    }
 
    private BoulderTile GetFromPool()
    {
        foreach (var b in _pool)
        {
            if (b != null && !b.IsActive && !b.gameObject.activeSelf)
                return b;
        }
 
        var newBoulder = Instantiate(_boulderPrefab, transform);
        newBoulder.gameObject.SetActive(false);
        _pool.Add(newBoulder);
        return newBoulder;
    }
 
    private void StopSpawnRoutine()
    {
        if (_spawnRoutine == null) return;
        StopCoroutine(_spawnRoutine);
        _spawnRoutine = null;
    }
 
    public void ForceCleanup()
    {
        StopSpawnRoutine();
        foreach (var boulder in _activeBoulders)
        {
            if (boulder != null)
            {
                TileGrid.SetBlocked(boulder.GridPosition, false);
                boulder.ForceHide();
            }
        }
        _activeBoulders.Clear();
    }
 
    private void OnDestroy() => StopSpawnRoutine();
}
