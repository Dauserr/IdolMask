using UnityEngine;

[CreateAssetMenu(fileName = "TrapConfig", menuName = "IdolMask/TrapConfig")]
public class TrapConfig : ScriptableObject
{
    [Header("Tile Timing")]
    [SerializeField] public float tileCrackDuration = 2f;
    [SerializeField] public float tileDestroyDuration = 3f;
    [SerializeField] public float tileRespawnDuration = 1.5f;

    [Header("Player")]
    [SerializeField] public float playerMoveSpeed = 0.15f;

    [Header("Anger Trap")]
    [SerializeField] public int angerStartDistance = 2;
    [SerializeField] public int angerRadiusMin = 2;
    [SerializeField] public int angerRadiusMax = 5;

    [Header("Joy / Sadness Traps")]
    [SerializeField] public float staggerTimingMin = 0.5f;
    [SerializeField] public float staggerTimingMax = 2.5f;
    // how long between each tile starting to crack as the wave sweeps across a row/column
    [SerializeField] public float tileWaveDelay = 0.15f;

    [Header("Fear Trap")]
    [SerializeField] public int fearStartDistance = 3;
    [SerializeField] public float fearMoveInterval = 0.4f;
    [SerializeField] public float fearMaxTracePercent = 0.3f;

    [Header("Shock Trap")]
    [SerializeField] public int shockStartDistance = 3;
    [SerializeField] public int shockRadius = 3;
    [SerializeField] public int shockStarCount = 3;
    [SerializeField] public float shockStarDelay = 0.8f;
    [SerializeField] public float shockMaxArenaPercent = 0.4f;

    [Header("Game Settings")]
    [SerializeField] public float gameDuration = 120f;
    [SerializeField] public float idolStateChangeIntervalMin = 8f;
    [SerializeField] public float idolStateChangeIntervalMax = 15f;
    [SerializeField] public float trapSpeedScaleStart = 0.3f;
    [SerializeField] public float trapSpeedScaleMax = 2.5f;

    [Header("Input")]
    [SerializeField] public float swipeMinDistance = 50f;
}
