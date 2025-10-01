using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class WaveStarterTrigger : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameModeSurvival gameMode;
    [SerializeField] private TextMeshProUGUI interactHint; // невеликий TMP-текст «Натисни E, щоб почати хвилю N»
    [SerializeField] private string hintFormat = "Натисни <b>E</b>, щоб почати хвилю {0}";

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool _playerInside = false;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (!gameMode) gameMode = FindObjectOfType<GameModeSurvival>();
        SetHintVisible(false);

        if (gameMode)
            gameMode.OnReadyForNext += OnReadyForNextWave;
    }

    private void OnDestroy()
    {
        if (gameMode)
            gameMode.OnReadyForNext -= OnReadyForNextWave;
    }

    private void Update()
    {
        if (!_playerInside || gameMode == null) return;

        if (!gameMode.IsWaveRunning)
        {
            if (Input.GetKeyDown(interactKey))
            {
                gameMode.RequestStartNextWave();
                SetHintVisible(false);
            }
        }
        else
        {
            // під час хвилі підказку ховаємо
            if (interactHint && interactHint.gameObject.activeSelf)
                SetHintVisible(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = true;

        if (gameMode && !gameMode.IsWaveRunning)
        {
            SetHintTextForWave(gameMode.CurrentWave + 1);
            SetHintVisible(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
        SetHintVisible(false);
    }

    private void OnReadyForNextWave(int nextWaveIndex)
    {
        if (_playerInside && gameMode && !gameMode.IsWaveRunning)
        {
            SetHintTextForWave(nextWaveIndex);
            SetHintVisible(true);
        }
    }

    private void SetHintTextForWave(int waveIndex)
    {
        if (!interactHint) return;
        interactHint.text = string.Format(hintFormat, Mathf.Max(1, waveIndex));
    }

    private void SetHintVisible(bool on)
    {
        if (interactHint) interactHint.gameObject.SetActive(on);
    }
}
