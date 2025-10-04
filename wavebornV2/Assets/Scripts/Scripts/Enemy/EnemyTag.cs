using UnityEngine;

public class EnemyTag : MonoBehaviour
{
    [SerializeField] private string questId = "kill_5_enemies";

    private void OnDestroy()
    {
        if (QuestManager.Instance != null && !string.IsNullOrEmpty(questId))
        {
            QuestManager.Instance.AddProgress(questId, 1);
        }
    }
}
