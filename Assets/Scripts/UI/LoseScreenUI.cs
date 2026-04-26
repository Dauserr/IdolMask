using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoseScreenUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _survivedText;
    [SerializeField] private TextMeshProUGUI _bestText;

    [Header("Records")]
    [SerializeField] private RectTransform _recordsContent;
    [SerializeField] private GameObject    _recordPrefab;

    [Header("Button")]
    [SerializeField] private Button _tryAgainButton;

    private void Awake()
    {
        _canvasGroup.alpha          = 0f;
        _canvasGroup.interactable   = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()  => GameEvents.OnGameLost += Show;
    private void OnDisable() => GameEvents.OnGameLost -= Show;

    private void Start()
    {
        _tryAgainButton.onClick.AddListener(OnTryAgain);
    }

    private void Show()
    {
        PopulateRecords();
        StartCoroutine(FadeIn());
    }

    private void PopulateRecords()
    {
        // clear old entries
        foreach (Transform child in _recordsContent)
            Destroy(child.gameObject);

        if (SaveManager.Instance == null) return;

        var records = SaveManager.Instance.GetAllRecords();

        float survivedTime = records.Count > 0
            ? records[records.Count - 1].timeOfDeath : 0f;
        _survivedText.text = $"Survived  {survivedTime:F1}s";

        float best = 0f;
        foreach (var r in records)
            if (r.timeOfDeath > best) best = r.timeOfDeath;
        _bestText.text = $"Best  {best:F1}s";

        // newest run at the top
        for (int i = records.Count - 1; i >= 0; i--)
        {
            var entry = Instantiate(_recordPrefab, _recordsContent);
            var label = entry.GetComponent<TextMeshProUGUI>();
            if (label != null)
                label.text = $"#{records[i].attemptNumber}   {records[i].timeOfDeath:F1}s";
        }

        // force the Content Size Fitter to recalculate height
        // so all entries are visible and the scroll works correctly
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_recordsContent);
    }

    private IEnumerator FadeIn()
    {
        _canvasGroup.interactable   = true;
        _canvasGroup.blocksRaycasts = true;

        float elapsed  = 0f;
        float duration = 0.6f;

        while (elapsed < duration)
        {
            _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _canvasGroup.alpha = 1f;
    }

    private void Hide()
    {
        _canvasGroup.alpha          = 0f;
        _canvasGroup.interactable   = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void OnTryAgain()
    {
        Hide();
        GameManager.Instance.RestartGame();
    }
}
