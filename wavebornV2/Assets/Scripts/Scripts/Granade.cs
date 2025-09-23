using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Grenade : MonoBehaviour
{
    [Header("Fuse & Explosion")]
    [SerializeField] private float fuseTime = 2.5f;
    [SerializeField] private float radius = 4.5f;
    [SerializeField] private float damage = 40f;
    [SerializeField] private float explosionForce = 12f;
    [SerializeField] private LayerMask damageMask = ~0; // ���� �������

    [Header("FX")]
    [SerializeField] private AudioClip explodeClip;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem explodeVfx;

    private bool exploded;

    private void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        StartCoroutine(Fuse());
    }

    private IEnumerator Fuse()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;

        // ���� + VFX
        if (explodeClip && audioSource) audioSource.PlayOneShot(explodeClip);
        if (explodeVfx) Instantiate(explodeVfx, transform.position, Quaternion.identity);

        // ����� � �������� ����
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, damageMask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            // damage
            if (h.TryGetComponent<Health>(out var hp))
            {
                // ������ ��������� � �������� (�������)
                float dist = Vector3.Distance(transform.position, h.ClosestPoint(transform.position));
                float falloff = Mathf.Clamp01(1f - dist / radius);
                hp.TakeDamage(damage * falloff);
            }

            // �������� ����
            if (h.attachedRigidbody)
                h.attachedRigidbody.AddExplosionForce(explosionForce, transform.position, radius, 0.25f, ForceMode.VelocityChange);
        }

        // ��������, ��� ���� ����� �������, ���� ���� �� ���� ������
        Destroy(gameObject, 0.02f);
    }

    // �������: ������� ����� �� ���/�����
    private void OnTriggerEnter(Collider other)
    {
        // ���� ����� ���������� �� �������, ����� �������� layer/����
        // if (other.CompareTag("Projectile")) Explode();
    }
}
