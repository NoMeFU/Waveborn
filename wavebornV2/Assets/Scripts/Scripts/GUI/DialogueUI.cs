using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private GameObject dialogueRoot;

    [Header("Menu Buttons")]
    [SerializeField] private Button whatButton;
    [SerializeField] private Button exitButton;

    [Header("Dialogue View")]
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private Button nextButton;
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;

    [Header("Dialogue Data")]
    [SerializeField] private DialogueData dialogueData; // ScriptableObject
    [SerializeField] private string[] fallbackLines; // Запасний варіант

    private Action onDialogueEnd;
    public bool IsOpen => (menuRoot != null && menuRoot.activeSelf) || (dialogueRoot != null && dialogueRoot.activeSelf);

    private string[] _currentDialogue;
    private int _index = -1;
    private bool _pushedInputBlock = false;

    private static readonly System.Collections.Generic.HashSet<DialogueUI> _openUIs = new();

    private void Awake()
    {
        SafeSet(menuRoot, false);
        SafeSet(dialogueRoot, false);

        if (whatButton)
        {
            whatButton.onClick.AddListener(OpenWhat);
            Debug.Log($"[DialogueUI] What button listener додано");
        }

        if (exitButton)
        {
            exitButton.onClick.AddListener(CloseAll);
            Debug.Log($"[DialogueUI] Exit button listener додано");
        }

        if (nextButton)
        {
            nextButton.onClick.AddListener(Advance);
            Debug.Log($"[DialogueUI] Next button listener додано");
        }

        // Перевірка даних діалогу
        if (dialogueData != null && dialogueData.Count > 0)
        {
            Debug.Log($"[DialogueUI] DialogueData завантажено: {dialogueData.Count} рядків");
        }
        else if (fallbackLines != null && fallbackLines.Length > 0)
        {
            Debug.Log($"[DialogueUI] Використовую fallbackLines: {fallbackLines.Length} рядків");
        }
        else
        {
            Debug.LogWarning($"[DialogueUI] Діалог не налаштовано! Додайте DialogueData або fallbackLines.");
        }
    }

    private void Update()
    {
        if (!IsOpen) return;

        if (dialogueRoot != null && dialogueRoot.activeSelf && Input.GetKeyDown(advanceKey))
            Advance();
    }

    public void OpenMenu()
    {
        Debug.Log($"[DialogueUI] OpenMenu() викликано");

        if (!_pushedInputBlock) { InputBlocker.Push(); _pushedInputBlock = true; }

        _openUIs.Add(this);

        SafeSet(menuRoot, true);
        SafeSet(dialogueRoot, false);

        if (textField) textField.gameObject.SetActive(false);
    }

    public void CloseAll()
    {
        Debug.Log($"[DialogueUI] CloseAll() викликано");

        SafeSet(menuRoot, false);
        SafeSet(dialogueRoot, false);

        if (textField)
        {
            textField.text = "";
            textField.gameObject.SetActive(false);
        }

        _currentDialogue = null;
        _index = -1;
        onDialogueEnd?.Invoke();
        onDialogueEnd = null;

        _openUIs.Remove(this);
        if (_pushedInputBlock) { InputBlocker.Pop(); _pushedInputBlock = false; }
    }

    private void OpenWhat()
    {
        Debug.Log($"[DialogueUI] OpenWhat() викликано");

        // Пріоритет: DialogueData -> fallbackLines -> дефолтний текст
        string[] linesToUse = null;

        if (dialogueData != null && dialogueData.Count > 0)
        {
            linesToUse = dialogueData.lines;
            Debug.Log($"[DialogueUI] Використовую DialogueData: {dialogueData.Count} рядків");
        }
        else if (fallbackLines != null && fallbackLines.Length > 0)
        {
            linesToUse = fallbackLines;
            Debug.Log($"[DialogueUI] Використовую fallbackLines: {fallbackLines.Length} рядків");
        }
        else
        {
            Debug.LogWarning($"[DialogueUI] Діалог не налаштовано! Використовую дефолтний текст.");
            linesToUse = new string[]
            {
                "Привіт! Я NPC.",
                "Діалог не налаштовано.",
                "Додайте DialogueData в Inspector!"
            };
        }

        StartDialogue(linesToUse, null);
    }

    public void StartDialogue(string[] lines, Action onEnd = null)
    {
        Debug.Log($"[DialogueUI] StartDialogue() викликано. Рядків: {(lines != null ? lines.Length : 0)}");

        onDialogueEnd = onEnd;
        if (!_pushedInputBlock) { InputBlocker.Push(); _pushedInputBlock = true; }
        _openUIs.Add(this);

        _currentDialogue = lines;
        _index = -1;

        SafeSet(menuRoot, false);
        SafeSet(dialogueRoot, true);

        if (textField) { textField.gameObject.SetActive(true); textField.enabled = true; }

        if (nextButton) nextButton.gameObject.SetActive(true);
        Advance();
    }

    // Overload для DialogueData
    public void StartDialogue(DialogueData data, Action onEnd = null)
    {
        if (data != null && data.Count > 0)
        {
            StartDialogue(data.lines, onEnd);
        }
        else
        {
            Debug.LogWarning($"[DialogueUI] DialogueData порожній або null!");
        }
    }

    public void StartDialogue(Action onEnd = null)
    {
        if (dialogueData != null && dialogueData.Count > 0)
            StartDialogue(dialogueData.lines, onEnd);
        else
            StartDialogue(fallbackLines, onEnd);
    }

    private void Advance()
    {
        if (_currentDialogue == null || _currentDialogue.Length == 0)
        {
            EndDialogueImmediately();
            return;
        }

        _index++;
        if (_index >= _currentDialogue.Length)
        {
            // Повертаємось до меню після останнього рядка
            SafeSet(dialogueRoot, false);
            if (textField) textField.gameObject.SetActive(false);
            SafeSet(menuRoot, true);

            _currentDialogue = null;
            _index = -1;

            onDialogueEnd?.Invoke();
            onDialogueEnd = null;

            return;
        }

        if (textField)
        {
            textField.text = _currentDialogue[_index];
            Debug.Log($"[DialogueUI] Показую рядок {_index + 1}/{_currentDialogue.Length}: {_currentDialogue[_index]}");
        }
    }

    private void EndDialogueImmediately()
    {
        SafeSet(dialogueRoot, false);
        if (textField) textField.gameObject.SetActive(false);
        SafeSet(menuRoot, true);

        _currentDialogue = null;
        _index = -1;

        onDialogueEnd?.Invoke();
        onDialogueEnd = null;

        _openUIs.Remove(this);
        if (_pushedInputBlock) { InputBlocker.Pop(); _pushedInputBlock = false; }
    }

    public void ShowDialogueText(string message)
    {
        if (!_pushedInputBlock) { InputBlocker.Push(); _pushedInputBlock = true; }
        _openUIs.Add(this);

        SafeSet(menuRoot, false);
        SafeSet(dialogueRoot, true);

        if (textField)
        {
            textField.gameObject.SetActive(true);
            textField.enabled = true;
            textField.text = message;
        }

        if (nextButton) nextButton.gameObject.SetActive(false);
    }

    private static void SafeSet(GameObject go, bool on)
    {
        if (!go) return;
        go.SetActive(on);
    }

    public static bool IsAnyOpen() => _openUIs.Count > 0;
}