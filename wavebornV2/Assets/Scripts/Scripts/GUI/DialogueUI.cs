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

    [Header("Debug / Data")]
    [SerializeField] private string[] fallbackLines;

    private Action onDialogueEnd;
    public bool IsOpen => (menuRoot != null && menuRoot.activeSelf) || (dialogueRoot != null && dialogueRoot.activeSelf);

    private string[] _currentDialogue;
    private int _index = -1;
    private bool _pushedInputBlock = false;

    private static readonly System.Collections.Generic.HashSet<DialogueUI> _openUIs = new(); // слідкуємо за активними меню

    private void Awake()
    {
        SafeSet(menuRoot, false);
        SafeSet(dialogueRoot, false);

        if (whatButton) whatButton.onClick.AddListener(OpenWhat);
        if (exitButton) exitButton.onClick.AddListener(CloseAll);
        if (nextButton) nextButton.onClick.AddListener(Advance);
    }

    private void Update()
    {
        if (!IsOpen) return;

        if (dialogueRoot != null && dialogueRoot.activeSelf && Input.GetKeyDown(advanceKey))
            Advance();
    }

    public void OpenMenu()
    {
        if (!_pushedInputBlock) { InputBlocker.Push(); _pushedInputBlock = true; }

        _openUIs.Add(this);

        SafeSet(menuRoot, true);
        SafeSet(dialogueRoot, false);

        if (textField) textField.gameObject.SetActive(false);
    }

    public void CloseAll()
    {
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
        StartDialogue(fallbackLines, null);
    }

    public void StartDialogue(string[] lines, Action onEnd = null)
    {
        onDialogueEnd = onEnd;
        if (!_pushedInputBlock) { InputBlocker.Push(); _pushedInputBlock = true; }
        _openUIs.Add(this);

        _currentDialogue = lines ?? fallbackLines;
        _index = -1;

        SafeSet(menuRoot, false);
        SafeSet(dialogueRoot, true);

        if (textField) { textField.gameObject.SetActive(true); textField.enabled = true; }

        if (nextButton) nextButton.gameObject.SetActive(true);
        Advance();
    }

    // Overload used by other scripts (no params) to start a simple dialogue that will call onDialogueEnd when closed
    public void StartDialogue(Action onEnd = null)
    {
        StartDialogue(fallbackLines, onEnd);
    }

    private void Advance()
    {
        if (_currentDialogue == null)
        {
            EndDialogueImmediately();
            return;
        }

        _index++;
        if (_index >= _currentDialogue.Length)
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
            return;
        }

        if (textField) textField.text = _currentDialogue[_index];
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
