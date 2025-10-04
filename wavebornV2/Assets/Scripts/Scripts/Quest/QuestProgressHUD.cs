using System.Collections.Generic;
using UnityEngine;

public class QuestProgressHUD : MonoBehaviour
{
    [SerializeField] private Transform questListParent;
    [SerializeField] private GameObject questItemPrefab;

    private bool subscribed;

    private void OnEnable()
    {
        TrySubscribe();
        RefreshUI();
    }

    private void Start()
    {
        TrySubscribe();
        RefreshUI();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void TrySubscribe()
    {
        if (subscribed) return;
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.OnQuestAccepted += OnAnyQuestEvent;
        QuestManager.Instance.OnQuestProgress += OnAnyQuestEvent;
        QuestManager.Instance.OnQuestCompleted += OnAnyQuestEvent;
        subscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!subscribed) return;
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAccepted -= OnAnyQuestEvent;
            QuestManager.Instance.OnQuestProgress -= OnAnyQuestEvent;
            QuestManager.Instance.OnQuestCompleted -= OnAnyQuestEvent;
        }
        subscribed = false;
    }

    private void OnAnyQuestEvent(QuestState _) => RefreshUI();

    private void RefreshUI()
    {
        if (questListParent == null || questItemPrefab == null)
        {
            Debug.LogWarning("QuestProgressHUD: assign Quest List Parent and Quest Item Prefab in inspector.", this);
            return;
        }

        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("QuestProgressHUD: QuestManager.Instance is null. Add and enable a QuestManager in the scene.", this);
            return;
        }

        for (int i = questListParent.childCount - 1; i >= 0; i--)
        {
            Destroy(questListParent.GetChild(i).gameObject);
        }

        var states = QuestManager.Instance.GetAllStates();
        if (states == null) return;

        foreach (var state in states)
        {
            var go = Instantiate(questItemPrefab, questListParent);
            var ui = go.GetComponent<QuestUI>() ?? go.GetComponentInChildren<QuestUI>();
            if (ui != null) ui.Setup(state);
            else Debug.LogWarning("QuestProgressHUD: Quest Item Prefab has no QuestUI component.", go);
        }
    }
}
