using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private NPCDialogue currentDialogue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // 🔹 Перевірка, чи зараз якийсь діалог відкритий
    public bool IsDialogueOpen()
    {
        return currentDialogue != null && currentDialogue.IsOpen;
    }

    // 🔹 Відкрити діалог (якщо жоден не активний)
    public void TryOpen(NPCDialogue dialogue)
    {
        if (IsDialogueOpen())
            return; // блокуємо інші діалоги

        currentDialogue = dialogue;
        currentDialogue.StartDialogue();
    }

    // 🔹 Закрити поточний діалог
    public void CloseCurrent()
    {
        if (currentDialogue != null)
        {
            currentDialogue.EndDialogue();
            currentDialogue = null;
        }
    }

    // 🔹 Якщо NPC сам закриває діалог — повідомляє менеджера
    public void NotifyClosed(NPCDialogue dialogue)
    {
        if (currentDialogue == dialogue)
            currentDialogue = null;
    }
}
