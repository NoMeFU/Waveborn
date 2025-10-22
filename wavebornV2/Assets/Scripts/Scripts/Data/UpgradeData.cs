using UnityEngine;

[System.Serializable]
public class UpgradeData
{
    [Header("Назва прокачки (для UI)")]
    public string name;

    [Header("Вартість кожного рівня")]
    public int[] costs;

    [Header("Значення, яке додається за рівень (HP, DMG, тощо)")]
    public float[] values;

    public int MaxLevel => Mathf.Min(costs.Length, values.Length);
}
