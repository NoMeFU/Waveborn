using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Collider))]
public class DoctorNPC : MonoBehaviour
{
    [Header("Основна інформація")]
    [SerializeField] private string npcName = "Олівія";
    [SerializeField] private Sprite npcAvatar;
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private int healCost = 50;
    [SerializeField] private float healAmount = 9999f;

    [Header("UI елементи")]
    [SerializeField] private GameObject doctorCanvas;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private Button healButton;
    [SerializeField] private Button exitButton;

    [Header("Параметри діалогу")]
    [SerializeField] private float textSpeed = 0.02f;

    private PlayerWallet wallet;
    private Health playerHealth;

    private bool isPlayerNearby = false;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private int currentLine = 0;
    private Coroutine typingCoroutine;

    private void Start()
    {
        wallet = FindObjectOfType<PlayerWallet>();
        playerHealth = FindObjectOfType<Health>();

        if (doctorCanvas) doctorCanvas.SetActive(false);
        if (promptText) promptText.gameObject.SetActive(false);

        if (healButton) healButton.onClick.AddListener(OnHealButtonClicked);
        if (exitButton) exitButton.onClick.AddListener(CloseDialogue);
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (!isDialogueActive)
                OpenDialogue();
            else
                NextDialogueLine();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (promptText)
            {
                promptText.gameObject.SetActive(true);
                promptText.text = "Натисни [E], щоб поговорити з Олівією";
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            CloseDialogue();
            if (promptText) promptText.gameObject.SetActive(false);
        }
    }

    private void OpenDialogue()
    {
        if (dialogueData == null || dialogueData.Count == 0)
        {
            Debug.LogWarning("[DoctorNPC] DialogueData не заданий або порожній!");
            return;
        }

        isDialogueActive = true;
        currentLine = 0;

        if (doctorCanvas) doctorCanvas.SetActive(true);
        if (promptText) promptText.gameObject.SetActive(false);

        if (avatarImage && npcAvatar)
            avatarImage.sprite = npcAvatar;
        if (nameText)
            nameText.text = npcName;

        // 🟢 одразу активуємо кнопки
        if (healButton) healButton.gameObject.SetActive(true);
        if (exitButton) exitButton.gameObject.SetActive(true);

        // Додаємо динамічний рядок з вартістю і кількістю лікування
        string firstLine = dialogueData.GetLine(0)
            .Replace("{cost}", healCost.ToString())
            .Replace("{heal}", healAmount.ToString());

        ShowLine(firstLine);
    }

    private void NextDialogueLine()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = dialogueData.GetLine(currentLine);
            isTyping = false;
            return;
        }

        currentLine++;

        if (currentLine < dialogueData.Count)
        {
            string line = dialogueData.GetLine(currentLine)
                .Replace("{cost}", healCost.ToString())
                .Replace("{heal}", healAmount.ToString());
            ShowLine(line);
        }
        else
        {
            dialogueText.text = $"Хочеш, я тебе підлікую за {healCost} монет? 💊";
        }
    }

    private void ShowLine(string text)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(text));
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    private void OnHealButtonClicked()
    {
        if (wallet == null || playerHealth == null) return;

        if (wallet.Coins >= healCost)
        {
            wallet.SpendCoins(healCost);
            playerHealth.Heal(healAmount);
            dialogueText.text = "Ти виглядаєш набагато краще! Бережи себе ❤️";
        }
        else
        {
            dialogueText.text = $"Ой, здається в тебе не вистачає {healCost} монет...";
        }

        if (healButton) healButton.gameObject.SetActive(false);
    }

    private void CloseDialogue()
    {
        isDialogueActive = false;
        if (doctorCanvas) doctorCanvas.SetActive(false);
        if (healButton) healButton.gameObject.SetActive(false);
        if (exitButton) exitButton.gameObject.SetActive(false);
    }
}
