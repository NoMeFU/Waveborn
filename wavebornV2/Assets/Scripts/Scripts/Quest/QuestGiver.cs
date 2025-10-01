using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestGiver : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private Button btnTasks;
    [SerializeField] private TextMeshProUGUI tasksStatusText; // �������

    [Header("Quests Offered")]
    [SerializeField] private List<QuestSO> quests = new();

    private void Awake()
    {
        if (!dialogueUI) dialogueUI = FindObjectOfType<DialogueUI>();
        if (btnTasks) btnTasks.onClick.AddListener(OnTasksPressed);
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

    public void OnTasksPressed()
    {
        if (!dialogueUI) return;

        // ��� �������� � ������ ������ ����� � ������
        var q = quests.Count > 0 ? quests[0] : null;
        if (q == null) { dialogueUI.ShowDialogueText("����� ����� ����."); StartCoroutine(CloseSoon()); return; }

        var qm = QuestManager.Instance;
        if (!qm.HasActive(q.questId) && !qm.IsCompleted(q.questId))
        {
            qm.Accept(q);
            dialogueUI.ShowDialogueText($"��������: <b>{q.title}</b>\n{q.description}");
            StartCoroutine(CloseSoon());
        }
        else
        {
            var st = qm.GetState(q.questId);
            if (st != null && !st.Completed)
            {
                dialogueUI.ShowDialogueText($"{q.title}\n�������: {st.Current}/{st.Required}");
            }
            else
            {
                dialogueUI.ShowDialogueText($"����� ���������: <b>{q.title}</b>\n��������: {q.rewardText}");
            }
            StartCoroutine(CloseSoon());
        }
    }

    private System.Collections.IEnumerator CloseSoon()
    {
        yield return new WaitForSeconds(1.0f);
        if (dialogueUI && dialogueUI.IsOpen) dialogueUI.CloseAll();
    }

    private void OnQuestAccepted(QuestState st)
    {
        if (tasksStatusText) tasksStatusText.text = $"��������: {st.Data.title}";
    }

    private void OnQuestProgress(QuestState st)
    {
        if (tasksStatusText && !st.Completed)
            tasksStatusText.text = $"{st.Data.title}: {st.Current}/{st.Required}";
    }

    private void OnQuestCompleted(QuestState st)
    {
        if (tasksStatusText)
            tasksStatusText.text = $"���������: {st.Data.title}\n{st.Data.rewardText}";
    }
}