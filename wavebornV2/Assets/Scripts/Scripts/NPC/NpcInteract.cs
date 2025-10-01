// Assets/Scripts/NPC/NpcInteract.cs
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class NpcInteract : MonoBehaviour
{
    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private TextMeshProUGUI interactHint;
    [SerializeField] private string hintText = "Натисни <b>E</b>, щоб поговорити";
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool _inside;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        if (!dialogueUI) dialogueUI = FindObjectOfType<DialogueUI>();
        SetHint(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _inside = true;
        if (dialogueUI && !dialogueUI.IsOpen)
        {
            if (interactHint) interactHint.text = hintText;
            SetHint(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _inside = false;
        SetHint(false);
        // бажаєш – закривай діалог при виході:
        // if (dialogueUI && dialogueUI.IsOpen) dialogueUI.CloseAll();
    }

    private void Update()
    {
        if (!_inside || !dialogueUI) return;
        if (Input.GetKeyDown(interactKey))
        {
            dialogueUI.OpenMenu();
            SetHint(false);
        }
    }

    private void SetHint(bool on)
    {
        if (interactHint) interactHint.gameObject.SetActive(on);
    }
}
