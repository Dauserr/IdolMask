using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdolController : MonoBehaviour
{
    [SerializeField] private TrapConfig _config;

    private Animator _animator;
    private IdolState _currentState = IdolState.Peaceful;
    private Coroutine _stateRoutine;
    private Coroutine _transitionRoutine;

    public IdolState CurrentState => _currentState;

    private static readonly Dictionary<(IdolState, IdolState), string> _clips = new()
    {
        { (IdolState.Peaceful, IdolState.Anger),   "Peaceful_ToAnger" },
        { (IdolState.Peaceful, IdolState.Joy),     "Peaceful_ToJoy" },
        { (IdolState.Peaceful, IdolState.Sadness), "Peaceful_ToSadness" },
        { (IdolState.Peaceful, IdolState.Shock),   "Peaceful_ToShock" },
        { (IdolState.Anger,    IdolState.Fear),    "Anger_ToFear" },
        { (IdolState.Anger,    IdolState.Joy),     "Anger_ToJoy" },
        { (IdolState.Anger,    IdolState.Shock),   "Anger_ToShock" },
        { (IdolState.Fear,     IdolState.Joy),     "Fear_ToJoy" },
        { (IdolState.Fear,     IdolState.Sadness), "Fear_ToSadness" },
        { (IdolState.Fear,     IdolState.Shock),   "Fear_ToShock" },
        { (IdolState.Joy,      IdolState.Sadness), "Joy_ToSadness" },
        { (IdolState.Shock,    IdolState.Joy),     "Shock_ToJoy" },
        { (IdolState.Shock,    IdolState.Sadness), "Shock_ToSadness" },
    };

    private static int StateToInt(IdolState state) => state switch
    {
        IdolState.Peaceful => 1,
        IdolState.Anger    => 2,
        IdolState.Fear     => 3,
        IdolState.Joy      => 4,
        IdolState.Sadness  => 5,
        IdolState.Shock    => 6,
        _                  => 1
    };

    private float GetClipLength(string clipName)
    {
        foreach (var clip in _animator.runtimeAnimatorController.animationClips)
            if (clip.name == clipName) return clip.length;
        return 0.5f;
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _animator.SetInteger("FromState", 0);
        _animator.SetInteger("ToState", 0);
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerFirstMoved += OpenEyes;
        GameEvents.OnGameStarted      += StartStateLoop;
        GameEvents.OnGameWon          += StopAndPeaceful;
        GameEvents.OnGameLost         += StopAndPeaceful;
        GameEvents.OnGameRestarted    += ResetToClosedEyes;
        GameEvents.OnIdolDestroyed += OnDestroyed;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerFirstMoved -= OpenEyes;
        GameEvents.OnGameStarted      -= StartStateLoop;
        GameEvents.OnGameWon          -= StopAndPeaceful;
        GameEvents.OnGameLost         -= StopAndPeaceful;
        GameEvents.OnGameRestarted    -= ResetToClosedEyes;
        GameEvents.OnIdolDestroyed -= OnDestroyed;
    }
    private void OnDestroyed()
    {
        StopStateLoop();
        StopTransition();
        _transitionRoutine = StartCoroutine(CloseEyesAndDestroy());
    }

    private IEnumerator CloseEyesAndDestroy()
    {
        // Step 1: Peaceful → ClosedEyes
        _animator.SetInteger("FromState", 1);
        _animator.SetInteger("ToState", 0);
        yield return null;
        yield return new WaitForSeconds(GetClipLength("ClosedEyes_ToPeaceful"));

        // Step 2: ClosedEyes → Destroy animation
        _animator.SetInteger("FromState", 0);
        _animator.SetInteger("ToState", 99);
        yield return null;
        yield return new WaitForSeconds(GetClipLength("ClosedEyes_Destroy"));

        GameEvents.TriggerShowWinScreen();
        Destroy(gameObject);
    }

    private void OpenEyes()
    {
        StopTransition();
        _transitionRoutine = StartCoroutine(OpenEyesRoutine());
    }

    private IEnumerator OpenEyesRoutine()
    {
        _animator.SetInteger("FromState", 0);
        _animator.SetInteger("ToState", 1);
        yield return null;
        yield return new WaitForSeconds(GetClipLength("ClosedEyes_ToPeaceful"));
        _currentState = IdolState.Peaceful;
        _animator.SetInteger("FromState", 1);
        _animator.SetInteger("ToState", 1);
        yield return null;
    }

    private void ResetToClosedEyes()
    {
        StopStateLoop();
        StopTransition();
        _animator.SetInteger("FromState", 0);
        _animator.SetInteger("ToState", 0);
        _currentState = IdolState.Peaceful;
    }

    private void StartStateLoop()
    {
        StopStateLoop();
        _stateRoutine = StartCoroutine(StateLoopRoutine());
    }

    private IEnumerator StateLoopRoutine()
    {
        // Wait for eye opening to finish before starting
        yield return new WaitForSeconds(GetClipLength("ClosedEyes_ToPeaceful") + 0.2f);

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
        while (next == _currentState)
            next = GetRandomTrapState();
        SetState(next, true);
    }

    private IdolState GetRandomTrapState() => (IdolState)Random.Range(1, 6);

    private void StopAndPeaceful()
    {
        StopStateLoop();
        StopTransition();
        SetState(IdolState.Peaceful, true);
    }

    private void StopStateLoop()
    {
        if (_stateRoutine == null) return;
        StopCoroutine(_stateRoutine);
        _stateRoutine = null;
    }

    private void StopTransition()
    {
        if (_transitionRoutine == null) return;
        StopCoroutine(_transitionRoutine);
        _transitionRoutine = null;
    }

    public void SetState(IdolState newState, bool notifySystems)
    {
        StopTransition();
        _transitionRoutine = StartCoroutine(TransitionRoutine(newState, notifySystems));
    }

    private IEnumerator TransitionRoutine(IdolState newState, bool notifySystems)
    {
        if (_clips.TryGetValue((_currentState, newState), out string clipName))
        {
            yield return StartCoroutine(PlayClip(_currentState, newState, clipName));
        }
        else
        {
            IdolState from = _currentState;

            // Leg 1: get to Peaceful
            if (from != IdolState.Peaceful)
            {
                if (_clips.TryGetValue((from, IdolState.Peaceful), out string leg1))
                {
                    yield return StartCoroutine(PlayClip(from, IdolState.Peaceful, leg1));
                }
                else
                {
                    // Snap to Peaceful instantly
                    _currentState = IdolState.Peaceful;
                    _animator.Play("Peaceful_Idle", 0, 0f);
                    _animator.SetInteger("FromState", 1);
                    _animator.SetInteger("ToState", 1);
                    yield return null;
                    yield return null;
                }
            }

            // Leg 2: Peaceful to target
            if (_clips.TryGetValue((IdolState.Peaceful, newState), out string leg2))
            {
                yield return StartCoroutine(PlayClip(IdolState.Peaceful, newState, leg2));
            }
            else
            {
                IdolState[] bridges = { IdolState.Anger, IdolState.Joy, IdolState.Shock, IdolState.Sadness };
                bool bridged = false;

                foreach (var bridge in bridges)
                {
                    if (_clips.TryGetValue((IdolState.Peaceful, bridge), out string tooBridge) &&
                        _clips.TryGetValue((bridge, newState), out string bridgeToTarget))
                    {
                        yield return StartCoroutine(PlayClip(IdolState.Peaceful, bridge, tooBridge));
                        yield return StartCoroutine(PlayClip(bridge, newState, bridgeToTarget));
                        bridged = true;
                        break;
                    }
                }

                if (!bridged)
                {
                    _currentState = newState;
                    _animator.SetInteger("FromState", StateToInt(newState));
                    _animator.SetInteger("ToState", StateToInt(newState));
                }
            }
        }

        if (notifySystems)
            GameEvents.TriggerIdolStateChanged(newState);
    }

    private IEnumerator PlayClip(IdolState from, IdolState to, string clipName)
    {
        _animator.SetInteger("FromState", StateToInt(from));
        _animator.SetInteger("ToState", StateToInt(to));

        yield return null;
        yield return new WaitForSeconds(GetClipLength(clipName));

        _currentState = to;
        _animator.SetInteger("FromState", StateToInt(to));
        _animator.SetInteger("ToState", StateToInt(to));
        yield return null;
    }
}