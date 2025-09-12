using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioSource audioSource;

    private float damage;
    private Vector3 dir;
    private float speed;
    private LayerMask hitMask;

    public void Init(float dmg, Vector3 direction, float spd, LayerMask mask)
    {
        damage = dmg;
        dir = direction.normalized;
        speed = spd;
        hitMask = mask;
    }

    private void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;

        if (other.TryGetComponent<Health>(out var hp))
        {
            hp.TakeDamage(damage);

            // 🔊 звук влучання
            if (hitClip && audioSource)
                audioSource.PlayOneShot(hitClip);
        }

        Destroy(gameObject);
    }
}
