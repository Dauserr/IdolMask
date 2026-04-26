using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // key used to store data in PlayerPrefs
    private const string SaveKey = "IdolMask_Records";

    private List<RunRecord> _records = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        LoadRecords();
    }

    private void OnEnable()
    {
        GameEvents.OnGameLost += OnGameLost;
    }

    private void OnDisable()
    {
        GameEvents.OnGameLost -= OnGameLost;
    }

    // called automatically when the player loses
    private void OnGameLost()
    {
        float survivalTime = TimerController.Instance != null
            ? TimerController.Instance.TimeRemaining
            : 0f;

        SaveRecord(survivalTime);
    }

    public void SaveRecord(float timeOfDeath)
    {
        int nextAttempt = _records.Count + 1;
        _records.Add(new RunRecord(nextAttempt, timeOfDeath));

        WriteToPlayerPrefs();
        GameEvents.TriggerRecordSaved();
    }

    // returns all records sorted oldest → newest
    public List<RunRecord> GetAllRecords()
    {
        var sorted = new List<RunRecord>(_records);
        sorted.Sort((a, b) => a.attemptNumber.CompareTo(b.attemptNumber));
        return sorted;
    }

    private void WriteToPlayerPrefs()
    {
        try
        {
            // wrap the list in a helper because JsonUtility can't serialize a raw List
            var wrapper = new RecordListWrapper { records = _records };
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveManager: failed to save records — {e.Message}");
        }
    }

    private void LoadRecords()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            _records = new List<RunRecord>();
            return;
        }

        try
        {
            string json    = PlayerPrefs.GetString(SaveKey);
            var    wrapper = JsonUtility.FromJson<RecordListWrapper>(json);
            _records = wrapper?.records ?? new List<RunRecord>();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveManager: failed to load records — {e.Message}");
            _records = new List<RunRecord>();
        }
    }

    // JsonUtility requires a wrapper class to serialize a List
    [Serializable]
    private class RecordListWrapper
    {
        public List<RunRecord> records = new();
    }
}
