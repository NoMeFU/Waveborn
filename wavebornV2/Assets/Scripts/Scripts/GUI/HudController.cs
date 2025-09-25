using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private WeaponSwitcher weaponSwitcher;

    [Header("HP")]
    [SerializeField] private Image hpFill;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Weapon")]
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponName;

    [Header("Grenades UI")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject ammoRoot;

    private IGrenadeInfo _subscribedGrenade;

    private void Awake()
    {
        if (!playerHealth) playerHealth = FindObjectOfType<Health>();
        if (!weaponSwitcher) weaponSwitcher = FindObjectOfType<WeaponSwitcher>();

        if (playerHealth) playerHealth.OnChanged += OnHealthChanged;          // ← OnChanged
        if (weaponSwitcher) weaponSwitcher.WeaponChanged += OnWeaponChanged;    // подія зі свічера (див. п.3)
    }

    private void Start()
    {
        if (playerHealth) OnHealthChanged(playerHealth.CurrentHP, playerHealth.MaxHP);     // ← CurrentHP/MaxHP
        if (weaponSwitcher && weaponSwitcher.Current) OnWeaponChanged(weaponSwitcher.Current);
        else SetAmmoVisible(false);
    }

    private void OnDestroy()
    {
        if (playerHealth) playerHealth.OnChanged -= OnHealthChanged;
        if (weaponSwitcher) weaponSwitcher.WeaponChanged -= OnWeaponChanged;
        UnsubscribeGrenade();
    }

    private void OnHealthChanged(float cur, float max)
    {
        if (hpFill)
        {
            hpFill.type = Image.Type.Filled;
            hpFill.fillMethod = Image.FillMethod.Horizontal;
            hpFill.fillAmount = max > 0f ? cur / max : 0f;
        }
        if (hpText) hpText.text = $"{Mathf.CeilToInt(cur)} / {Mathf.CeilToInt(max)}";
    }

    private void OnWeaponChanged(WeaponBase w)
    {
        if (weaponIcon)
        {
            weaponIcon.sprite = w ? w.Icon : null;
            weaponIcon.enabled = (w && w.Icon);
        }
        if (weaponName) weaponName.text = w ? w.DisplayName : "—";

        UnsubscribeGrenade();
        if (w is IGrenadeInfo gi)
        {
            _subscribedGrenade = gi;
            _subscribedGrenade.GrenadeCountChanged += OnGrenadeChanged;
            OnGrenadeChanged(gi.CurrentGrenades, gi.MaxGrenades);
            SetAmmoVisible(true);
        }
        else
        {
            SetAmmoVisible(false);
        }
    }

    private void OnGrenadeChanged(int cur, int max)
    {
        if (ammoText) ammoText.text = $"{cur}/{max}";
    }

    private void SetAmmoVisible(bool on)
    {
        if (ammoRoot) ammoRoot.SetActive(on);
        else if (ammoText) ammoText.gameObject.SetActive(on);
    }

    private void UnsubscribeGrenade()
    {
        if (_subscribedGrenade != null)
        {
            _subscribedGrenade.GrenadeCountChanged -= OnGrenadeChanged;
            _subscribedGrenade = null;
        }
    }
}
