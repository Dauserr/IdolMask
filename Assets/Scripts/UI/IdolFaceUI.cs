using UnityEngine;

public class IdolFaceUI : MonoBehaviour
{
    [Header("Idol Sprite Renderer")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Face Sprites")]
    [SerializeField] private Sprite _peaceful;
    [SerializeField] private Sprite _anger;
    [SerializeField] private Sprite _joy;
    [SerializeField] private Sprite _sadness;
    [SerializeField] private Sprite _fear;
    [SerializeField] private Sprite _shock;

    private void OnEnable()  => GameEvents.OnIdolStateChanged += OnIdolStateChanged;
    private void OnDisable() => GameEvents.OnIdolStateChanged -= OnIdolStateChanged;

    private void Start() => SetFace(_peaceful);

    private void OnIdolStateChanged(IdolState newState)
    {
        switch (newState)
        {
            case IdolState.Peaceful: SetFace(_peaceful); break;
            case IdolState.Anger:    SetFace(_anger);    break;
            case IdolState.Joy:      SetFace(_joy);      break;
            case IdolState.Sadness:  SetFace(_sadness);  break;
            case IdolState.Fear:     SetFace(_fear);     break;
            case IdolState.Shock:    SetFace(_shock);    break;
        }
    }

    private void SetFace(Sprite sprite)
    {
        if (_spriteRenderer == null || sprite == null) return;
        _spriteRenderer.sprite = sprite;
    }
}
