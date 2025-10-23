using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class UpgradeItem
{
    public string upgradeName;
    public UpgradeType upgradeType;
    public TMP_Text upgradeText;
    public Button upgradeButton;
    public int level = 0;
    public int baseCost = 100;
    public float costMultiplier = 1.5f;
    public float upgradeValue = 1f;

    [Header("Limit")]
    public int maxLevel = 10; // 👈 нове поле: максимальний рівень
}

public enum UpgradeType
{
    Health,
    Damage,
    Speed,
    Shield,
    FireRate,
    CritChance,
    CritDamage,
    RegenHP // 👈 новий тип
}

public class UpgradeSystem : MonoBehaviour
{
    [Header("Основні компоненти")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private Button closeButton;
    [SerializeField] private List<UpgradeItem> upgrades = new List<UpgradeItem>();

    [Header("Звук")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;
    [SerializeField] private AudioClip upgradeSuccessClip;
    [SerializeField] private AudioClip upgradeFailClip;

    private PlayerStats playerStats;
    private PlayerWallet wallet;

    private bool isOpen = false;
    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (menuPanel)
            menuPanel.SetActive(false);
    }

    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        wallet = FindObjectOfType<PlayerWallet>();

        if (closeButton)
            closeButton.onClick.AddListener(CloseMenu);

        foreach (var upg in upgrades)
        {
            if (upg.upgradeButton)
            {
                UpgradeItem item = upg;
                upg.upgradeButton.onClick.AddListener(() => TryUpgrade(item));
            }
        }

        UpdateUI();
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseMenu();
    }

    public void OpenMenu()
    {
        if (menuPanel) menuPanel.SetActive(true);
        isOpen = true;

        if (audioSource && openClip)
            audioSource.PlayOneShot(openClip);

        Time.timeScale = 0f;
        UpdateUI();
    }

    public void CloseMenu()
    {
        if (menuPanel) menuPanel.SetActive(false);
        isOpen = false;

        if (audioSource && closeClip)
            audioSource.PlayOneShot(closeClip);

        Time.timeScale = 1f;
    }

    private void TryUpgrade(UpgradeItem item)
    {
        if (item.level >= item.maxLevel)
            return;

        int cost = GetCost(item);

        if (!wallet.SpendCoins(cost))
        {
            if (audioSource && upgradeFailClip)
                audioSource.PlayOneShot(upgradeFailClip);
            return;
        }

        item.level++;
        ApplyUpgrade(item);

        if (audioSource && upgradeSuccessClip)
            audioSource.PlayOneShot(upgradeSuccessClip);

        UpdateUI();
    }

    private int GetCost(UpgradeItem item)
    {
        double raw = item.baseCost * System.Math.Pow(item.costMultiplier, item.level);
        double clamped = System.Math.Min(raw, 9999999);
        return Mathf.RoundToInt((float)clamped);
    }

    private void ApplyUpgrade(UpgradeItem item)
    {
        switch (item.upgradeType)
        {
            case UpgradeType.Health:
                playerStats.AddHealth(item.upgradeValue);
                break;
            case UpgradeType.Damage:
                playerStats.AddDamage(item.upgradeValue);
                break;
            case UpgradeType.Speed:
                playerStats.AddSpeed(item.upgradeValue);
                break;
            case UpgradeType.Shield:
                playerStats.AddShieldDuration(item.upgradeValue);
                break;
            case UpgradeType.FireRate:
                playerStats.AddFireRate(item.upgradeValue);
                break;
            case UpgradeType.CritChance:
                playerStats.AddCritChance(item.upgradeValue);
                break;
            case UpgradeType.CritDamage:
                playerStats.AddCritMultiplier(item.upgradeValue);
                break;
            case UpgradeType.RegenHP:
                playerStats.AddRegen(item.upgradeValue);
                break;
        }
    }

    private void UpdateUI()
    {
        if (wallet && coinsText)
            coinsText.text = $"Монети: {FormatCost(wallet.Coins)}";

        foreach (var item in upgrades)
        {
            if (!item.upgradeText) continue;

            if (item.level >= item.maxLevel)
            {
                item.upgradeText.text = $"{item.upgradeName} Lv.{item.level} (MAX)";
                if (item.upgradeButton) item.upgradeButton.interactable = false;
            }
            else
            {
                string costText = FormatCost(GetCost(item));
                item.upgradeText.text = $"{item.upgradeName} Lv.{item.level} ({costText}💰)";
                if (item.upgradeButton) item.upgradeButton.interactable = true;
            }
        }
    }

    private string FormatCost(int cost)
    {
        if (cost >= 1_000_000) return (cost / 1_000_000f).ToString("F1") + "M";
        if (cost >= 1_000) return (cost / 1_000f).ToString("F1") + "K";
        return cost.ToString();
    }
}
