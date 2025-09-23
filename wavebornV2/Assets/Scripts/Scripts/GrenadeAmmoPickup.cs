using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class GrenadeAmmoPickup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int amount = 1;                 // ������ ������ ������
    [SerializeField] private float lifeTime = 60f;           // ����-�������
    [SerializeField] private GameObject grenadeWeaponPrefab; // ������ GrenadeWeapon (� WeaponBase ��������)

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

        // 1) ���� GrenadeWeapon ��� � � ����������
        if (switcher.TryGetWeaponOfType<GrenadeWeapon>(out var gw))
        {
            gw.AddGrenades(amount);
            Destroy(gameObject);
            return;
        }

        // 2) ���� �� ���� � ������ ����� � ����� (���������� ���/���� �������) � ����� ����������
        if (grenadeWeaponPrefab != null)
        {
            switcher.AddWeaponFromPrefab(grenadeWeaponPrefab);

            // ����� ������ ����� ������ ������� � ������ ������� (�� ������ ���� ������ 0)
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
