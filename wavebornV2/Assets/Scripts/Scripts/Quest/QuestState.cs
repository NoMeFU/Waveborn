// Assets/Scripts/Quests/QuestState.cs
public class QuestState
{
    public readonly QuestSO Data;
    public int Current;
    public bool Completed;

    public QuestState(QuestSO data) { Data = data; Current = 0; Completed = false; }

    public int Required => Data.requiredKills;
    public string Id => Data.questId;
}
