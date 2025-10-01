// Assets/Scripts/Quests/QuestManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public event Action<QuestState> OnQuestAccepted;
    public event Action<QuestState> OnQuestProgress;
    public event Action<QuestState> OnQuestCompleted;

    private readonly Dictionary<string, QuestState> _active = new();
    private readonly HashSet<string> _completed = new();

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // якщо треба між сценами
    }

    public bool HasActive(string questId) => _active.ContainsKey(questId);
    public bool IsCompleted(string questId) => _completed.Contains(questId);
    public QuestState GetState(string questId) => _active.TryGetValue(questId, out var st) ? st : null;

    public bool Accept(QuestSO data)
    {
        if (data == null || string.IsNullOrEmpty(data.questId)) return false;
        if (_active.ContainsKey(data.questId) || _completed.Contains(data.questId)) return false;

        var st = new QuestState(data);
        _active[data.questId] = st;
        OnQuestAccepted?.Invoke(st);
        return true;
    }

    /// Викликати, коли помер ворог із певним enemyId
    public void RegisterEnemyKill(string enemyId)
    {
        if (string.IsNullOrEmpty(enemyId)) return;

        // проходимось по всіх активних квестах, які таргетять цього ворога
        foreach (var kv in _active)
        {
            var st = kv.Value;
            if (st.Completed) continue;
            if (st.Data.targetEnemyId != enemyId) continue;

            st.Current = Mathf.Min(st.Required, st.Current + 1);
            OnQuestProgress?.Invoke(st);

            if (st.Current >= st.Required)
            {
                st.Completed = true;
                _completed.Add(st.Id);
                OnQuestCompleted?.Invoke(st);
            }
        }
    }
}
