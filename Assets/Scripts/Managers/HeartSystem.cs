using UnityEngine;

public class HeartSystem : MonoBehaviour
{
    public static HeartSystem Instance { get; private set; }

    [SerializeField] private int _maxHearts = 5;

    private int _currentHearts;

    public int CurrentHearts => _currentHearts;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _currentHearts = _maxHearts;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerFell += LoseHeart;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerFell -= LoseHeart;
    }

    public void LoseHeart()
    {
        // ignore extra calls after hearts already hit zero
        if (_currentHearts <= 0) return;

        _currentHearts--;
        GameEvents.TriggerHeartLost(_currentHearts);

        if (_currentHearts <= 0)
            GameEvents.TriggerAllHeartsLost();
    }
}
