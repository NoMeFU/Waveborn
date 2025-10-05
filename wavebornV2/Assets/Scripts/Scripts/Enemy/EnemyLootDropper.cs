using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyLootDropper : MonoBehaviour
{
    [Serializable]
    public class LootItem
    {
        public GameObject itemPrefab;
        [Range(0f, 100f)] public float dropChance = 50f;
        public int minAmount = 1;
        public int maxAmount = 1;
    }

    [Header("Loot Settings")]
    [SerializeField] private List<LootItem> lootTable = new();

    [Header("Coins")]
    [SerializeField] private int minCoins = 5;
    [SerializeField] private int maxCoins = 25;
    [SerializeField] private GameObject coinPrefab;

    [Header("XP Reward")]
    [SerializeField] private int xpReward = 50;

    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        health.OnDied -= HandleDeath;
    }

    private void HandleDeath()
    {
        DropLoot();
        DropCoins();
        GrantXP();
    }

    private void DropLoot()
    {
        foreach (var loot in lootTable)
        {
            float roll = UnityEngine.Random.Range(0f, 100f);
            if (roll <= loot.dropChance && loot.itemPrefab != null)
            {
                int count = UnityEngine.Random.Range(loot.minAmount, loot.maxAmount + 1);
                for (int i = 0; i < count; i++)
                {
                    Vector3 dropPos = transform.position + UnityEngine.Random.insideUnitSphere * 0.5f;
                    dropPos.y = transform.position.y + 0.5f;
                    Instantiate(loot.itemPrefab, dropPos, Quaternion.identity);
                }

                Debug.Log($"🎁 {name} dropped {loot.itemPrefab.name} x{count}");
            }
        }
    }

    private void DropCoins()
    {
        int coins = UnityEngine.Random.Range(minCoins, maxCoins + 1);
        if (coinPrefab != null && coins > 0)
        {
            for (int i = 0; i < coins; i++)
            {
                Vector3 dropPos = transform.position + UnityEngine.Random.insideUnitSphere * 0.3f;
                dropPos.y = transform.position.y + 0.2f;
                Instantiate(coinPrefab, dropPos, Quaternion.identity);
            }
        }

        Debug.Log($"💰 {name} dropped {coins} coins");
    }

    private void GrantXP()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player && player.TryGetComponent<PlayerExperience>(out var xp))
        {
            xp.AddXP(xpReward);
            Debug.Log($"🟩 Player gained {xpReward} XP for killing {name}");
        }
    }
}
