using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private Collider pickupTrigger;
    [SerializeField] private float lifeTime = 60f;
    [SerializeField] private float pickupDelay = 0.5f;

    private bool canBePickedUp = false;
    private float spawnTime;

    private void Start()
    {
        spawnTime = Time.time;

        // Активуємо фізику для об’єкта
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        // Перевірка наявності тригера
        if (pickupTrigger)
        {
            pickupTrigger.isTrigger = true;
            var triggerObj = pickupTrigger.gameObject;
            var helper = triggerObj.GetComponent<PickupTriggerHelper>();
            if (!helper)
                helper = triggerObj.AddComponent<PickupTriggerHelper>();
            helper.Initialize(this);
        }

        if (lifeTime > 0)
            Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (!canBePickedUp && Time.time - spawnTime >= pickupDelay)
            canBePickedUp = true;
    }

    public void Setup(GameObject prefab)
    {
        weaponPrefab = prefab;
    }

    public void TryPickup(GameObject player)
    {
        if (!canBePickedUp || !weaponPrefab) return;
        var switcher = player.GetComponentInChildren<WeaponSwitcher>();
        if (switcher && switcher.AddWeaponFromPrefab(weaponPrefab))
            Destroy(gameObject);
    }
}

public class PickupTriggerHelper : MonoBehaviour
{
    private WeaponPickup parentPickup;

    public void Initialize(WeaponPickup pickup)
    {
        parentPickup = pickup;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        parentPickup?.TryPickup(other.gameObject);
    }
}
