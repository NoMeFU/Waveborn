using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Система прокачки гравця через NPC.
/// </summary>
public class UpgradeSystem : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public Health playerHealth;
    public PlayerWallet playerWallet;
    public GameObject upgradeMenu;

    [Header("Weapon Reference")]
    public WeaponBase currentWeapon; // додано для доступу до зброї

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip upgradeSuccessClip;
    public AudioClip upgradeFailClip;

    [Header("UI Buttons")]
    public Button closeButton;

    [System.Serializable]
    public class Upgrade
    {
        public string upgradeName;
        public UpgradeType upgradeType;
        public Button button;
        public TMP_Text levelText;
        public float upgradeValue = 10f;
        public int maxLevel = 5;
        public int baseCost = 100;
        [HideInInspector] public int currentLevel = 0;
    }

    [Header("Upgrades")]
    public Upgrade[] upgrades;

    private bool menuOpen = false;

    private void Start()
    {
        foreach (var upgrade in upgrades)
        {
            upgrade.button.onClick.AddListener(() => TryUpgrade(upgrade));
            UpdateUpgradeText(upgrade);
        }

        if (closeButton)
            closeButton.onClick.AddListener(CloseMenu);

        if (upgradeMenu)
            upgradeMenu.SetActive(false);
    }

    private void Update()
    {
        if (menuOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseMenu();
    }

    public void OpenMenu()
    {
        if (upgradeMenu)
        {
            upgradeMenu.SetActive(true);
            menuOpen = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var controller = FindObjectOfType<PlayerController>();
            if (controller) controller.enabled = false;

            // оновлюємо посилання на поточну зброю
            currentWeapon = FindObjectOfType<WeaponBase>();
        }
    }

    public void CloseMenu()
    {
        if (upgradeMenu)
        {
            upgradeMenu.SetActive(false);
            menuOpen = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            var controller = FindObjectOfType<PlayerController>();
            if (controller) controller.enabled = true;
        }
    }

    private void TryUpgrade(Upgrade upgrade)
    {
        if (upgrade.currentLevel >= upgrade.maxLevel)
        {
            Debug.LogWarning($"{upgrade.upgradeName} вже на максимальному рівні!");
            PlaySound(upgradeFailClip);
            return;
        }

        int cost = GetUpgradeCost(upgrade);

        if (!playerWallet.SpendCoins(cost))
        {
            Debug.LogWarning($"Недостатньо монет! Треба {cost}, а є {playerWallet.Coins}");
            PlaySound(upgradeFailClip);
            return;
        }

        upgrade.currentLevel++;
        ApplyUpgrade(upgrade);
        UpdateUpgradeText(upgrade);

        Debug.Log($"✅ Прокачано: {upgrade.upgradeName} до рівня {upgrade.currentLevel}");
        PlaySound(upgradeSuccessClip);
    }

    private void ApplyUpgrade(Upgrade upgrade)
    {
        switch (upgrade.upgradeType)
        {
            case UpgradeType.Health:
                playerStats.AddHealth(upgrade.upgradeValue);
                playerHealth.SetMaxHP(playerStats.maxHealth);
                break;

            case UpgradeType.Damage:
                playerStats.AddDamage(upgrade.upgradeValue);
                break;

            case UpgradeType.Speed:
                playerStats.AddSpeed(upgrade.upgradeValue);
                break;

            case UpgradeType.Shield:
                playerStats.AddShieldDuration(upgrade.upgradeValue);
                break;

            case UpgradeType.Regen:
                playerHealth.SetRegenRate(playerHealth.BaseRegen + upgrade.upgradeValue * upgrade.currentLevel);
                break;

            case UpgradeType.FireRate:
                if (currentWeapon != null)
                {
                    currentWeapon.ModifyFireRate(upgrade.upgradeValue);
                    Debug.Log($"🔥 Швидкість стрільби збільшено: {currentWeapon.name}");
                }
                break;
        }
    }

    private int GetUpgradeCost(Upgrade upgrade)
    {
        return upgrade.baseCost * (upgrade.currentLevel + 1);
    }

    private void UpdateUpgradeText(Upgrade upgrade)
    {
        if (upgrade.levelText != null)
        {
            int cost = GetUpgradeCost(upgrade);
            upgrade.levelText.text = $"{upgrade.upgradeName}\nLv. {upgrade.currentLevel}/{upgrade.maxLevel}\n💰 {cost}";
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource && clip)
            audioSource.PlayOneShot(clip);
    }
}

/// <summary>
/// Типи апгрейдів, включно зі швидкістю стрільби.
/// </summary>
public enum UpgradeType
{
    Health,
    Damage,
    Speed,
    Shield,
    Regen,
    FireRate
}
