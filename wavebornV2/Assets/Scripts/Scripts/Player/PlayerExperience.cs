using System;
using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    [Header("XP Settings")]
    [SerializeField] private int baseXPToLevelUp = 100;
    [SerializeField] private float xpIncreaseFactor = 2.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField] private AudioSource audioSource;

    public int CurrentLevel { get; private set; } = 1;
    public int CurrentXP { get; private set; } = 0;
    public int XPToNextLevel { get; private set; }

    public event Action<int, int> OnXPChanged;
    public event Action<int> OnLevelUp;        

    private void Start()
    {
        XPToNextLevel = baseXPToLevelUp;

       
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        NotifyXPChange();
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        CurrentXP += amount;
        Debug.Log($"🟩 Gained {amount} XP. Total: {CurrentXP}/{XPToNextLevel}");

        while (CurrentXP >= XPToNextLevel)
        {
            LevelUp();
        }

        NotifyXPChange();
    }

    private void LevelUp()
    {
        CurrentXP -= XPToNextLevel;
        CurrentLevel++;
        XPToNextLevel = Mathf.RoundToInt(XPToNextLevel * xpIncreaseFactor);

        Debug.Log($"⭐ LEVEL UP! New Level: {CurrentLevel} | Next XP: {XPToNextLevel}");

        PlayLevelUpSound();

        OnLevelUp?.Invoke(CurrentLevel);
        NotifyXPChange();
    }

    private void PlayLevelUpSound()
    {
        if (levelUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }
        else
        {
            Debug.LogWarning("🎵 Level Up sound is missing or AudioSource not assigned.");
        }
    }

    private void NotifyXPChange()
    {
        OnXPChanged?.Invoke(CurrentXP, XPToNextLevel);
    }
}
