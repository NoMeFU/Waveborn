using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
    [SerializeField] private float damage = 5f;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private float cooldown = 0.5f;
    private float cd;

    private void Update() { if (cd > 0f) cd -= Time.deltaTime; }

    private void OnTriggerEnter(Collider other)
    {
        if (cd > 0f) return;
        if (!other.CompareTag(targetTag)) return;

        if (other.TryGetComponent<Health>(out var hp))
        {
            hp.TakeDamage(damage);
            cd = cooldown;
        }
    }
}
