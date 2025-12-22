using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeNpc : MonoBehaviour
{
    [Header("Параметри взаємодії")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Посилання")]
    [SerializeField] private UpgradeSystem upgradeSystem;

    [Header("UI")]
    [SerializeField] private GameObject interactButton; // Кнопка для мобільних
    [SerializeField] private TextMeshProUGUI promptText; // Текст "Натисни E" для ПК (опціонально)

    private Transform player;
    private bool isPlayerNear = false;
    private bool isMobile = false;

    private void Start()
    {
        // Визначаємо чи це мобільний пристрій
        isMobile = Application.isMobilePlatform;

#if UNITY_ANDROID || UNITY_IOS
            isMobile = true;
#endif

        // Шукаємо гравця
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;
        else Debug.LogWarning("⚠️ Player не знайдено! Переконайся, що у гравця тег 'Player'.");

        // Якщо UpgradeSystem не задано вручну — знайдемо автоматично
        if (!upgradeSystem)
            upgradeSystem = FindObjectOfType<UpgradeSystem>();

        // Ховаємо UI на старті
        if (interactButton) interactButton.SetActive(false);
        if (promptText) promptText.gameObject.SetActive(false);

        // Підключаємо кнопку для мобільних
        if (interactButton && isMobile)
        {
            var button = interactButton.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(OnInteractButtonClick);
        }
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
            ShowInteractUI(true);
        }
        // Гравець відійшов
        else if (!nowNear && isPlayerNear)
        {
            isPlayerNear = false;
            ShowInteractUI(false);
            upgradeSystem.CloseMenu();
        }

        // Якщо поруч і це ПК — можна натискати E
        if (isPlayerNear && !isMobile && Input.GetKeyDown(interactKey))
        {
            ToggleUpgradeMenu();
        }
    }

    // Показати/сховати UI взаємодії
    private void ShowInteractUI(bool show)
    {
        if (isMobile)
        {
            // На мобільних показуємо кнопку
            if (interactButton) interactButton.SetActive(show);
        }
        else
        {
            // На ПК показуємо текст
            if (promptText)
            {
                promptText.gameObject.SetActive(show);
                if (show) promptText.text = "Натисни [E] щоб відкрити меню";
            }
            else
            {
                // Якщо немає TextMeshPro - виводимо в консоль
                if (show) Debug.Log("🟢 Гравець поруч. Натисни [E] щоб відкрити меню.");
            }
        }
    }

    // Обробник кнопки для мобільних
    private void OnInteractButtonClick()
    {
        if (isPlayerNear)
            ToggleUpgradeMenu();
    }

    // Відкрити/закрити меню апгрейдів
    private void ToggleUpgradeMenu()
    {
        if (upgradeSystem.IsOpen)
            upgradeSystem.CloseMenu();
        else
            upgradeSystem.OpenMenu();
    }

    // Для налагодження - показує радіус взаємодії
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}