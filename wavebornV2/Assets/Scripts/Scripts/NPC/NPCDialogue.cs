using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(SphereCollider))]
public class NPCDialogue : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField, Range(0.5f, 10f)] private float interactRadius = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactionHint; // UI підказка "Натисни E"

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

    // references
    private Transform player;
    private SphereCollider sphereCollider;

    #region Unity lifecycle
    private void Awake()
    {
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
        // only allow interaction if player is in range
        if (playerInRange && Input.GetKeyDown(interactKey))
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
        if (interactionHint != null && !isOpen)
            interactionHint.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (interactionHint != null)
            interactionHint.SetActive(false);

        // close dialogue if player leaves
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
        if (interactionHint != null)
            interactionHint.SetActive(false);

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
        if (interactionHint != null)
            interactionHint.SetActive(playerInRange && !isOpen);
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
}
