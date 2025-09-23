using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("Panels (GameObjects)")]
    [SerializeField] private GameObject menuRoot;      // панель з кнопками
    [SerializeField] private GameObject dialogueRoot;  // панель з текстом

    [Header("Menu Buttons")]
    [SerializeField] private Button whatButton;        // "Що тут відбувається?"
    [SerializeField] private Button exitButton;        // "Вийти"

    [Header("Dialogue View")]
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private Button nextButton;        // "Далі"
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;

    [Header("Data")]
    [SerializeField] private DialogueData whatDialogue;

    private DialogueData current;
    private int index = -1;
    private bool isOpen;

    void Awake()
    {
        HideAllImmediate();

        // автопідписка
        if (whatButton) whatButton.onClick.AddListener(OpenWhat);
        if (exitButton) exitButton.onClick.AddListener(HideAllImmediate);
        if (nextButton) nextButton.onClick.AddListener(Advance);
    }

    void Update()
    {
        if (!isOpen) return;
        if (dialogueRoot && dialogueRoot.activeSelf && Input.GetKeyDown(advanceKey))
            Advance();
    }

    // ==== API для NPC ====
    public void ShowMenu()
    {
        isOpen = true;
        SafeSetActive(menuRoot, true);
        SafeSetActive(dialogueRoot, false);
        if (textField) textField.gameObject.SetActive(false);
        Debug.Log("[DialogueUI] ShowMenu()");
    }

    public void HideAllImmediate()
    {
        isOpen = false;
        SafeSetActive(menuRoot, false);
        SafeSetActive(dialogueRoot, false);
        if (textField)
        {
            textField.text = "";
            textField.gameObject.SetActive(false);
            // на всяк випадок – вмикаємо компонент, якщо хтось вимкнув
            textField.enabled = true;
        }
        current = null;
        index = -1;
        Debug.Log("[DialogueUI] HideAllImmediate()");
    }

    // ==== Кнопка "Що тут відбувається?" ====
    public void OpenWhat()
    {
        if (!whatDialogue) { Debug.LogWarning("[DialogueUI] whatDialogue не призначений!"); return; }
        if (whatDialogue.Count == 0) { Debug.LogWarning("[DialogueUI] whatDialogue порожній."); return; }
        StartDialogue(whatDialogue);
    }

    // ==== Діалог ====
    private void StartDialogue(DialogueData data)
    {
        current = data;
        index = -1;

        SafeSetActive(menuRoot, false);
        SafeSetActive(dialogueRoot, true);

        // ⚡️ насильно вмикаємо сам текстовий об'єкт
        if (textField)
        {
            textField.gameObject.SetActive(true);
            textField.enabled = true; // на випадок, якщо вимкнений компонент
            // і колір не прозорий
            var c = textField.color; c.a = 1f; textField.color = c;
        }

        Debug.Log($"[DialogueUI] StartDialogue(): lines={current.Count}, dialogueRoot.active={dialogueRoot.activeSelf}, textActive={textField?.gameObject.activeSelf}");
        Advance();
    }

    private void Advance()
    {
        if (current == null) return;

        index++;
        if (index >= current.Count)
        {
            SafeSetActive(dialogueRoot, false);
            if (textField) textField.gameObject.SetActive(false);
            SafeSetActive(menuRoot, true);
            current = null;
            index = -1;
            Debug.Log("[DialogueUI] Dialogue finished → back to menu");
            return;
        }

        if (!textField) { Debug.LogWarning("[DialogueUI] TextField не призначений!"); return; }

        textField.text = current.GetLine(index);
        Debug.Log($"[DialogueUI] Line {index + 1}/{current.Count}");
    }

    // ніколи не вимикаємо Canvas помилково
    private void SafeSetActive(GameObject go, bool on)
    {
        if (!go) return;
        if (go.GetComponent<Canvas>() != null)
        {
            Debug.LogWarning("[DialogueUI] Не вимикай Canvas — признач дочірню панель (MenuRoot/DialogueRoot).", go);
            return;
        }
        go.SetActive(on);
    }
}
