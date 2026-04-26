using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SaveKey = "IdolMask_Records";

    private List<RunRecord> _records = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        LoadRecords();
    }

    /// <summary>Called by GameManager right before TriggerGameLost so the
    /// record is always in the list when LoseScreenUI reads it.</summary>
    public void SaveRecord(float elapsedTime)
    {
        int nextAttempt = _records.Count + 1;
        _records.Add(new RunRecord(nextAttempt, elapsedTime));
        WriteToPlayerPrefs();
        GameEvents.TriggerRecordSaved();
    }

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
            var wrapper = new RecordListWrapper { records = _records };
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(wrapper));
            PlayerPrefs.Save();
        }
        catch (Exception e) { Debug.LogWarning($"SaveManager: save failed — {e.Message}"); }
    }

    private void LoadRecords()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) { _records = new List<RunRecord>(); return; }
        try
        {
            var wrapper = JsonUtility.FromJson<RecordListWrapper>(PlayerPrefs.GetString(SaveKey));
            _records = wrapper?.records ?? new List<RunRecord>();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SaveManager: load failed — {e.Message}");
            _records = new List<RunRecord>();
        }
    }

    [Serializable]
    private class RecordListWrapper { public List<RunRecord> records = new(); }
}
