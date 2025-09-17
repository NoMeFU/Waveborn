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
            // на випадок, якщо тригер не спрацював — підбір вручну
            Collider[] hits = Physics.OverlapSphere(transform.position, 1.5f);
            foreach (var h in hits)
            {
                var p = h.GetComponentInParent<WeaponPickup>();
                if (p) { p.TryPickup(gameObject); break; }
            }
        }
    }
}
