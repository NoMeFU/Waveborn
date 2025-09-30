using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MedkitPickup : MonoBehaviour
{
    [Header("Heal")]
    [SerializeField] private float healAmount = 30f;     // ������ ����������
    [SerializeField] private bool overhealToMax = true;  // �� �������� MaxHP (true = clamp)

    [Header("Lifetime")]
    [SerializeField] private float autoDestroyAfter = 0f; // 0 = �� �������� �����������

    [Header("FX (�������)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField] private GameObject pickupVfx;        // ����� ��� �����
    [SerializeField] private float vfxLifetime = 2f;

    private bool _consumed;

    private void Awake()
    {
        // ������ �� ���� ���������
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

        // ������ Health �� ��'���, �� ������ � ������
        if (!other.TryGetComponent<Health>(out var hp))
        {
            // ��� � ������� (������ �������� ������ ������)
            hp = other.GetComponentInParent<Health>();
        }

        if (hp == null) return;            // �� ��� �볺��
        if (!hp.IsAlive) return;           // ������� �� �����

        // ����������, �� � ���� �������
        float before = hp.CurrentHP;
        float target = overhealToMax ? Mathf.Min(hp.MaxHP, before + healAmount)
                                     : before + healAmount;
        float delta = target - before;
        if (delta <= 0f) return;

        // ��������
        hp.Heal(delta);

        // FX
        if (pickupSfx && audioSource)
            audioSource.PlayOneShot(pickupSfx);

        if (pickupVfx)
        {
            var vfx = Instantiate(pickupVfx, transform.position, Quaternion.identity);
            if (vfxLifetime > 0f) Destroy(vfx, vfxLifetime);
        }

        // ��������� �������
        _consumed = true;
        // ���� ������� ���� �� ����� � ��'��� � ����� ������� ����� �����
        float delay = (pickupSfx && audioSource) ? pickupSfx.length * 0.9f : 0f;
        Destroy(gameObject, delay);
    }
}
