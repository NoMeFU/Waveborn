using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class GrenadeAmmoPickup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int amount = 1;                 // ск≥льки гранат додати
    [SerializeField] private float lifeTime = 60f;           // авто-деспаун
    [SerializeField] private GameObject grenadeWeaponPrefab; // префаб GrenadeWeapon (з WeaponBase усередин≥)

    private void Reset()
    {
        var col = GetComponent<Collider>(); col.isTrigger = true;
        var rb = GetComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
    }

    private void Start()
    {
        if (lifeTime > 0) StartCoroutine(AutoDestroy());
    }

    private IEnumerator AutoDestroy()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var switcher = other.GetComponentInChildren<WeaponSwitcher>();
        if (!switcher) return;

        // 1) €кщо GrenadeWeapon уже Ї Ч поповнюЇмо
        if (switcher.TryGetWeaponOfType<GrenadeWeapon>(out var gw))
        {
            gw.AddGrenades(amount);
            Destroy(gameObject);
            return;
        }

        // 2) €кщо ще немаЇ Ч додаЇмо зброю у слоти (враховуючи л≥м≥т/дроп поточноњ) ≥ також поповнюЇмо
        if (grenadeWeaponPrefab != null)
        {
            switcher.AddWeaponFromPrefab(grenadeWeaponPrefab);

            // знову шукаЇмо щойно додану гранату ≥ додаЇмо к≥льк≥сть (на випаде €кщо дефолт 0)
            if (switcher.TryGetWeaponOfType<GrenadeWeapon>(out var gw2))
                gw2.AddGrenades(amount);

            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning($"{name}: grenadeWeaponPrefab is not assigned on GrenadeAmmoPickup.");
        }
    }
}
