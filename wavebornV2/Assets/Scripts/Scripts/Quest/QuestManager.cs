using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public event Action<QuestState> OnQuestAccepted;
    public event Action<QuestState> OnQuestProgress;
    public event Action<QuestState> OnQuestCompleted;

    private Dictionary<string, QuestState> activeQuests = new();
    private HashSet<string> completedQuests = new();

    public IReadOnlyDictionary<string, QuestState> ActiveQuests => activeQuests;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Accept(QuestSO quest)
    {
        if (quest == null || activeQuests.ContainsKey(quest.questId))
            return;

        var state = new QuestState(quest);
        activeQuests.Add(quest.questId, state);
        OnQuestAccepted?.Invoke(state);
    }

    public bool HasActive(string questId) => activeQuests.ContainsKey(questId);

    public QuestState GetState(string questId)
    {
        activeQuests.TryGetValue(questId, out var state);
        return state;
    }

    public bool IsCompleted(string questId) => completedQuests.Contains(questId);

    public void AddProgress(string questId, int amount)
    {
        if (!activeQuests.TryGetValue(questId, out var state))
            return;

        state.AddProgress(amount);
        OnQuestProgress?.Invoke(state);

        if (state.Completed)
            CompleteQuest(questId);
    }

    private void CompleteQuest(string questId)
    {
        if (!activeQuests.TryGetValue(questId, out var state))
            return;

        completedQuests.Add(questId);
        activeQuests.Remove(questId);

        OnQuestCompleted?.Invoke(state);

        if (PlayerExperienceInstance != null)
        {
            PlayerExperienceInstance.AddXP(state.Data.XPReward);
            Debug.Log($"🟢 Гравець отримав {state.Data.XPReward} XP за квест {state.Data.Title}");
        }

        Debug.Log($"✅ Квест завершено: {state.Data.title}");
    }

    private PlayerExperience PlayerExperienceInstance
    {
        get
        {
            if (_playerXP == null)
                _playerXP = FindObjectOfType<PlayerExperience>();
            return _playerXP;
        }
    }
    private PlayerExperience _playerXP;


    public List<QuestState> GetAllStates() => activeQuests.Values.ToList();


}
