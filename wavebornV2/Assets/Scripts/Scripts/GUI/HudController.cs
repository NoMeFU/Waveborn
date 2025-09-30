using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HudController : MonoBehaviour
{
    [Header("Refs (можеш кинути руками; якщо порожні — знайду по тегу Player)")]
    [SerializeField] private Health playerHealth;          // Має: CurrentHP, MaxHP, OnChanged(cur,max)
    [SerializeField] private WeaponSwitcher weaponSwitcher; // Має: Current, подію WeaponChanged(WeaponBase)

    [Header("HP UI")]
    [SerializeField] private Image hpFill;                 // Image -> Type=Filled -> Horizontal
    [SerializeField] private TextMeshProUGUI hpText;       // "50 / 100" (необов’язково)

    [Header("Weapon UI (контент)")]
    [SerializeField] private Image weaponIcon;             // іконка зброї
    [SerializeField] private TextMeshProUGUI weaponName;   // назва зброї

    [Header("Weapon UI (анімація)")]
    [SerializeField] private CanvasGroup weaponGroup;      // CanvasGroup на контейнері WeaponArea
    [SerializeField] private RectTransform weaponGroupRect;// RectTransform того ж контейнера
    [SerializeField, Range(0.05f, 1f)] private float swapDuration = 0.22f;
    [SerializeField, Range(1f, 1.5f)] private float popScale = 1.08f;

    [Header("Grenades UI")]
    [SerializeField] private GameObject ammoRoot;          // контейнер (ховаємо/показуємо)
    [SerializeField] private CanvasGroup ammoGroup;        // CanvasGroup на ammoRoot (для кросфейду)
    [SerializeField] private TextMeshProUGUI ammoText;     // "2/3"
    [SerializeField, Range(0.05f, 1f)] private float ammoFadeDuration = 0.18f;

    private IGrenadeInfo _grenade; // якщо активна зброя підтримує гранати
    private Coroutine _swapRoutine;
    private Vector3 _weaponGroupBaseScale = Vector3.one;

    private void Awake()
    {
        if (!playerHealth || !weaponSwitcher)
        {
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO)
            {
                if (!playerHealth) playerHealth = playerGO.GetComponent<Health>();
                if (!weaponSwitcher) weaponSwitcher = playerGO.GetComponentInChildren<WeaponSwitcher>(true);
            }
        }

        if (weaponGroupRect) _weaponGroupBaseScale = weaponGroupRect.localScale;

        if (playerHealth) playerHealth.OnChanged += OnHealthChanged;
        if (weaponSwitcher) weaponSwitcher.WeaponChanged += OnWeaponChanged;

        // Початковий стан груп
        if (weaponGroup) weaponGroup.alpha = 0f; // сховано до першого вибору
        if (ammoGroup) ammoGroup.alpha = 0f; // сховано до першого детекту гранат
        if (ammoRoot) ammoRoot.SetActive(false);
    }

    private void Start()
    {
        if (playerHealth) OnHealthChanged(playerHealth.CurrentHP, playerHealth.MaxHP);
        // Показати стартову зброю (або сховати, якщо її нема)
        OnWeaponChanged(weaponSwitcher ? weaponSwitcher.Current : null);
    }

    private void OnDestroy()
    {
        if (playerHealth) playerHealth.OnChanged -= OnHealthChanged;
        if (weaponSwitcher) weaponSwitcher.WeaponChanged -= OnWeaponChanged;
        UnsubscribeGrenade();
    }

    // ======= HP =======

    private void OnHealthChanged(float cur, float max)
    {
        float k = max > 0f ? cur / max : 0f;

        if (hpFill)
        {
            hpFill.type = Image.Type.Filled;
            hpFill.fillMethod = Image.FillMethod.Horizontal;
            hpFill.fillAmount = Mathf.Clamp01(k);
        }
        if (hpText) hpText.text = $"{Mathf.CeilToInt(cur)} / {Mathf.CeilToInt(max)}";
    }

    // ======= WEAPON =======

    private void OnWeaponChanged(WeaponBase w)
    {
        // зупинити попередню анімацію, якщо була
        if (_swapRoutine != null)
        {
            StopCoroutine(_swapRoutine);
            _swapRoutine = null;
        }
        _swapRoutine = StartCoroutine(SwapWeaponUI(w));
    }

    private IEnumerator SwapWeaponUI(WeaponBase newW)
    {
        // 1) фейдаут/стиснення
        yield return StartCoroutine(FadeAndScale(weaponGroup, weaponGroupRect, 1f, 0f, _weaponGroupBaseScale, _weaponGroupBaseScale * 0.96f, swapDuration * 0.5f));

        // 2) заміна контенту
        if (newW == null)
        {
            // очистити
            if (weaponIcon)
            {
                weaponIcon.sprite = null;
                weaponIcon.enabled = false;
            }
            if (weaponName) weaponName.text = "—";

            // ховаємо амуніцію
            UnsubscribeGrenade();
            yield return StartCoroutine(FadeAmmo(false));
        }
        else
        {
            if (weaponIcon)
            {
                weaponIcon.sprite = newW.Icon;
                weaponIcon.enabled = (newW.Icon != null);
            }
            if (weaponName) weaponName.text = newW.DisplayName;

            // гранати
            UnsubscribeGrenade();
            if (newW is IGrenadeInfo gi)
            {
                _grenade = gi;
                _grenade.GrenadeCountChanged += OnGrenadeChanged;
                OnGrenadeChanged(_grenade.CurrentGrenades, _grenade.MaxGrenades);
                yield return StartCoroutine(FadeAmmo(true));
            }
            else
            {
                yield return StartCoroutine(FadeAmmo(false));
            }
        }

        // 3) фейдин/поп-scale
        Vector3 pop = _weaponGroupBaseScale * popScale;
        yield return StartCoroutine(FadeAndScale(weaponGroup, weaponGroupRect, 0f, 1f, _weaponGroupBaseScale * 0.98f, pop, swapDuration * 0.4f));
        // повернути до базового масштабу (легкий “поп”)
        if (weaponGroupRect) weaponGroupRect.localScale = _weaponGroupBaseScale;

        _swapRoutine = null;
    }

    private IEnumerator FadeAmmo(bool show)
    {
        if (!ammoRoot && !ammoGroup) yield break;

        if (show)
        {
            if (ammoRoot && !ammoRoot.activeSelf) ammoRoot.SetActive(true);
            if (ammoGroup) yield return StartCoroutine(FadeCanvasGroup(ammoGroup, ammoGroup.alpha, 1f, ammoFadeDuration));
        }
        else
        {
            if (ammoGroup) yield return StartCoroutine(FadeCanvasGroup(ammoGroup, ammoGroup.alpha, 0f, ammoFadeDuration));
            if (ammoRoot) ammoRoot.SetActive(false);
        }
    }

    private void OnGrenadeChanged(int cur, int max)
    {
        if (ammoText) ammoText.text = $"{cur}/{max}";
    }

    private void UnsubscribeGrenade()
    {
        if (_grenade != null)
        {
            _grenade.GrenadeCountChanged -= OnGrenadeChanged;
            _grenade = null;
        }
    }

    // ======= Helpers (анімації) =======

    private IEnumerator FadeAndScale(CanvasGroup cg, RectTransform rt,
                                     float aFrom, float aTo,
                                     Vector3 sFrom, Vector3 sTo,
                                     float duration)
    {
        if (!cg && !rt) yield break;

        float t = 0f;
        if (cg) cg.alpha = aFrom;
        if (rt) rt.localScale = sFrom;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // незалежно від Time.timeScale
            float k = Mathf.Clamp01(t / duration);
            // легкий ease (SmoothStep)
            float e = k * k * (3f - 2f * k);

            if (cg) cg.alpha = Mathf.Lerp(aFrom, aTo, e);
            if (rt) rt.localScale = Vector3.Lerp(sFrom, sTo, e);

            yield return null;
        }

        if (cg) cg.alpha = aTo;
        if (rt) rt.localScale = sTo;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (!cg) yield break;
        float t = 0f;
        cg.alpha = from;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            float e = k * k * (3f - 2f * k);
            cg.alpha = Mathf.Lerp(from, to, e);
            yield return null;
        }
        cg.alpha = to;
    }
}
