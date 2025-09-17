using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioSource audioSource;

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 5f;

    private float damage;
    private Vector3 dir;
    private float speed;
    private LayerMask hitMask;
    private float life;

    public void Init(float dmg, Vector3 direction, float spd, LayerMask mask)
    {
        damage = dmg;
        dir = direction.normalized;
        speed = spd;
        hitMask = mask;
        life = lifeTime;
    }

    private void Update()
    {
        transform.position += dir * speed * Time.deltaTime;

        life -= Time.deltaTime;
        if (life <= 0f) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;

        if (other.TryGetComponent<Health>(out var hp))
        {
            hp.TakeDamage(damage);
            if (hitClip && audioSource) audioSource.PlayOneShot(hitClip);
        }
        Destroy(gameObject);
    }
}
