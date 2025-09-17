using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WeaponPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private GameObject weaponPrefab; // <- ось це поле для перетягування зброї
    [SerializeField] private float lifeTime = 60f;    // час існування пікапа

    private Coroutine autoDestroy;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void Start()
    {
        if (weaponPrefab == null)
            Debug.LogWarning($"{name}: Weapon Prefab не призначений у інспекторі!");

        if (lifeTime > 0)
            autoDestroy = StartCoroutine(DestroyAfter(lifeTime));
    }

    private IEnumerator DestroyAfter(float t)
    {
        yield return new WaitForSeconds(t);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        TryPickup(other.gameObject);
    }

    public void TryPickup(GameObject player)
    {
        if (!weaponPrefab)
        {
            Debug.LogWarning($"{name}: weaponPrefab == null, нічого підбирати.");
            return;
        }

        var switcher = player.GetComponentInChildren<WeaponSwitcher>();
        if (!switcher)
        {
            Debug.LogWarning("Pickup: у Player немає WeaponSwitcher.");
            return;
        }

        switcher.AddWeaponFromPrefab(weaponPrefab);
        if (autoDestroy != null) StopCoroutine(autoDestroy);
        Destroy(gameObject);
    }

    // якщо ти спавниш пікап кодом (DropCurrent) — викличеш це
    public void Setup(GameObject prefab)
    {
        weaponPrefab = prefab;
    }
}
