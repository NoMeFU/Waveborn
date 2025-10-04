using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private TMP_Text questText;
    private QuestState questState;

    public void Setup(QuestState state)
    {
        questState = state;
        Refresh();
    }

    private void Refresh()
    {
        if (questState == null || questState.Data == null)
            return;

        questText.text =
            $"{questState.Data.Title}\n<size=80%><color=#CCCCCC>{questState.Data.Description}</color></size>\n" +
            $"<b>Progress:</b> {questState.CurrentProgress}/{questState.Data.TargetAmount}";
    }
}
