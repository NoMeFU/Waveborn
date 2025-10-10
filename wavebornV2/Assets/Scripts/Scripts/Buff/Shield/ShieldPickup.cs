using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] private float shieldDuration = 15f;

    [Header("Pickup Settings")]
    [SerializeField] private bool autoPickup = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobAmount = 0.3f;

    [Header("Effects")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private AudioClip pickupSound;

    [Header("Respawn")]
    [SerializeField] private bool respawnAfterPickup = true;
    [SerializeField] private float respawnTime = 30f;

    private Vector3 startPosition;
    private bool isPickedUp;
    private Renderer[] renderers;
    private Collider pickupCollider;

    private void Awake()
    {
        startPosition = transform.position;
        renderers = GetComponentsInChildren<Renderer>();
        pickupCollider = GetComponent<Collider>();

        if (!pickupCollider)
        {
            pickupCollider = gameObject.AddComponent<SphereCollider>();
            ((SphereCollider)pickupCollider).radius = 1f;
            pickupCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        if (isPickedUp) return;

        // Обертання
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Левітація вгору-вниз
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp || !autoPickup) return;

        if (other.CompareTag(playerTag))
        {
            TryPickup(other.gameObject);
        }
    }

    public bool TryPickup(GameObject player)
    {
        if (isPickedUp) return false;

        ShieldController shield = player.GetComponent<ShieldController>();
        if (!shield)
        {
            shield = player.GetComponentInChildren<ShieldController>();
        }

        if (!shield)
        {
            Debug.LogWarning($"ShieldPickup: Гравець {player.name} не має компонента ShieldController!");
            return false;
        }

        // Активувати щит
        shield.ActivateShield(shieldDuration);

        // Ефекти
        OnPickedUp();

        return true;
    }

    private void OnPickedUp()
    {
        isPickedUp = true;

        // Звук
        if (pickupSound)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // Ефект частинок
        if (pickupEffect)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        // Сховати об'єкт
        SetVisible(false);

        Debug.Log("<color=green>✅ Shield Pickup підібрано!</color>");

        // Респавн
        if (respawnAfterPickup)
        {
            Invoke(nameof(Respawn), respawnTime);
        }
        else
        {
            Destroy(gameObject, 0.1f);
        }
    }

    private void Respawn()
    {
        isPickedUp = false;
        transform.position = startPosition;
        SetVisible(true);

        Debug.Log("<color=cyan>🔄 Shield Pickup респавнувся!</color>");
    }

    private void SetVisible(bool visible)
    {
        foreach (var r in renderers)
        {
            if (r) r.enabled = visible;
        }

        if (pickupCollider)
        {
            pickupCollider.enabled = visible;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}