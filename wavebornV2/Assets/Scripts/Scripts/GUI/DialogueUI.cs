using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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

    [Header("Data")]
    [SerializeField] private DialogueData whatDialogue;

    public bool IsOpen { get; private set; }

    private DialogueData _current;
    private int _index = -1;
    private bool _pushed;

    private static readonly HashSet<DialogueUI> _openUIs = new(); // 🔹 слідкуємо за активними меню

    private void Awake()
    {
        CloseAll();

        if (whatButton) whatButton.onClick.AddListener(OpenWhat);
        if (exitButton) exitButton.onClick.AddListener(CloseAll);
        if (nextButton) nextButton.onClick.AddListener(Advance);
    }

    private void Update()
    {
        if (!IsOpen) return;
        if (dialogueRoot && dialogueRoot.activeSelf && Input.GetKeyDown(advanceKey))
            Advance();
    }

    public void OpenMenu()
    {
        if (!_pushed) { InputBlocker.Push(); _pushed = true; }

        IsOpen = true;
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

        _current = null;
        _index = -1;
        IsOpen = false;
        _openUIs.Remove(this);

        if (_pushed) { InputBlocker.Pop(); _pushed = false; }
    }

    private void OpenWhat()
    {
        if (whatDialogue == null || whatDialogue.Count == 0) return;
        StartDialogue(whatDialogue);
    }

    private void StartDialogue(DialogueData data)
    {
        if (!_pushed) { InputBlocker.Push(); _pushed = true; }
        IsOpen = true;

        _current = data;
        _index = -1;

        SafeSet(menuRoot, false);
        SafeSet(dialogueRoot, true);
        if (textField) { textField.gameObject.SetActive(true); textField.enabled = true; }

        if (nextButton) nextButton.gameObject.SetActive(true);
        Advance();
    }

    private void Advance()
    {
        if (_current == null) return;

        _index++;
        if (_index >= _current.Count)
        {
            SafeSet(dialogueRoot, false);
            if (textField) textField.gameObject.SetActive(false);
            SafeSet(menuRoot, true);
            _current = null;
            _index = -1;
            return;
        }
        if (textField) textField.text = _current.GetLine(_index);
    }

    private static void SafeSet(GameObject go, bool on)
    {
        if (!go) return;
        if (go.GetComponent<Canvas>()) return;
        go.SetActive(on);
    }

    public static bool IsAnyOpen() => _openUIs.Count > 0;
    public void ShowDialogueText(string message)
    {
        if (!_pushed) { InputBlocker.Push(); _pushed = true; }
        IsOpen = true;
        _openUIs.Add(this);

        SafeSet(menuRoot, false);
        SafeSet(dialogueRoot, true);

        if (textField)
        {
            textField.gameObject.SetActive(true);
            textField.enabled = true;
            textField.text = message;
        }

        if (nextButton) nextButton.gameObject.SetActive(false); // одноразове повідомлення
    }
}
