using System;
using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileState { Normal, Cracking, Destroyed, Respawning }

    [SerializeField] private Animator _animator;

    private static readonly int AnimCrack   = Animator.StringToHash("Crack");
    private static readonly int AnimDestroy = Animator.StringToHash("Destroy");
    private static readonly int AnimRespawn = Animator.StringToHash("Respawn");
    private static readonly int AnimNormal  = Animator.StringToHash("Normal");

    private TileState  _state = TileState.Normal;
    private Coroutine  _currentCoroutine;
    private TrapConfig _config;

    public event Action OnDestroyed;
    public event Action OnRespawned;

    public TileState State      => _state;
    public bool      IsWalkable => _state == TileState.Normal || _state == TileState.Cracking;

    public void Initialize(TrapConfig config)
    {
        _config = config;
    }

    // ── Public API ───────────────────────────────────────────────

    /// <summary>Begins cracking animation, then auto-transitions to Destroyed.</summary>
    public void StartCrack()
    {
        if (_state != TileState.Normal) return;
        StopCurrentCoroutine();
        _currentCoroutine = StartCoroutine(CrackRoutine());
    }

    /// <summary>Immediately destroys the tile (skips crack phase).</summary>
    public void StartDestroy()
    {
        if (_state == TileState.Destroyed) return;
        StopCurrentCoroutine();
        _currentCoroutine = StartCoroutine(DestroyRoutine());
    }

    /// <summary>Respawns tile back to Normal from any non-Normal state.</summary>
    public void StartRespawn()
    {
        if (_state == TileState.Normal) return;
        StopCurrentCoroutine();
        _currentCoroutine = StartCoroutine(RespawnRoutine());
    }

    // ── Coroutines ───────────────────────────────────────────────

    private IEnumerator CrackRoutine()
    {
        _state = TileState.Cracking;
        _animator.SetTrigger(AnimCrack);
        yield return new WaitForSeconds(_config.tileCrackDuration);
        _currentCoroutine = StartCoroutine(DestroyRoutine());
    }

    private IEnumerator DestroyRoutine()
    {
        _state = TileState.Destroyed;
        _animator.SetTrigger(AnimDestroy);
        OnDestroyed?.Invoke();
        yield return new WaitForSeconds(_config.tileDestroyDuration);
    }

    private IEnumerator RespawnRoutine()
    {
        _state = TileState.Respawning;
        _animator.SetTrigger(AnimRespawn);
        yield return new WaitForSeconds(_config.tileRespawnDuration);
        _state = TileState.Normal;
        _animator.SetTrigger(AnimNormal);
        OnRespawned?.Invoke();
    }

    // ── Helpers ──────────────────────────────────────────────────

    private void StopCurrentCoroutine()
    {
        if (_currentCoroutine == null) return;
        StopCoroutine(_currentCoroutine);
        _currentCoroutine = null;
    }

    private void OnDestroy()
    {
        StopCurrentCoroutine();
    }
}
