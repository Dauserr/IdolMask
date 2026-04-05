using System.Collections;
using UnityEngine;

public class IdolController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private TrapConfig _config;
    [SerializeField] private GameObject _eyeGlowObject;
    [SerializeField] private float _eyeGlowDuration = 0.35f;

    private static readonly int AnimPeaceful = Animator.StringToHash("Peaceful");
    private static readonly int AnimAnger = Animator.StringToHash("Anger");
    private static readonly int AnimJoy = Animator.StringToHash("Joy");
    private static readonly int AnimSadness = Animator.StringToHash("Sadness");
    private static readonly int AnimFear = Animator.StringToHash("Fear");
    private static readonly int AnimShock = Animator.StringToHash("Shock");

    private IdolState _currentState = IdolState.Peaceful;
    private Coroutine _stateRoutine;
    private Coroutine _glowRoutine;

    public IdolState CurrentState => _currentState;

    private void Start()
    {
        SetState(IdolState.Peaceful, false);
    }

    private void OnEnable()
    {
        GameEvents.OnGameStarted += StartStateLoop;
        GameEvents.OnGameWon += StopAndPeaceful;
        GameEvents.OnGameLost += StopAndPeaceful;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted -= StartStateLoop;
        GameEvents.OnGameWon -= StopAndPeaceful;
        GameEvents.OnGameLost -= StopAndPeaceful;
    }

    private void StartStateLoop()
    {
        StopStateLoop();

        // as soon as the player grabs the mask, the idol starts choosing trap states
        _stateRoutine = StartCoroutine(StateLoopRoutine());
    }

    private IEnumerator StateLoopRoutine()
    {
        ChangeToRandomTrapState();

        while (true)
        {
            float delay = Random.Range(_config.idolStateChangeIntervalMin, _config.idolStateChangeIntervalMax);
            yield return new WaitForSeconds(delay);
            ChangeToRandomTrapState();
        }
    }

    private void ChangeToRandomTrapState()
    {
        IdolState nextState = GetRandomTrapState();

        // avoid repeating the same emotion twice in a row
        while (nextState == _currentState)
            nextState = GetRandomTrapState();

        SetState(nextState, true);
    }

    private IdolState GetRandomTrapState()
    {
        int value = Random.Range(1, 6);
        return (IdolState)value;
    }

    private void StopAndPeaceful()
    {
        StopStateLoop();
        SetState(IdolState.Peaceful, true);
    }

    private void StopStateLoop()
    {
        if (_stateRoutine != null)
        {
            StopCoroutine(_stateRoutine);
            _stateRoutine = null;
        }
    }

    public void SetState(IdolState newState, bool notifySystems)
    {
        _currentState = newState;
        PlayStateAnimation(newState);

        if (_glowRoutine != null)
            StopCoroutine(_glowRoutine);

        _glowRoutine = StartCoroutine(EyeGlowRoutine());

        if (notifySystems)
            GameEvents.TriggerIdolStateChanged(newState);
    }

    private void PlayStateAnimation(IdolState state)
    {
        if (_animator == null) return;

        switch (state)
        {
            case IdolState.Peaceful:
                _animator.SetTrigger(AnimPeaceful);
                break;
            case IdolState.Anger:
                _animator.SetTrigger(AnimAnger);
                break;
            case IdolState.Joy:
                _animator.SetTrigger(AnimJoy);
                break;
            case IdolState.Sadness:
                _animator.SetTrigger(AnimSadness);
                break;
            case IdolState.Fear:
                _animator.SetTrigger(AnimFear);
                break;
            case IdolState.Shock:
                _animator.SetTrigger(AnimShock);
                break;
        }
    }

    private IEnumerator EyeGlowRoutine()
    {
        if (_eyeGlowObject == null)
            yield break;

        _eyeGlowObject.SetActive(true);
        yield return new WaitForSeconds(_eyeGlowDuration);
        _eyeGlowObject.SetActive(false);
    }
}