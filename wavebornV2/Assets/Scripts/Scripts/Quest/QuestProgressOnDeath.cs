using UnityEngine;

[RequireComponent(typeof(Health))]
public class QuestProgressOnDeath : MonoBehaviour
{
    [SerializeField] private string questId;
    [SerializeField] private int amount = 1;

    private void Awake()
    {
        var hp = GetComponent<Health>();
        hp.OnDied += () =>
        {
            if (!string.IsNullOrEmpty(questId) && QuestManager.Instance != null)
                QuestManager.Instance.AddProgress(questId, amount);
        };
    }
}
