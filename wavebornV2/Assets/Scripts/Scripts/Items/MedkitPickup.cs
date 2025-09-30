using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MedkitPickup : MonoBehaviour
{
    [Header("Heal")]
    [SerializeField] private float healAmount = 30f;     // ск≥льки в≥дновлюЇмо
    [SerializeField] private bool overhealToMax = true;  // не перевищуЇ MaxHP (true = clamp)

    [Header("Lifetime")]
    [SerializeField] private float autoDestroyAfter = 0f; // 0 = не видал€ти автоматично

    [Header("FX (опц≥йно)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField] private GameObject pickupVfx;        // ефект при п≥дбор≥
    [SerializeField] private float vfxLifetime = 2f;

    private bool _consumed;

    private void Awake()
    {
        // тригер маЇ бути вв≥мкнений
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (autoDestroyAfter > 0f)
            Destroy(gameObject, autoDestroyAfter);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_consumed) return;

        // шукаЇмо Health на об'Їкт≥, що зайшов у тригер
        if (!other.TryGetComponent<Health>(out var hp))
        {
            // або у батьках (раптом колайдер окремо висить)
            hp = other.GetComponentInParent<Health>();
        }

        if (hp == null) return;            // не наш кл≥Їнт
        if (!hp.IsAlive) return;           // мертвих не л≥куЇмо

        // обчислюЇмо, чи Ї сенс л≥кувати
        float before = hp.CurrentHP;
        float target = overhealToMax ? Mathf.Min(hp.MaxHP, before + healAmount)
                                     : before + healAmount;
        float delta = target - before;
        if (delta <= 0f) return;

        // зц≥люЇмо
        hp.Heal(delta);

        // FX
        if (pickupSfx && audioSource)
            audioSource.PlayOneShot(pickupSfx);

        if (pickupVfx)
        {
            var vfx = Instantiate(pickupVfx, transform.position, Quaternion.identity);
            if (vfxLifetime > 0f) Destroy(vfx, vfxLifetime);
        }

        // прибираЇмо предмет
        _consumed = true;
        // €кщо звучить ауд≥о на цьому ж об'Їкт≥ Ч можна знищити трохи п≥зн≥ше
        float delay = (pickupSfx && audioSource) ? pickupSfx.length * 0.9f : 0f;
        Destroy(gameObject, delay);
    }
}
