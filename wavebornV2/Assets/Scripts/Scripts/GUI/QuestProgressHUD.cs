// Assets/Scripts/Quests/UI/QuestProgressHUD.cs
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestProgressHUD : MonoBehaviour
{
    [SerializeField] private Transform listRoot;  // вертикальний контейнер
    [SerializeField] private TMP_Text linePrefab; // простий TMP_Text (14–16pt)

    private readonly Dictionary<string, TMP_Text> lines = new();

    private void OnEnable()
    {
        if (QuestManager.Instance)
        {
            QuestManager.Instance.OnQuestAccepted += OnQuestChanged;
            QuestManager.Instance.OnQuestProgress += OnQuestChanged;
            QuestManager.Instance.OnQuestCompleted += OnQuestChanged;
        }
    }

    private void OnDisable()
    {
        if (QuestManager.Instance)
        {
            QuestManager.Instance.OnQuestAccepted -= OnQuestChanged;
            QuestManager.Instance.OnQuestProgress -= OnQuestChanged;
            QuestManager.Instance.OnQuestCompleted -= OnQuestChanged;
        }
    }

    private void OnQuestChanged(QuestState st)
    {
        if (!st?.Data) return;

        if (!lines.TryGetValue(st.Id, out var t))
        {
            t = Instantiate(linePrefab, listRoot);
            lines[st.Id] = t;
        }

        if (st.Completed)
        {
            t.text = $"<color=#7CFC7C>✔ {st.Data.title}</color>";
        }
        else
        {
            t.text = $"• {st.Data.title}: <b>{st.Current}/{st.Required}</b>";
        }
    }
}
