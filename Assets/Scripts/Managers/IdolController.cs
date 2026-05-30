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
    private bool _destroyed = false;
    private SpriteRenderer _spriteRenderer;

    public IdolState CurrentState => _currentState;

    private static readonly Dictionary<(IdolState, IdolState), string> _clips = new()
    {
        // From Peaceful
        { (IdolState.Peaceful, IdolState.Anger),   "Peaceful_ToAnger" },
        { (IdolState.Peaceful, IdolState.Joy),     "Peaceful_ToJoy" },
        { (IdolState.Peaceful, IdolState.Sadness), "Peaceful_ToSadness" },
        { (IdolState.Peaceful, IdolState.Shock),   "Peaceful_ToShock" },
        { (IdolState.Peaceful, IdolState.Fear),    "Peaceful_ToFear" },
        { (IdolState.Peaceful, IdolState.Boulder), "Peaceful_ToBoulder" },

        // From Anger
        { (IdolState.Anger, IdolState.Peaceful),   "Anger_ToPeaceful" },
        { (IdolState.Anger, IdolState.Sadness),    "Anger_ToSadness" },
        { (IdolState.Anger, IdolState.Fear),       "Anger_ToFear" },
        { (IdolState.Anger, IdolState.Joy),        "Anger_ToJoy" },
        { (IdolState.Anger, IdolState.Shock),      "Anger_ToShock" },
        { (IdolState.Anger, IdolState.Boulder), "Anger_ToBoulder"    },

        // From Fear
        { (IdolState.Fear, IdolState.Peaceful),    "Fear_ToPeaceful" },
        { (IdolState.Fear, IdolState.Anger),       "Fear_ToAnger" },
        { (IdolState.Fear, IdolState.Joy),         "Fear_ToJoy" },
        { (IdolState.Fear, IdolState.Sadness),     "Fear_ToSadness" },
        { (IdolState.Fear, IdolState.Shock),       "Fear_ToShock" },
        { (IdolState.Fear,     IdolState.Boulder), "Fear_ToBoulder"     },

        // From Joy
        { (IdolState.Joy, IdolState.Peaceful),     "Joy_ToPeaceful" },
        { (IdolState.Joy, IdolState.Anger),        "Joy_ToAnger" },
        { (IdolState.Joy, IdolState.Fear),         "Joy_ToFear" },
        { (IdolState.Joy, IdolState.Sadness),      "Joy_ToSadness" },
        { (IdolState.Joy, IdolState.Shock),        "Joy_ToShock" },
        { (IdolState.Joy, IdolState.Boulder), "Joy_ToBoulder"      },

        // From Sadness
        { (IdolState.Sadness, IdolState.Peaceful), "Sadness_ToPeaceful" },
        { (IdolState.Sadness, IdolState.Anger),    "Sadness_ToAnger" },
        { (IdolState.Sadness, IdolState.Fear),     "Sadness_ToFear" },
        { (IdolState.Sadness, IdolState.Joy),      "Sadness_ToJoy" },
        { (IdolState.Sadness, IdolState.Shock),    "Sadness_ToShock" },
        { (IdolState.Sadness, IdolState.Boulder), "Sadness_ToBoulder"  },

        // From Shock
        { (IdolState.Shock, IdolState.Peaceful),   "Shock_ToPeaceful" },
        { (IdolState.Shock, IdolState.Anger),      "Shock_ToAnger" },
        { (IdolState.Shock, IdolState.Fear),       "Shock_ToFear" },
        { (IdolState.Shock, IdolState.Joy),        "Shock_ToJoy" },
        { (IdolState.Shock, IdolState.Sadness),    "Shock_ToSadness" },
        { (IdolState.Shock, IdolState.Boulder), "Shock_ToBoulder"    },

        // From Boulder
        { (IdolState.Boulder, IdolState.Peaceful), "Boulder_ToPeaceful" },
        { (IdolState.Boulder, IdolState.Anger),    "Boulder_ToAnger"    },
        { (IdolState.Boulder, IdolState.Fear),     "Boulder_ToFear"     },
        { (IdolState.Boulder, IdolState.Joy),      "Boulder_ToJoy"      },
        { (IdolState.Boulder, IdolState.Sadness),  "Boulder_ToSadness"  },
        { (IdolState.Boulder, IdolState.Shock),    "Boulder_ToShock"    },
    };

    private static readonly Dictionary<IdolState, Color> _stateColors = new()
    {
        { IdolState.Peaceful, new Color(0.9f, 0.9f, 0.8f) },  // warm stone white
        { IdolState.Anger,    new Color(1.0f, 0.2f, 0.1f) },  // red
        { IdolState.Joy,      new Color(1.0f, 0.9f, 0.1f) },  // golden yellow
        { IdolState.Sadness,  new Color(0.3f, 0.5f, 1.0f) },  // blue
        { IdolState.Fear,     new Color(0.5f, 0.2f, 0.7f) },  // purple
        { IdolState.Shock,    new Color(0.2f, 1.0f, 0.8f) },  // cyan
        { IdolState.Boulder,  new Color(0.5f, 0.4f, 0.3f) },  // stone brown
    };

    private static int StateToInt(IdolState state) => state switch
    {
        IdolState.Peaceful => 1,
        IdolState.Anger    => 2,
        IdolState.Fear     => 3,
        IdolState.Joy      => 4,
        IdolState.Sadness  => 5,
        IdolState.Shock    => 6,
        IdolState.Boulder  => 7,
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
        _spriteRenderer = GetComponent<SpriteRenderer>();
        GameEvents.OnPlayerFirstMoved += OpenEyes;
        GameEvents.OnGameStarted      += StartStateLoop;
        GameEvents.OnGameWon          += StopAndPeaceful;
        GameEvents.OnGameLost         += StopAndPeaceful;
        GameEvents.OnGameRestarted    += ResetToClosedEyes;
        GameEvents.OnIdolDestroyed    += OnDestroyed;
    }

    private void OnDestroy()
    {
        GameEvents.OnPlayerFirstMoved -= OpenEyes;
        GameEvents.OnGameStarted      -= StartStateLoop;
        GameEvents.OnGameWon          -= StopAndPeaceful;
        GameEvents.OnGameLost         -= StopAndPeaceful;
        GameEvents.OnGameRestarted    -= ResetToClosedEyes;
        GameEvents.OnIdolDestroyed    -= OnDestroyed;
    }
    
    private void Start()
    {
        _animator.SetInteger("FromState", 0);
        _animator.SetInteger("ToState", 0);
    }


    private void OnDestroyed()
    {
        if (_destroyed) return;
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
        yield return new WaitForSeconds(GetClipLength("Peaceful_ToClosedEyes"));

        // Step 2: ClosedEyes → Destroy animation
        _animator.SetInteger("FromState", 0);
        _animator.SetInteger("ToState", 99);
        yield return null;
        yield return new WaitForSeconds(GetClipLength("ClosedEyes_Destroy"));

        GameEvents.TriggerShowWinScreen();
        gameObject.SetActive(false);
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
        _destroyed = false;
        StopStateLoop();
        StopTransition();
        gameObject.SetActive(true);
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

    private IdolState GetRandomTrapState()
    {
        IdolState[] trapStates = {
            IdolState.Anger,
            IdolState.Fear,
            IdolState.Joy,
            IdolState.Sadness,
            IdolState.Shock,
            IdolState.Boulder
        };
        return trapStates[Random.Range(0, trapStates.Length)];
    }

    private void StopAndPeaceful()
    {
        StopStateLoop();
        gameObject.SetActive(true);
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
            Debug.LogWarning($"[Idol] No clip for {_currentState} → {newState}, snapping.");
            _currentState = newState;
            _animator.SetInteger("FromState", StateToInt(newState));
            _animator.SetInteger("ToState", StateToInt(newState));
            yield return null;
        }

        if (notifySystems)
            GameEvents.TriggerIdolStateChanged(newState);
    }

    private IEnumerator PlayClip(IdolState from, IdolState to, string clipName)
    {
        _animator.SetInteger("FromState", StateToInt(from));
        _animator.SetInteger("ToState",   StateToInt(to));
    
        yield return null;
    
        float clipLength = GetClipLength(clipName);
        float elapsed    = 0f;
    
        Color fromColor = _stateColors.TryGetValue(from, out var fc) ? fc : Color.white;
        Color toColor   = _stateColors.TryGetValue(to,   out var tc) ? tc : Color.white;
    
        while (elapsed < clipLength)
        {
            float t = elapsed / clipLength;
    
            Color midColor = Color.Lerp(fromColor, Color.grey, Mathf.Sin(t * Mathf.PI));
            Color tint     = Color.Lerp(midColor, toColor, t);
    
            if (_spriteRenderer != null)
                _spriteRenderer.color = tint;
    
            elapsed += Time.deltaTime;
            yield return null;
        }
    
        // Snap to final color
        if (_spriteRenderer != null)
            _spriteRenderer.color = toColor;
    
        _currentState = to;
        _animator.SetInteger("FromState", StateToInt(to));
        _animator.SetInteger("ToState",   StateToInt(to));
        yield return null;
    }
}