using UnityEngine;

public class PlayerPickupController : MonoBehaviour
{
    [SerializeField] private WeaponSwitcher weaponSwitcher;

    private void Awake()
    {
        if (!weaponSwitcher) weaponSwitcher = GetComponentInChildren<WeaponSwitcher>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            weaponSwitcher?.DropCurrent();

        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f);
            foreach (var h in hits)
            {
                var pickup = h.GetComponentInParent<WeaponPickup>();
                if (pickup)
                {
                    var prefab = GetWeaponPrefab(pickup);
                    if (prefab && weaponSwitcher.AddWeaponFromPrefab(prefab))
                        Destroy(pickup.gameObject);
                    break;
                }
            }
        }
    }

    private GameObject GetWeaponPrefab(WeaponPickup pickup)
    {
        // доступ через приватне поле
        var field = typeof(WeaponPickup).GetField("weaponPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(pickup) as GameObject;
    }
}
