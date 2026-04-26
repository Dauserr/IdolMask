using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BestTimeUI : MonoBehaviour
{
    [SerializeField] private Transform  _contentParent;   // the Content object inside ScrollView
    [SerializeField] private GameObject _recordPrefab;    // a prefab with a TextMeshProUGUI component

    private void Start()
    {
        RefreshList();
    }

    private void OnEnable()
    {
        GameEvents.OnRecordSaved += RefreshList;
    }

    private void OnDisable()
    {
        GameEvents.OnRecordSaved -= RefreshList;
    }

    private void RefreshList()
    {
        if (SaveManager.Instance == null) return;

        // clear old entries before rebuilding the list
        foreach (Transform child in _contentParent)
            Destroy(child.gameObject);

        List<RunRecord> records = SaveManager.Instance.GetAllRecords();

        if (records.Count == 0)
        {
            SpawnEntry("No runs yet — survive as long as you can!");
            return;
        }

        foreach (var record in records)
        {
            string text = $"Attempt #{record.attemptNumber} — survived {record.timeOfDeath:F1}s";
            SpawnEntry(text);
        }
    }

    private void SpawnEntry(string text)
    {
        var entry = Instantiate(_recordPrefab, _contentParent);
        var label = entry.GetComponent<TextMeshProUGUI>();
        if (label != null)
            label.text = text;
    }
}
