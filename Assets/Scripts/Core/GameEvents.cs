using System;
using UnityEngine;

/// Central Observer hub. Systems raise events here; listeners subscribe here.
/// Unsubscribe in OnDestroy to prevent memory leaks.
public static class GameEvents
{
    // ── Game Flow ────────────────────────────────────────────────
    public static event Action OnGameStarted;
    public static event Action OnGameWon;
    public static event Action OnGameLost;

    // ── Player ───────────────────────────────────────────────────
    public static event Action<Vector2Int> OnPlayerMoved;
    public static event Action             OnPlayerFell;

    // ── Hearts ───────────────────────────────────────────────────
    public static event Action<int> OnHeartLost;      // passes hearts remaining
    public static event Action      OnAllHeartsLost;

    // ── Idol / Traps ─────────────────────────────────────────────
    public static event Action<IdolState> OnIdolStateChanged;

    // ── Timer ────────────────────────────────────────────────────
    public static event Action<float> OnTimerTick;    // normalised 0 → 1
    public static event Action        OnTimerEnded;

    // ── Save ─────────────────────────────────────────────────────
    public static event Action OnRecordSaved;

    // ── Triggers ─────────────────────────────────────────────────
    public static void TriggerGameStarted()                    => OnGameStarted?.Invoke();
    public static void TriggerGameWon()                        => OnGameWon?.Invoke();
    public static void TriggerGameLost()                       => OnGameLost?.Invoke();
    public static void TriggerPlayerMoved(Vector2Int position) => OnPlayerMoved?.Invoke(position);
    public static void TriggerPlayerFell()                     => OnPlayerFell?.Invoke();
    public static void TriggerHeartLost(int remaining)         => OnHeartLost?.Invoke(remaining);
    public static void TriggerAllHeartsLost()                  => OnAllHeartsLost?.Invoke();
    public static void TriggerIdolStateChanged(IdolState state)=> OnIdolStateChanged?.Invoke(state);
    public static void TriggerTimerTick(float normalized)      => OnTimerTick?.Invoke(normalized);
    public static void TriggerTimerEnded()                     => OnTimerEnded?.Invoke();
    public static void TriggerRecordSaved()                    => OnRecordSaved?.Invoke();
}
