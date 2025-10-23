using UnityEngine;

public class UpgradeNpc : MonoBehaviour
{
    [Header("Параметри взаємодії")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Посилання")]
    [SerializeField] private UpgradeSystem upgradeSystem;

    private Transform player;
    private bool isPlayerNear = false;

    private void Start()
    {
        // Шукаємо гравця
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;
        else Debug.LogWarning("⚠️ Player не знайдено! Переконайся, що у гравця тег 'Player'.");

        // Якщо UpgradeSystem не задано вручну — знайдемо автоматично
        if (!upgradeSystem)
            upgradeSystem = FindObjectOfType<UpgradeSystem>();
    }

    private void Update()
    {
        if (player == null || upgradeSystem == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool nowNear = distance <= interactDistance;

        // Гравець наблизився вперше
        if (nowNear && !isPlayerNear)
        {
            isPlayerNear = true;
            Debug.Log("🟢 Гравець поруч. Натисни [E] щоб відкрити меню.");
        }
        // Гравець відійшов
        else if (!nowNear && isPlayerNear)
        {
            isPlayerNear = false;
            upgradeSystem.CloseMenu();
        }

        // Якщо поруч — можна натискати E
        if (isPlayerNear && Input.GetKeyDown(interactKey))
        {
            // Якщо меню вже відкрите — закриваємо
            if (upgradeSystem.IsOpen)
                upgradeSystem.CloseMenu();
            else
                upgradeSystem.OpenMenu();
        }
    }
}
