// Assets/Scripts/Quests/QuestSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quest")]
public class QuestSO : ScriptableObject
{
    [Header("ID & Text")]
    public string questId = "kill_zombies_5";
    public string title = "�������� ���������";
    [TextArea] public string description = "���� 5 ��������� ���� ����� �� �����.";

    [Header("Objective")]
    public string targetEnemyId = "zombie_basic";
    public int requiredKills = 5;

    [Header("Reward (text)")]
    public string rewardText = "+500 XP, +50 �������";
}
