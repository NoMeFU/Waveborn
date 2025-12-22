using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class NpcInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactionHint; // Для ПК
    [SerializeField] private GameObject mobileInteractButton; // Для мобільних

    [Header("Linked Components")]
    [SerializeField] private DialogueUI dialogueUI;

    private Transform player;
    private bool playerInRange = false;
    private bool isMobile = false;

    private void Awake()
    {
        // Визначаємо чи це мобільний пристрій
        isMobile = Application.isMobilePlatform;

#if UNITY_ANDROID || UNITY_IOS
            isMobile = true;
#endif
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (!player)
        {
            Debug.LogError($"[{name}] Не знайдено гравця з тегом 'Player'!");
            return;
        }

        if (!dialogueUI)
            dialogueUI = GetComponentInChildren<DialogueUI>();
        if (!dialogueUI)
            Debug.LogError($"[{name}] DialogueUI не знайдено!");

        if (interactionHint)
            interactionHint.SetActive(false);

        if (mobileInteractButton)
            mobileInteractButton.SetActive(false);

        // Підключаємо мобільну кнопку
        if (mobileInteractButton && isMobile)
        {
            var button = mobileInteractButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnMobileInteractClick);
            }
        }

        Debug.Log($"[{name}] NPC ініціалізовано. Відстань взаємодії: {interactDistance}m. Платформа: {(isMobile ? "Mobile" : "PC")}");
    }

    private void Update()
    {
        if (!player) return;

        // Якщо діалог вже відкритий — не перевіряємо взаємодію
        if (DialogueUI.IsAnyOpen())
        {
            ShowInteractionUI(false);
            return;
        }

        // Перевірка відстані до гравця
        float distance = Vector3.Distance(transform.position, player.position);
        bool inRange = distance <= interactDistance;

        // Оновлюємо статус тільки при зміні
        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            ShowInteractionUI(playerInRange);

            if (playerInRange)
                Debug.Log($"[{name}] Гравець увійшов в зону взаємодії (відстань: {distance:F2}m)");
        }

        // Перевірка натискання клавіші (тільки для ПК)
        if (playerInRange && !isMobile && Input.GetKeyDown(interactKey))
        {
            Debug.Log($"[{name}] Натиснуто {interactKey} - відкриваю діалог!");
            Interact();
        }
    }

    // Показати/сховати UI взаємодії
    private void ShowInteractionUI(bool show)
    {
        if (isMobile)
        {
            // На мобільних показуємо кнопку
            if (mobileInteractButton)
                mobileInteractButton.SetActive(show);
        }
        else
        {
            // На ПК показуємо підказку
            if (interactionHint)
                interactionHint.SetActive(show);
        }
    }

    // Обробник мобільної кнопки
    private void OnMobileInteractClick()
    {
        if (!playerInRange) return;

        Debug.Log($"[{name}] Натиснуто мобільну кнопку - відкриваю діалог!");
        Interact();
    }

    private void Interact()
    {
        if (!dialogueUI)
        {
            Debug.LogWarning($"NPC '{name}' не має підключеного DialogueUI!");
            return;
        }

        dialogueUI.OpenMenu();
    }

    // Візуалізація зони взаємодії в Scene View
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}