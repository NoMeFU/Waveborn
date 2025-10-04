using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XPBar : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private Image glowEffect;

    [Header("Animation Settings")]
    [SerializeField] private float fillSpeed = 3f;
    [SerializeField] private AudioClip levelUpSound;

    private PlayerExperience playerXP;
    private AudioSource audioSource;
    private float targetFill;

    private void Start()
    {
        playerXP = FindObjectOfType<PlayerExperience>();
        if (playerXP == null)
        {
            Debug.LogWarning("?? PlayerExperience не знайдено у сцені!");
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        playerXP.OnXPChanged += UpdateXPBar;
        playerXP.OnLevelUp += OnLevelUp;

        UpdateLevelText(playerXP.CurrentLevel);
        UpdateXPBar(playerXP.CurrentXP, playerXP.XPToNextLevel);
    }

    private void OnDestroy()
    {
        if (playerXP != null)
        {
            playerXP.OnXPChanged -= UpdateXPBar;
            playerXP.OnLevelUp -= OnLevelUp;
        }
    }

    private void Update()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * fillSpeed);
        }
    }

    private void UpdateXPBar(int currentXP, int xpToNext)
    {
        if (fillImage == null) return;

        targetFill = Mathf.Clamp01((float)currentXP / xpToNext);
        if (xpText != null)
            xpText.text = $"{currentXP} / {xpToNext}";
    }

    private void OnLevelUp(int newLevel)
    {
        UpdateLevelText(newLevel);
        if (audioSource != null && levelUpSound != null)
            audioSource.PlayOneShot(levelUpSound);
        if (glowEffect != null)
            StartCoroutine(LevelGlow());
    }

    private void UpdateLevelText(int newLevel)
    {
        if (levelText != null)
            levelText.text = $"LEVEL {newLevel}";
    }

    private System.Collections.IEnumerator LevelGlow()
    {
        float duration = 0.7f;
        float timer = 0;
        Color start = new Color(1f, 1f, 1f, 0);
        Color end = new Color(1f, 1f, 1f, 0.8f);

        glowEffect.enabled = true;

        while (timer < duration)
        {
            glowEffect.color = Color.Lerp(start, end, Mathf.PingPong(timer * 3f, 1));
            timer += Time.deltaTime;
            yield return null;
        }

        glowEffect.enabled = false;
    }
}
