using System.Collections;
using UnityEngine;
 

public class BoulderTile : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
 
    public bool IsActive { get; private set; }
 
    private Animator _animator;
 
    private static readonly int AnimSpawn   = Animator.StringToHash("Spawn");
    private static readonly int AnimDespawn = Animator.StringToHash("Despawn");
 
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
 
    public void Spawn(Vector2Int gridPosition)
    {
        GridPosition = gridPosition;
        IsActive     = true;
 
        var worldPos       = TileGrid.GridToWorld(gridPosition);
        transform.position = new Vector3(worldPos.x, worldPos.y, -0.1f); // slightly in front of floor
 
        gameObject.SetActive(true);
 
        if (_animator != null)
            _animator.SetTrigger(AnimSpawn);
 
        GameEvents.TriggerBoulderSpawned(gridPosition);
    }
 
    public void Despawn()
    {
        if (!IsActive) return;
        StartCoroutine(DespawnRoutine());
    }
 
    private IEnumerator DespawnRoutine()
    {
        IsActive = false; // immediately unblock the tile for movement checks
 
        if (_animator != null)
        {
            _animator.SetTrigger(AnimDespawn);
            yield return new WaitForSeconds(0.5f);
        }
 
        GameEvents.TriggerBoulderRemoved(GridPosition);
        gameObject.SetActive(false);
    }
 
    public void ForceHide()
    {
        StopAllCoroutines();
        IsActive = false;
        gameObject.SetActive(false);
    }
}
