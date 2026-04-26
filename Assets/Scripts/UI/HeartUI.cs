using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    // assign all 5 heart Image objects in order in the Inspector
    [SerializeField] private Image[] _hearts;

    [SerializeField] private Sprite _heartFull;
    [SerializeField] private Sprite _heartEmpty;

    private void Start()
    {
        // show all hearts full at the start
        RefreshHearts(5);
    }

    private void OnEnable()
    {
        GameEvents.OnHeartLost += OnHeartLost;
    }

    private void OnDisable()
    {
        GameEvents.OnHeartLost -= OnHeartLost;
    }

    private void OnHeartLost(int heartsRemaining)
    {
        RefreshHearts(heartsRemaining);
    }

    private void RefreshHearts(int heartsRemaining)
    {
        for (int i = 0; i < _hearts.Length; i++)
        {
            if (_hearts[i] == null) continue;

            // hearts from left to right — filled ones come first
            _hearts[i].sprite = i < heartsRemaining ? _heartFull : _heartEmpty;
        }
    }
}
