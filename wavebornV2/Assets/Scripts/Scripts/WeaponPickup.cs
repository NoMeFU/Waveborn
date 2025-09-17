using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WeaponPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private GameObject weaponPrefab; // <- ��� �� ���� ��� ������������� ����
    [SerializeField] private float lifeTime = 60f;    // ��� ��������� �����

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
            Debug.LogWarning($"{name}: Weapon Prefab �� ����������� � ���������!");

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
            Debug.LogWarning($"{name}: weaponPrefab == null, ����� ��������.");
            return;
        }

        var switcher = player.GetComponentInChildren<WeaponSwitcher>();
        if (!switcher)
        {
            Debug.LogWarning("Pickup: � Player ���� WeaponSwitcher.");
            return;
        }

        switcher.AddWeaponFromPrefab(weaponPrefab);
        if (autoDestroy != null) StopCoroutine(autoDestroy);
        Destroy(gameObject);
    }

    // ���� �� ������� ���� ����� (DropCurrent) � �������� ��
    public void Setup(GameObject prefab)
    {
        weaponPrefab = prefab;
    }
}
