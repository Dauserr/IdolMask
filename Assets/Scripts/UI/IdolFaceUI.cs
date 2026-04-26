using UnityEngine;

public class IdolFaceUI : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private static readonly int AnimPeaceful = Animator.StringToHash("Peaceful");
    private static readonly int AnimAnger    = Animator.StringToHash("Anger");
    private static readonly int AnimJoy      = Animator.StringToHash("Joy");
    private static readonly int AnimSadness  = Animator.StringToHash("Sadness");
    private static readonly int AnimFear     = Animator.StringToHash("Fear");
    private static readonly int AnimShock    = Animator.StringToHash("Shock");

    private void OnEnable()
    {
        GameEvents.OnIdolStateChanged += OnIdolStateChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnIdolStateChanged -= OnIdolStateChanged;
    }

    private void OnIdolStateChanged(IdolState newState)
    {
        if (_animator == null) return;

        switch (newState)
        {
            case IdolState.Peaceful: _animator.SetTrigger(AnimPeaceful); break;
            case IdolState.Anger:    _animator.SetTrigger(AnimAnger);    break;
            case IdolState.Joy:      _animator.SetTrigger(AnimJoy);      break;
            case IdolState.Sadness:  _animator.SetTrigger(AnimSadness);  break;
            case IdolState.Fear:     _animator.SetTrigger(AnimFear);     break;
            case IdolState.Shock:    _animator.SetTrigger(AnimShock);    break;
        }
    }
}
