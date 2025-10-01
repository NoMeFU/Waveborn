using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveAsset", menuName = "Game/Wave Asset")]
public class WaveAsset : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField, Min(1)] private int count = 5;

        public GameObject Prefab => enemyPrefab;
        public int Count => Mathf.Max(1, count);
    }

    [Header("Manual Wave")]
    [SerializeField] private List<Entry> entries = new();

    public IReadOnlyList<Entry> Entries => entries;
}
