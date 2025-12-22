using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;

    [Header("Hover Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.85f, 0.85f, 1f);

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    [Header("Scenes")]
    [SerializeField] private string pcSceneName;
    [SerializeField] private string mobileSceneName;

    private void Awake()
    {
        // Button click listeners
        if (playButton != null)
            playButton.onClick.AddListener(() => ButtonClick(playButton, PlayGame));

        if (exitButton != null)
            exitButton.onClick.AddListener(() => ButtonClick(exitButton, ExitGame));

        // Add hover handlers
        AddHover(playButton);
        AddHover(exitButton);
    }

    // -------------------------------
    // CLICK HANDLING + SOUND
    // -------------------------------
    private void ButtonClick(Button btn, System.Action action)
    {
        if (audioSource && clickSound)
            audioSource.PlayOneShot(clickSound);

        action?.Invoke();
    }

    // -------------------------------
    // HOVER HANDLING
    // -------------------------------
    private void AddHover(Button btn)
    {
        EventTrigger trigger = btn.gameObject.AddComponent<EventTrigger>();

        // Pointer Enter
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) =>
        {
            btn.image.color = hoverColor;

            if (audioSource && hoverSound)
                audioSource.PlayOneShot(hoverSound);
        });
        trigger.triggers.Add(entryEnter);

        // Pointer Exit
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) =>
        {
            btn.image.color = normalColor;
        });
        trigger.triggers.Add(entryExit);
    }

    // -------------------------------
    // LOAD GAME WITH LOADING SCENE
    // -------------------------------
    private void PlayGame()
    {
        string targetScene;

#if UNITY_ANDROID || UNITY_IOS
        targetScene = mobileSceneName;
#else
        targetScene = pcSceneName;
#endif

        // Передаємо назву сцени в LoadingManager
        LoadingManager.nextScene = targetScene;

        // Відкриваємо екран завантаження
        SceneManager.LoadScene("Loading");
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        Debug.Log("Exit Game (Editor)");
#else
        Application.Quit();
#endif
    }
}
