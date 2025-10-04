using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class QuestLogUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup canvasGroup;   // CanvasGroup на QuestLogPanel
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private float fadeSpeed = 12f;

    [Header("List")]
    [SerializeField] private RectTransform content;     // Scroll View → Viewport → Content
    [SerializeField] private QuestItemUI itemPrefab;    // Префаб елемента

    private bool _visible;
    private readonly Dictionary<string, QuestItemUI> _items = new();

    private void Reset()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        SetVisible(false, true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            SetVisible(!_visible);

        float targetA = _visible ? 1f : 0f;
        if (!Mathf.Approximately(canvasGroup.alpha, targetA))
        {
            canvasGroup.alpha = Mathf.MoveTowards(
                canvasGroup.alpha, targetA, fadeSpeed * Time.unscaledDeltaTime);
            bool on = canvasGroup.alpha > 0.001f;
            canvasGroup.interactable = on;
            canvasGroup.blocksRaycasts = on;
        }
    }

    private void SetVisible(bool on, bool instant = false)
    {
        _visible = on;
        if (instant)
        {
            canvasGroup.alpha = on ? 1f : 0f;
            canvasGroup.interactable = on;
            canvasGroup.blocksRaycasts = on;
        }
    }

    /// <summary>Додати або оновити квест.</summary>
    public void AddOrUpdate(string id, string title, string description, int current, int max)
    {
        if (string.IsNullOrEmpty(id) || !itemPrefab || !content) return;

        if (!_items.TryGetValue(id, out var ui))
        {
            ui = Instantiate(itemPrefab, content);
            _items[id] = ui;
        }

        ui.Set(title, description, current, max); // ✅ викликаємо Set, а не Bind
    }

    /// <summary>Прибрати квест із журналу.</summary>
    public void Remove(string id)
    {
        if (_items.TryGetValue(id, out var ui))
        {
            Destroy(ui.gameObject);
            _items.Remove(id);
        }
    }
}
