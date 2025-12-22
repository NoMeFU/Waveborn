using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Collider))]
public class WaveStarterTrigger : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameModeSurvival gameMode;
    [SerializeField] private TextMeshProUGUI interactHint; // невеликий TMP-текст «Натисни E, щоб почати хвилю N»
    [SerializeField] private string hintFormat = "Натисни <b>E</b>, щоб почати хвилю {0}";

    [Header("Mobile UI")]
    [SerializeField] private Button mobileStartButton;     // Кнопка для мобільних пристроїв
    [SerializeField] private TextMeshProUGUI buttonText;   // Текст на кнопці
    [SerializeField] private string buttonTextFormat = "Почати хвилю {0}";
    [SerializeField] private bool showMobileButton = true; // Показувати кнопку на мобільних

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool _playerInside = false;
    private bool _isMobile = false;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        if (!gameMode) gameMode = FindObjectOfType<GameModeSurvival>();

        // Визначення платформи
        _isMobile = Application.isMobilePlatform;

#if UNITY_ANDROID || UNITY_IOS
        _isMobile = true;
#endif

        // Якщо кнопка призначена, показуємо її завжди (для тестування)
        if (mobileStartButton != null && showMobileButton)
        {
            _isMobile = true;
        }

        SetHintVisible(false);
        SetMobileButtonVisible(false);

        // Підключення кнопки
        if (mobileStartButton != null)
        {
            mobileStartButton.onClick.RemoveAllListeners();
            mobileStartButton.onClick.AddListener(OnMobileButtonClicked);
        }

        if (gameMode)
            gameMode.OnReadyForNext += OnReadyForNextWave;

        Debug.Log($"WaveStarterTrigger: Mobile mode = {_isMobile}, Button assigned = {mobileStartButton != null}");
    }

    private void OnDestroy()
    {
        if (gameMode)
            gameMode.OnReadyForNext -= OnReadyForNextWave;

        if (mobileStartButton != null)
        {
            mobileStartButton.onClick.RemoveListener(OnMobileButtonClicked);
        }
    }

    private void Update()
    {
        if (!_playerInside || gameMode == null) return;

        if (!gameMode.IsWaveRunning)
        {
            // ПК: клавіша E (працює завжди якщо не мобілка або разом з кнопкою)
            if (Input.GetKeyDown(interactKey))
            {
                StartWave();
            }
        }
        else
        {
            // під час хвилі UI ховаємо
            if (interactHint && interactHint.gameObject.activeSelf)
                SetHintVisible(false);
            if (mobileStartButton && mobileStartButton.gameObject.activeSelf)
                SetMobileButtonVisible(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player entered wave trigger");
        _playerInside = true;

        if (gameMode && !gameMode.IsWaveRunning)
        {
            int nextWave = gameMode.CurrentWave + 1;

            // Показуємо кнопку якщо це мобілка
            if (_isMobile && mobileStartButton != null)
            {
                SetButtonTextForWave(nextWave);
                SetMobileButtonVisible(true);
                Debug.Log($"Mobile button shown for wave {nextWave}");
            }

            // Показуємо підказку для ПК
            if (interactHint != null)
            {
                SetHintTextForWave(nextWave);
                SetHintVisible(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player exited wave trigger");
        _playerInside = false;
        SetHintVisible(false);
        SetMobileButtonVisible(false);
    }

    private void OnReadyForNextWave(int nextWaveIndex)
    {
        Debug.Log($"Ready for next wave: {nextWaveIndex}, Player inside: {_playerInside}");

        if (_playerInside && gameMode && !gameMode.IsWaveRunning)
        {
            if (_isMobile && mobileStartButton != null)
            {
                SetButtonTextForWave(nextWaveIndex);
                SetMobileButtonVisible(true);
            }

            if (interactHint != null)
            {
                SetHintTextForWave(nextWaveIndex);
                SetHintVisible(true);
            }
        }
    }

    private void OnMobileButtonClicked()
    {
        Debug.Log("Mobile button clicked");
        if (_playerInside && gameMode && !gameMode.IsWaveRunning)
        {
            StartWave();
        }
    }

    private void StartWave()
    {
        Debug.Log("Starting wave");
        if (gameMode != null)
        {
            gameMode.RequestStartNextWave();
            SetHintVisible(false);
            SetMobileButtonVisible(false);
        }
    }

    private void SetHintTextForWave(int waveIndex)
    {
        if (!interactHint) return;
        interactHint.text = string.Format(hintFormat, Mathf.Max(1, waveIndex));
    }

    private void SetButtonTextForWave(int waveIndex)
    {
        if (!buttonText) return;
        buttonText.text = string.Format(buttonTextFormat, Mathf.Max(1, waveIndex));
    }

    private void SetHintVisible(bool on)
    {
        if (interactHint)
        {
            interactHint.gameObject.SetActive(on);
            Debug.Log($"Hint visibility: {on}");
        }
    }

    private void SetMobileButtonVisible(bool on)
    {
        if (mobileStartButton)
        {
            mobileStartButton.gameObject.SetActive(on);
            Debug.Log($"Mobile button visibility: {on}");
        }
    }
}