using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(SphereCollider))]
public class NPCDialogue : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField, Range(0.5f, 10f)] private float interactRadius = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactionHint; // UI підказка "Натисни E" (для ПК)
    [SerializeField] private GameObject mobileInteractButton; // Кнопка для мобільних

    [Header("UI Elements (specific to this NPC)")]
    [Tooltip("Panel / Canvas which will be shown for this NPC")]
    [SerializeField] private GameObject dialoguePanel; // яке саме меню відкривати
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image npcAvatar;
    [SerializeField] private TMP_InputField playerInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button closeButton;

    [Header("NPC Data & AI")]
    [SerializeField] private NPCMemory npcMemory;
    [SerializeField] private NPCBrain npcBrain;

    [Header("Fallback info")]
    [SerializeField] private string fallbackName = "Невідомий";
    [SerializeField] private Sprite fallbackAvatar;

    // state
    private bool playerInRange = false;
    private bool isOpen = false;
    private bool isMobile = false;

    // references
    private Transform player;
    private SphereCollider sphereCollider;

    #region Unity lifecycle
    private void Awake()
    {
        // Визначаємо чи це мобільний пристрій
        isMobile = Application.isMobilePlatform;

#if UNITY_ANDROID || UNITY_IOS
            isMobile = true;
#endif

        // ensure we have a SphereCollider we can control
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
            sphereCollider = gameObject.AddComponent<SphereCollider>();

        sphereCollider.isTrigger = true;
        sphereCollider.radius = interactRadius;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (dialoguePanel)
            dialoguePanel.SetActive(false);

        if (interactionHint)
            interactionHint.SetActive(false);

        if (mobileInteractButton)
            mobileInteractButton.SetActive(false);

        if (closeButton)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(EndDialogue);
        }

        if (sendButton)
        {
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(SendPlayerMessage);
        }

        // Підключаємо мобільну кнопку взаємодії
        if (mobileInteractButton && isMobile)
        {
            var button = mobileInteractButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnMobileInteractClick);
            }
        }
    }

    private void OnValidate()
    {
        // keep collider radius in sync in editor when changed
        if (sphereCollider == null)
            sphereCollider = GetComponent<SphereCollider>();

        if (sphereCollider != null)
            sphereCollider.radius = interactRadius;
    }

    private void Update()
    {
        // only allow interaction if player is in range (тільки для ПК)
        if (playerInRange && !isMobile && Input.GetKeyDown(interactKey))
        {
            if (!isOpen)
                StartDialogue();
            else
                EndDialogue();
        }
    }
    #endregion

    #region Trigger detection
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;

        if (!isOpen)
            ShowInteractionUI(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;

        ShowInteractionUI(false);

        // close dialogue if player leaves
        EndDialogue();
    }
    #endregion

    #region UI Management
    /// <summary>
    /// Показує/ховає UI взаємодії в залежності від платформи
    /// </summary>
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
            // На ПК показуємо текст-підказку
            if (interactionHint)
                interactionHint.SetActive(show);
        }
    }

    /// <summary>
    /// Обробник кнопки для мобільних
    /// </summary>
    private void OnMobileInteractClick()
    {
        if (!playerInRange) return;

        if (!isOpen)
            StartDialogue();
        else
            EndDialogue();
    }
    #endregion

    #region Dialogue API
    /// <summary>
    /// Open this NPC's dialogue UI (can be called from NpcInteract or other systems)
    /// </summary>
    public void StartDialogue()
    {
        if (dialoguePanel == null || isOpen) return;

        // open UI
        dialoguePanel.SetActive(true);
        isOpen = true;

        // hide hint while open
        ShowInteractionUI(false);

        // fill name and avatar
        string npcName = (npcMemory != null && !string.IsNullOrEmpty(npcMemory.data.npcName))
            ? npcMemory.data.npcName
            : fallbackName;

        if (nameText != null)
            nameText.text = npcName;

        if (npcAvatar != null)
        {
            Sprite avatarSprite = (npcMemory != null && npcMemory.avatarSprite != null)
                ? npcMemory.avatarSprite
                : fallbackAvatar;

            npcAvatar.sprite = avatarSprite;
            npcAvatar.enabled = avatarSprite != null;
        }

        if (dialogueText != null)
            dialogueText.text = $"[{npcName}]: Привіт, солдате. Чим можу допомогти?";

        if (playerInput != null)
            playerInput.text = "";
    }

    /// <summary>
    /// Close this NPC's dialogue UI
    /// </summary>
    public void EndDialogue()
    {
        if (!isOpen) return;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        isOpen = false;

        if (playerInput != null)
            playerInput.text = "";

        // show hint again if player still inside range
        if (playerInRange)
            ShowInteractionUI(true);
    }

    public bool IsOpen => isOpen;
    public bool PlayerInRange => playerInRange;
    #endregion

    #region Player messaging (uses NPCBrain)
    private void SendPlayerMessage()
    {
        if (!isOpen || dialogueText == null || playerInput == null)
            return;

        string playerMessage = playerInput.text.Trim();
        if (string.IsNullOrEmpty(playerMessage)) return;

        string npcName = (npcMemory != null) ? npcMemory.data.npcName : fallbackName;
        string response = (npcBrain != null) ? npcBrain.Think(playerMessage) : "…";

        dialogueText.text = $"Ти: {playerMessage}\n\n[{npcName}]: {response}";
        playerInput.text = "";
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
    #endregion
}