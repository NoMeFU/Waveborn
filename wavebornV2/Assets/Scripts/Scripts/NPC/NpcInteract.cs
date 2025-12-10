using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NpcInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactionHint;

    [Header("Linked Components")]
    [SerializeField] private DialogueUI dialogueUI;

    private Transform player;
    private bool playerInRange = false;

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

        Debug.Log($"[{name}] NPC ініціалізовано. Відстань взаємодії: {interactDistance}m");
    }

    private void Update()
    {
        if (!player) return;

        // Якщо діалог вже відкритий — не перевіряємо взаємодію
        if (DialogueUI.IsAnyOpen())
        {
            if (interactionHint && interactionHint.activeSelf)
                interactionHint.SetActive(false);
            return;
        }

        // Перевірка відстані до гравця
        float distance = Vector3.Distance(transform.position, player.position);
        bool inRange = distance <= interactDistance;

        // Оновлюємо статус тільки при зміні
        if (inRange != playerInRange)
        {
            playerInRange = inRange;

            if (interactionHint)
                interactionHint.SetActive(playerInRange);

            if (playerInRange)
                Debug.Log($"[{name}] Гравець увійшов в зону взаємодії (відстань: {distance:F2}m)");
        }

        // Перевірка натискання клавіші
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            Debug.Log($"[{name}] Натиснуто {interactKey} - відкриваю діалог!");
            Interact();
        }
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