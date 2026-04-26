using TMPro;
using UnityEngine;

/// <summary>
/// Shows "Walk to the Mask" hint before game starts.
/// Hides automatically when the game begins.
/// </summary>
public class HintUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _hintText;

    private void OnEnable()
    {
        GameEvents.OnGameStarted += Hide;
        GameEvents.OnGameLost    += ShowAfterDelay;
        GameEvents.OnGameWon     += HideForWin;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted -= Hide;
        GameEvents.OnGameLost    -= ShowAfterDelay;
        GameEvents.OnGameWon     -= HideForWin;
    }

    private void Start()
    {
        Show();
    }

    private void Show()
    {
        if (_hintText != null)
        {
            _hintText.text    = "Walk to the Golden Mask";
            _hintText.enabled = true;
        }
    }

    private void Hide()
    {
        if (_hintText != null)
            _hintText.enabled = false;
    }

    // after a loss, hint re-appears once the lose screen is dismissed (via RestartGame)
    private void ShowAfterDelay() { }

    private void HideForWin() => Hide();

    // called by GameManager.RestartGame so hint reappears for next run
    public void ResetHint() => Show();
}
