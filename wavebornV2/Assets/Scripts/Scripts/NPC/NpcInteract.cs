using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class NpcInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string hintText = "Натисни <b>E</b>, щоб поговорити";
    [SerializeField] private TextMeshProUGUI interactHint;

    [Header("UI Settings")]
    [Tooltip("Перетягни сюди Canvas або панель, яку потрібно вмикати при взаємодії")]
    [SerializeField] private GameObject uiToActivate;

    private bool _inside;
    private bool _uiWasActive;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        SetHint(false);
        if (uiToActivate) uiToActivate.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _inside = true;

        if (!DialogueUI.IsAnyOpen() && interactHint)
        {
            interactHint.text = hintText;
            SetHint(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _inside = false;
        SetHint(false);

        if (uiToActivate)
        {
            uiToActivate.SetActive(false);
            _uiWasActive = false;
        }
    }

    private void Update()
    {
        if (!_inside || uiToActivate == null) return;

        if (Input.GetKeyDown(interactKey))
        {
            bool isActive = uiToActivate.activeSelf;

            // Закриваємо якщо вже було відкрите
            if (isActive)
            {
                uiToActivate.SetActive(false);
                _uiWasActive = false;
            }
            else
            {
                uiToActivate.SetActive(true);
                _uiWasActive = true;
            }

            SetHint(false);
        }
    }

    private void SetHint(bool on)
    {
        if (interactHint)
            interactHint.gameObject.SetActive(on);
    }
}
