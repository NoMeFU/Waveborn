using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject mobilePauseButton; // Кнопка для мобільних

    [Header("Menu Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private string loadingSceneName = "LoadingScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;
    private Canvas pauseCanvas;

    private void Start()
    {
        // Знаходимо Canvas
        if (pauseMenuPanel != null)
        {
            pauseCanvas = pauseMenuPanel.GetComponentInParent<Canvas>();

            // КРИТИЧНО: Canvas повинен ігнорувати паузу
            if (pauseCanvas != null)
            {
                pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                // Додаємо GraphicRaycaster якщо немає
                if (pauseCanvas.GetComponent<GraphicRaycaster>() == null)
                    pauseCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }

            pauseMenuPanel.SetActive(false);
        }

        // Показуємо кнопку паузи тільки на мобільних
        SetupMobileButton();

        // Підключаємо кнопки
        SetupButtons();

        ResumeGame();
    }

    private void SetupButtons()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(LoadMainMenuWithLoading);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    private void SetupMobileButton()
    {
        if (mobilePauseButton == null) return;

        bool isMobile = Application.isMobilePlatform;

#if UNITY_ANDROID || UNITY_IOS
        isMobile = true;
#endif

        mobilePauseButton.SetActive(isMobile);

        // Підключаємо кнопку паузи
        Button pauseBtn = mobilePauseButton.GetComponent<Button>();
        if (pauseBtn != null)
        {
            pauseBtn.onClick.RemoveAllListeners();
            pauseBtn.onClick.AddListener(TogglePause);
        }
    }

    private void Update()
    {
        // Перевірка натискання клавіші паузи
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        // Зупиняємо час
        Time.timeScale = 0f;

        // Показуємо курсор
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Відновлюємо час
        Time.timeScale = 1f;

        // Курсор (налаштуйте під свою гру)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Game Resumed");
    }

    public void LoadMainMenuWithLoading()
    {
        Debug.Log("Loading Main Menu");

        // Відновлюємо час
        Time.timeScale = 1f;

        // Перевіряємо чи існує LoadingManager
        if (System.Type.GetType("LoadingManager") != null)
        {
            LoadingManager.nextScene = mainMenuSceneName;
            SceneManager.LoadScene(loadingSceneName);
        }
        else
        {
            // Якщо немає LoadingManager, завантажуємо напряму
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void LoadMainMenuDirect()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game");
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public bool IsPaused => isPaused;
}