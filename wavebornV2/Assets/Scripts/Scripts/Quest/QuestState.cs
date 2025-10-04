[System.Serializable]
public class QuestState
{
    public QuestSO Data;
    public int Current;
    public int Required => Data.TargetAmount;
    public bool Completed => Current >= Required;

    public int CurrentProgress => Current;

    public QuestSO Quest => Data;

    public QuestState(QuestSO data)
    {
        Data = data;
        Current = 0;
    }

    public void AddProgress(int amount)
    {
        if (Completed) return;
        Current += amount;
        if (Current > Required)
            Current = Required;
    }
}
