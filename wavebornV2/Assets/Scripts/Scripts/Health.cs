using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHP = 50f;

    public System.Action OnDied;

    private float hp;

    private void Awake()
    {
        hp = maxHP;
    }

    public void TakeDamage(float dmg)
    {
        hp -= dmg;
        if (hp <= 0f)
        {
            OnDied?.Invoke();
            Destroy(gameObject);
        }
    }
}
