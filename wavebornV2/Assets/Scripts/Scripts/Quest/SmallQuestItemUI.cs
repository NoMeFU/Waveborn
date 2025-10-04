using UnityEngine;
using UnityEngine.UI;

public class SmallQuestItemUI : MonoBehaviour
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text progressText;

    private QuestState questState;

    public void Setup(QuestState state)
    {
        questState = state;
        Refresh();
    }

    public void Refresh()
    {
        if (questState == null) return;
        titleText.text = questState.Data.Title;
        progressText.text = $"{questState.Current} / {questState.Required}";
    }
}
