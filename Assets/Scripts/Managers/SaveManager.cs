using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    private List<RunRecord> _records = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadRecords();
    }

    /// Called by GameManager right before TriggerGameLost so the
    /// record is always in the list when LoseScreenUI reads it.
    public void SaveRecord(float elapsedTime)
    {
        int nextAttempt = _records.Count + 1;
        _records.Add(new RunRecord(nextAttempt, elapsedTime));
        WriteToFile();
        GameEvents.TriggerRecordSaved();
    }

    public List<RunRecord> GetAllRecords()
    {
        var sorted = new List<RunRecord>(_records);
        sorted.Sort((a, b) => a.attemptNumber.CompareTo(b.attemptNumber));
        return sorted;
    }

    private void WriteToFile()
    {
        try
        {
            var wrapper = new RecordListWrapper { records = _records };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"SaveManager: saved to {SavePath}");
        }
        catch (Exception e) { Debug.LogWarning($"SaveManager: save failed — {e.Message}"); }
    }

    private void LoadRecords()
    {
        if (!File.Exists(SavePath)) { _records = new List<RunRecord>(); return; }
        try
        {
            string json = File.ReadAllText(SavePath);
            var wrapper = JsonUtility.FromJson<RecordListWrapper>(json);
            _records = wrapper?.records ?? new List<RunRecord>();
            Debug.Log($"SaveManager: loaded {_records.Count} records from {SavePath}");
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