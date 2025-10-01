// Assets/Scripts/Quests/EnemyTag.cs
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyTag : MonoBehaviour
{
    public string enemyId = "zombie_basic";

    private Health _hp;

    private void Awake()
    {
        _hp = GetComponent<Health>();
        _hp.OnDied += OnDied;
    }

    private void OnDestroy()
    {
        if (_hp != null) _hp.OnDied -= OnDied;
    }

    private void OnDied()
    {
        if (QuestManager.Instance)
            QuestManager.Instance.RegisterEnemyKill(enemyId);
    }
}
