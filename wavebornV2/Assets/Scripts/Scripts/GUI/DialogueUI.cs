// Assets/Scripts/NPC/DialogueUI.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuRoot;     // панель із кнопками (What/Tasks/Exit)
    [SerializeField] private GameObject dialogueRoot; // панель з текстом і кнопкою "Далі"

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

    private void Awake()
    {
        CloseAll(); // сховати все і зняти блок, якщо був
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

    // ===== API =====
    public void OpenMenu()
    {
        if (!_pushed) { InputBlocker.Push(); _pushed = true; }
        IsOpen = true;
        SafeSet(menuRoot, true);
        SafeSet(dialogueRoot, false);
        if (textField) textField.gameObject.SetActive(false);
    }

    public void ShowDialogueText(string message)
    {
        if (!_pushed) { InputBlocker.Push(); _pushed = true; }
        IsOpen = true;

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

    public void CloseAll()
    {
        SafeSet(menuRoot, false);
        SafeSet(dialogueRoot, false);
        if (textField) { textField.text = ""; textField.gameObject.SetActive(false); }
        _current = null;
        _index = -1;
        IsOpen = false;

        if (_pushed) { InputBlocker.Pop(); _pushed = false; }
    }

    // ===== Вбудований діалог "Що тут відбувається?" =====
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
            // закінчили – назад у меню
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
        if (go.GetComponent<Canvas>()) return; // не вимикати весь Canvas
        go.SetActive(on);
    }
}
