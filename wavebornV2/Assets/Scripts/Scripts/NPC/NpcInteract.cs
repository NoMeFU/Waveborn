using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class NpcInteract : MonoBehaviour
{
    [SerializeField] private GameObject interactHint; // можна дати Canvas або сам текст
    [SerializeField] private DialogueUI dialogueUI;

    private bool playerInside;
    private GameObject resolvedHint; // те, що реально будемо вмикати/вимикати

    void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void Awake()
    {
        // якщо дали Canvas — шукаємо в ньому перший TMP-текст із назвою "InteractHint"
        resolvedHint = ResolveHint(interactHint);
        if (!resolvedHint)
            Debug.LogWarning("[NpcInteract] InteractHint не знайдено. Створи окремий UI-текст і признач у поле.");
        else
            resolvedHint.SetActive(false);
    }

    private GameObject ResolveHint(GameObject go)
    {
        if (!go) return null;
        if (go.GetComponent<Canvas>() != null)
        {
            // 1) спробуємо знайти дочірній об’єкт із назвою "InteractHint"
            var t = go.transform.Find("InteractHint");
            if (t) return t.gameObject;
            // 2) або перший TextMeshProUGUI
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp) return tmp.gameObject;
            return null;
        }
        return go;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        if (resolvedHint) resolvedHint.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        if (resolvedHint) resolvedHint.SetActive(false);
        dialogueUI?.HideAllImmediate();
    }

    void Update()
    {
        if (!playerInside) return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (resolvedHint) resolvedHint.SetActive(false);
            dialogueUI?.ShowMenu();
        }
    }
}
