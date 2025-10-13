using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NpcInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactionHint; // UI підказка “Натисни E”

    [Header("Linked Components")]
    [SerializeField] private DialogueUI dialogueUI;

    private Transform player;
    private bool playerInRange = false;

    private void Start()
    {
        // Пошук гравця
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Автоматичний пошук DialogueUI
        if (!dialogueUI)
            dialogueUI = GetComponentInChildren<DialogueUI>();

        // Переконуємося, що колайдер тригер
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        // Ховаємо підказку спочатку
        if (interactionHint)
            interactionHint.SetActive(false);
    }

    private void Update()
    {
        // Якщо діалог вже відкритий — не перевіряємо взаємодію
        if (DialogueUI.IsAnyOpen())
            return;

        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionHint)
                interactionHint.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionHint)
                interactionHint.SetActive(false);

            // Закрити UI, якщо вийшов із зони
            if (dialogueUI && dialogueUI.IsOpen)
                dialogueUI.CloseAll();
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
}
