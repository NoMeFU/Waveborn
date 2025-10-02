// Assets/Scripts/Quests/UI/QuestLogUI.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestLogUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private float fadeSpeed = 12f;

    [Header("List")]
    [SerializeField] private Transform content;      // ScrollView/Viewport/Content
    [SerializeField] private QuestItemUI itemPrefab; // префаб елемента

    private readonly Dictionary<string, QuestItemUI> items = new();
    private bool visible;

    private void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        SetVisible(false, instant: true);
    }

    private void OnEnable()
    {
        if (QuestManager.Instance)
        {
            QuestManager.Instance.OnQuestAccepted += OnQuestAccepted;
            QuestManager.Instance.OnQuestProgress += OnQuestProgress;
            QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
        }
    }

    private void OnDisable()
    {
        if (QuestManager.Instance)
        {
            QuestManager.Instance.OnQuestAccepted -= OnQuestAccepted;
            QuestManager.Instance.OnQuestProgress -= OnQuestProgress;
            QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            SetVisible(!visible);

        // плавний фейд
        float target = visible ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, fadeSpeed * Time.deltaTime);
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;
    }

    private void SetVisible(bool on, bool instant = false)
    {
        visible = on;
        if (!instant) return;

        canvasGroup.alpha = on ? 1f : 0f;
        canvasGroup.blocksRaycasts = on;
        canvasGroup.interactable = on;
    }

    // ==== QuestManager events ====
    private void OnQuestAccepted(QuestState st) => AddOrUpdateItem(st);
    private void OnQuestProgress(QuestState st) => AddOrUpdateItem(st);
    private void OnQuestCompleted(QuestState st) => AddOrUpdateItem(st);

    private void AddOrUpdateItem(QuestState st)
    {
        if (!st?.Data) return;

        if (!items.TryGetValue(st.Id, out var ui))
        {
            ui = Instantiate(itemPrefab, content);
            ui.Bind(st);
            items[st.Id] = ui;
        }
        else
        {
            ui.Bind(st);
        }
    }
}
