using System.Collections;
using UnityEngine;

public class IdolController : MonoBehaviour
{
    [SerializeField] private TrapConfig _config;

    private IdolState _currentState = IdolState.Peaceful;
    private Coroutine _stateRoutine;

    public IdolState CurrentState => _currentState;

    private void Start()
    {
        SetState(IdolState.Peaceful, false);
    }

    private void OnEnable()
    {
        GameEvents.OnGameStarted   += StartStateLoop;
        GameEvents.OnGameWon       += StopAndPeaceful;
        GameEvents.OnGameLost      += StopAndPeaceful;
        GameEvents.OnGameRestarted += StopAndPeaceful;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted   -= StartStateLoop;
        GameEvents.OnGameWon       -= StopAndPeaceful;
        GameEvents.OnGameLost      -= StopAndPeaceful;
        GameEvents.OnGameRestarted -= StopAndPeaceful;
    }

    private void StartStateLoop()
    {
        StopStateLoop();
        _stateRoutine = StartCoroutine(StateLoopRoutine());
    }

    private IEnumerator StateLoopRoutine()
    {
        ChangeToRandomTrapState();

        while (true)
        {
            float delay = Random.Range(
                _config.idolStateChangeIntervalMin,
                _config.idolStateChangeIntervalMax);
            yield return new WaitForSeconds(delay);
            ChangeToRandomTrapState();
        }
    }

    private void ChangeToRandomTrapState()
    {
        IdolState next = GetRandomTrapState();

        // avoid same emotion twice in a row
        while (next == _currentState)
            next = GetRandomTrapState();

        SetState(next, true);
    }

    private IdolState GetRandomTrapState()
    {
        return (IdolState)Random.Range(1, 6);
    }

    private void StopAndPeaceful()
    {
        StopStateLoop();
        SetState(IdolState.Peaceful, true);
    }

    private void StopStateLoop()
    {
        if (_stateRoutine == null) return;
        StopCoroutine(_stateRoutine);
        _stateRoutine = null;
    }

    public void SetState(IdolState newState, bool notifySystems)
    {
        _currentState = newState;

        if (notifySystems)
            GameEvents.TriggerIdolStateChanged(newState);
    }
}
