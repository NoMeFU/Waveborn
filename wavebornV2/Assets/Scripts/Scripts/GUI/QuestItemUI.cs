// Assets/Scripts/Quests/UI/QuestItemUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestItemUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image background;

    [Header("Colors")]
    [SerializeField] private Color normalBg = new(0.12f, 0.15f, 0.18f, 0.6f);
    [SerializeField] private Color completedBg = new(0.10f, 0.20f, 0.10f, 0.65f);

    private QuestState state;

    public void Bind(QuestState st)
    {
        state = st;
        titleText.text = st.Data.title;
        descriptionText.text = st.Data.description;

        UpdateView();
    }

    public void UpdateView()
    {
        if (state == null) return;

        float pct = state.Required > 0 ? (float)state.Current / state.Required : 1f;
        if (progressFill)
        {
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillAmount = Mathf.Clamp01(pct);
        }
        if (progressText)
            progressText.text = $"{state.Current}/{state.Required}";

        if (background)
            background.color = state.Completed ? completedBg : normalBg;

        // Візуал завершення: трошки приглушуємо опис
        if (state.Completed) descriptionText.alpha = 0.75f;
        else descriptionText.alpha = 1f;
    }

    public string QuestId => state?.Id;
}
