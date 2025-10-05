using UnityEngine;
using System;

[DisallowMultipleComponent]
public class PlayerWallet : MonoBehaviour
{
    [Header("Wallet Settings")]
    [SerializeField, Min(0)] private int startCoins = 0;
    [SerializeField] private bool saveProgress = true;

    public int Coins { get; private set; }

    public event Action<int> OnCoinsChanged;

    private const string SaveKey = "PlayerCoins";

    private void Awake()
    {
        // Якщо збереження включено — підтягуємо монети з PlayerPrefs
        if (saveProgress && PlayerPrefs.HasKey(SaveKey))
            Coins = PlayerPrefs.GetInt(SaveKey, startCoins);
        else
            Coins = startCoins;

        OnCoinsChanged?.Invoke(Coins);
    }

    /// <summary>
    /// Додати монети до гаманця
    /// </summary>
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        // Захист від переповнення
        if (Coins + amount < Coins)
        {
            Coins = int.MaxValue;
        }
        else
        {
            Coins += amount;
        }

        OnCoinsChanged?.Invoke(Coins);
        SaveWallet();

        Debug.Log($"🟢 +{amount} монет (Всього: {Coins})");
    }

    /// <summary>
    /// Витратити монети, якщо вистачає
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || Coins < amount)
        {
            Debug.LogWarning("⚠️ Недостатньо монет або неправильна сума!");
            return false;
        }

        Coins -= amount;
        OnCoinsChanged?.Invoke(Coins);
        SaveWallet();

        Debug.Log($"🔴 -{amount} монет (Всього: {Coins})");
        return true;
    }

    /// <summary>
    /// Обнулити гаманець (наприклад, при рестарті гри)
    /// </summary>
    public void ResetWallet()
    {
        Coins = startCoins;
        OnCoinsChanged?.Invoke(Coins);
        SaveWallet();

        Debug.Log($"♻️ Баланс скинуто. Монети: {Coins}");
    }

    private void SaveWallet()
    {
        if (!saveProgress) return;
        PlayerPrefs.SetInt(SaveKey, Coins);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        SaveWallet();
    }
}
