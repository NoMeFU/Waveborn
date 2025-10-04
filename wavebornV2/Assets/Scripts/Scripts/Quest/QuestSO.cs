using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Quest")]
public class QuestSO : ScriptableObject
{
    [Header("Quest Info")]
    [SerializeField] private string _questId;
    [SerializeField] private string _title;
    [SerializeField] private string _description;
    [SerializeField] private int _targetAmount;
    [SerializeField] private bool _repeatable;
    [SerializeField] private string _rewardText;

    [Header("XP Reward")]
    [SerializeField] private int _xpReward = 50;

    public string questId => _questId;
    public string ID => _questId;
    public string title => _title;
    public string Title => _title;
    public string description => _description;
    public string Description => _description;
    public int requiredAmount => _targetAmount;
    public int RequiredAmount => _targetAmount;
    public int TargetAmount => _targetAmount;
    public bool repeatable => _repeatable;
    public string rewardText => _rewardText;
    public string Reward => _rewardText;
    public int XPReward => _xpReward;
}
