using UnityEngine;
using System.Collections;   // <-- потрібно для IEnumerator

public class MeleeWeapon : WeaponBase
{
    [Header("Swing")]
    [SerializeField] private Transform pivot;
    [SerializeField] private float arcDegrees = 120f;
    [SerializeField] private float swingTime = 0.18f;
    [SerializeField] private Collider hitbox;
    [SerializeField] private float knockback = 5f;

    private Quaternion startRot, endRot;

    private void Start()
    {
        if (!pivot) pivot = transform;
        startRot = Quaternion.Euler(0, -arcDegrees * 0.5f, 0);
        endRot = Quaternion.Euler(0, arcDegrees * 0.5f, 0);
        if (hitbox) hitbox.enabled = false;
    }

    protected override void OnAttack()
    {
        StopAllCoroutines();
        StartCoroutine(Swing());
    }

    private IEnumerator Swing()
    {
        float t = 0f;
        if (hitbox) hitbox.enabled = true;

        while (t < swingTime)
        {
            float k = t / swingTime;
            float s = Mathf.SmoothStep(0f, 1f, k);
            pivot.localRotation = Quaternion.Slerp(startRot, endRot, s);
            t += Time.deltaTime;
            yield return null;
        }

        pivot.localRotation = Quaternion.identity;
        if (hitbox) hitbox.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hitbox == null || !hitbox.enabled) return;

        if (other.TryGetComponent<Health>(out var hp))
        {
            hp.TakeDamage(damage);
            if (other.attachedRigidbody)
            {
                Vector3 dir = (other.transform.position - pivot.position).normalized;
                other.attachedRigidbody.AddForce(dir * knockback, ForceMode.VelocityChange);
            }
        }
    }
}
