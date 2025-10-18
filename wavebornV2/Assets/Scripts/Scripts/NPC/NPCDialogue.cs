using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCDialogue : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image npcAvatar;
    [SerializeField] private TMP_InputField playerInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button closeButton;

    [Header("NPC Components")]
    [SerializeField] private NPCMemory npcMemory;
    [SerializeField] private NPCBrain npcBrain;

    [Header("NPC Info (Fallback)")]
    [SerializeField] private string fallbackName = "Невідомий";
    [SerializeField] private Sprite fallbackAvatar;

    private void Start()
    {
        dialoguePanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(EndDialogue);

        if (sendButton != null)
            sendButton.onClick.AddListener(SendPlayerMessage);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        dialoguePanel.SetActive(true);

        // Ім’я з пам’яті або fallback
        string npcName = npcMemory != null && !string.IsNullOrEmpty(npcMemory.data.npcName)
            ? npcMemory.data.npcName
            : fallbackName;

        nameText.text = npcName;

        // Аватарка (підтягується з пам’яті або запасна)
        if (npcAvatar != null)
        {
            Sprite avatarSprite = npcMemory != null && npcMemory.avatarSprite != null
                ? npcMemory.avatarSprite
                : fallbackAvatar;

            npcAvatar.sprite = avatarSprite;
            npcAvatar.enabled = avatarSprite != null;
        }

        dialogueText.text = $"[{npcName}]: Привіт, солдате. Чим можу допомогти?";
    }

    private void SendPlayerMessage()
    {
        string playerMessage = playerInput.text.Trim();
        if (string.IsNullOrEmpty(playerMessage)) return;

        string npcName = npcMemory != null ? npcMemory.data.npcName : fallbackName;
        string response = npcBrain != null ? npcBrain.Think(playerMessage) : "…";

        dialogueText.text = $"Ти: {playerMessage}\n\n[{npcName}]: {response}";
        playerInput.text = "";
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}
