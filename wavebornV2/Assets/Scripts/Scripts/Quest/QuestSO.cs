// Assets/Scripts/Quests/QuestSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quest")]
public class QuestSO : ScriptableObject
{
    [Header("ID & Text")]
    public string questId = "kill_zombies_5";
    public string title = "Очищення периметра";
    [TextArea] public string description = "Знищ 5 звичайних зомбі поруч із базою.";

    [Header("Objective")]
    public string targetEnemyId = "zombie_basic";
    public int requiredKills = 5;

    [Header("Reward (text)")]
    public string rewardText = "+500 XP, +50 кредитів";
}
