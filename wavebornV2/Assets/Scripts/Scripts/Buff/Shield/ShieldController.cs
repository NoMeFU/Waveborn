using System;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] private float shieldDuration = 15f;
    [SerializeField] private GameObject shieldVisualPrefab;
    [SerializeField] private Transform shieldParent;

    [Header("Visual Effects")]
    [SerializeField] private Color shieldColor = new Color(0.3f, 0.8f, 1f, 0.5f);
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.1f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activateClip;
    [SerializeField] private AudioClip deactivateClip;
    [SerializeField] private AudioClip blockClip;

    private GameObject shieldVisual;
    private Material shieldMaterial;
    private float shieldTimer;
    private bool isActive;
    private Vector3 originalScale;

    public bool IsActive => isActive;
    public float RemainingTime => shieldTimer;
    public float RemainingPercent => isActive ? shieldTimer / shieldDuration : 0f;

    public event Action ShieldActivated;
    public event Action ShieldDeactivated;
    public event Action<float> ShieldDamageBlocked;

    private void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!shieldParent) shieldParent = transform;
    }

    private void Update()
    {
        if (isActive)
        {
            shieldTimer -= Time.deltaTime;

            if (shieldTimer <= 0f)
            {
                DeactivateShield();
            }
            else
            {
                UpdateShieldVisual();
            }
        }
    }

    /// <summary>
    /// Активувати щит на вказаний час
    /// </summary>
    public void ActivateShield(float duration = 0f)
    {
        if (duration > 0f)
            shieldDuration = duration;

        if (isActive)
        {
            // Продовжити час щита
            shieldTimer = Mathf.Max(shieldTimer, shieldDuration);
            Debug.Log($"<color=cyan>🛡️ Щит продовжено! Залишилось: {shieldTimer:F1}с</color>");
            return;
        }

        isActive = true;
        shieldTimer = shieldDuration;

        CreateShieldVisual();
        PlaySound(activateClip);
        ShieldActivated?.Invoke();

        Debug.Log($"<color=green>✅ Щит активовано на {shieldDuration}с!</color>");
    }

    /// <summary>
    /// Деактивувати щит
    /// </summary>
    public void DeactivateShield()
    {
        if (!isActive) return;

        isActive = false;
        shieldTimer = 0f;

        DestroyShieldVisual();
        PlaySound(deactivateClip);
        ShieldDeactivated?.Invoke();

        Debug.Log("<color=yellow>⚠️ Щит деактивовано!</color>");
    }

    /// <summary>
    /// Перевірити чи щит блокує урон
    /// </summary>
    public bool TryBlockDamage(float damageAmount)
    {
        if (!isActive) return false;

        PlaySound(blockClip);
        ShieldDamageBlocked?.Invoke(damageAmount);

        Debug.Log($"<color=cyan>🛡️ Щит заблокував {damageAmount} урону!</color>");

        // Можна додати ефект при блокуванні
        if (shieldVisual)
        {
            StartCoroutine(FlashShield());
        }

        return true;
    }

    private void CreateShieldVisual()
    {
        if (shieldVisual) return;

        if (shieldVisualPrefab)
        {
            shieldVisual = Instantiate(shieldVisualPrefab, shieldParent);
            shieldVisual.transform.localPosition = Vector3.zero;
            shieldVisual.transform.localRotation = Quaternion.identity;
        }
        else
        {
            // Створити стандартну сферу якщо немає префабу
            shieldVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shieldVisual.transform.SetParent(shieldParent);
            shieldVisual.transform.localPosition = Vector3.zero;
            shieldVisual.transform.localScale = Vector3.one * 2.5f;

            // Видалити колайдер
            Destroy(shieldVisual.GetComponent<Collider>());

            // Створити прозорий матеріал
            Renderer renderer = shieldVisual.GetComponent<Renderer>();
            shieldMaterial = new Material(Shader.Find("Standard"));
            shieldMaterial.SetFloat("_Mode", 3); // Transparent mode
            shieldMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            shieldMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            shieldMaterial.SetInt("_ZWrite", 0);
            shieldMaterial.DisableKeyword("_ALPHATEST_ON");
            shieldMaterial.EnableKeyword("_ALPHABLEND_ON");
            shieldMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            shieldMaterial.renderQueue = 3000;
            shieldMaterial.color = shieldColor;
            shieldMaterial.EnableKeyword("_EMISSION");
            shieldMaterial.SetColor("_EmissionColor", shieldColor * 0.5f);

            renderer.material = shieldMaterial;
        }

        originalScale = shieldVisual.transform.localScale;
    }

    private void UpdateShieldVisual()
    {
        if (!shieldVisual) return;

        // Пульсуючий ефект
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        shieldVisual.transform.localScale = originalScale * pulse;

        // Зменшення прозорості при закінченні
        if (shieldTimer < 3f && shieldMaterial)
        {
            float alpha = Mathf.Lerp(0.1f, shieldColor.a, shieldTimer / 3f);
            Color color = shieldColor;
            color.a = alpha;
            shieldMaterial.color = color;
        }

        // Обертання
        shieldVisual.transform.Rotate(Vector3.up, 20f * Time.deltaTime);
    }

    private void DestroyShieldVisual()
    {
        if (shieldVisual)
        {
            Destroy(shieldVisual);
            shieldVisual = null;
        }

        if (shieldMaterial)
        {
            Destroy(shieldMaterial);
            shieldMaterial = null;
        }
    }

    private System.Collections.IEnumerator FlashShield()
    {
        if (!shieldMaterial) yield break;

        Color originalColor = shieldMaterial.color;
        shieldMaterial.color = Color.white;

        yield return new WaitForSeconds(0.1f);

        if (shieldMaterial)
            shieldMaterial.color = originalColor;
    }

    private void PlaySound(AudioClip clip)
    {
        if (!clip) return;

        if (audioSource)
            audioSource.PlayOneShot(clip);
        else
            AudioSource.PlayClipAtPoint(clip, transform.position);
    }

    private void OnDestroy()
    {
        DestroyShieldVisual();
    }

    // Для тестування в Editor
    [ContextMenu("Activate Shield (Test)")]
    private void TestActivate()
    {
        ActivateShield();
    }

    [ContextMenu("Deactivate Shield (Test)")]
    private void TestDeactivate()
    {
        DeactivateShield();
    }
}