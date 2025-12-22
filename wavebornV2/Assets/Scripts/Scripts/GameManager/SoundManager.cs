using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioSource musicSource1;
    [SerializeField] private AudioSource musicSource2;
    [SerializeField] private AudioClip ambientMusic;      // Музика поза хвилями
    [SerializeField] private AudioClip combatMusic;       // Музика під час хвилі
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField, Range(0.1f, 5f)] private float fadeTime = 1.5f; // Час плавного переходу

    [Header("Sound Effects")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.7f;

    // Звуки для різних подій (приклади)
    [Header("SFX Clips")]
    [SerializeField] private AudioClip enemyHitSound;
    [SerializeField] private AudioClip playerHitSound;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip bossWarningSound;
    [SerializeField] private AudioClip waveStartSound;
    [SerializeField] private AudioClip waveClearSound;

    private AudioSource currentMusicSource;
    private AudioSource nextMusicSource;
    private Coroutine fadeCoroutine;
    private bool isInCombat = false;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Створюємо AudioSource якщо їх немає
        if (musicSource1 == null)
        {
            musicSource1 = gameObject.AddComponent<AudioSource>();
            musicSource1.loop = true;
            musicSource1.playOnAwake = false;
        }
        if (musicSource2 == null)
        {
            musicSource2 = gameObject.AddComponent<AudioSource>();
            musicSource2.loop = true;
            musicSource2.playOnAwake = false;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        currentMusicSource = musicSource1;
        nextMusicSource = musicSource2;
    }

    private void Start()
    {
        // Починаємо з ambient музики
        if (ambientMusic != null)
        {
            currentMusicSource.clip = ambientMusic;
            currentMusicSource.volume = musicVolume;
            currentMusicSource.Play();
        }

        // Підписуємось на події GameModeSurvival
        var gameMode = FindObjectOfType<GameModeSurvival>();
        if (gameMode != null)
        {
            gameMode.OnWaveStarted += OnWaveStarted;
            gameMode.OnWaveCleared += OnWaveCleared;
        }
    }

    private void OnDestroy()
    {
        // Відписуємось від подій
        var gameMode = FindObjectOfType<GameModeSurvival>();
        if (gameMode != null)
        {
            gameMode.OnWaveStarted -= OnWaveStarted;
            gameMode.OnWaveCleared -= OnWaveCleared;
        }
    }

    // ===== МУЗИКА =====

    public void PlayAmbientMusic()
    {
        if (isInCombat)
        {
            isInCombat = false;
            SwitchMusic(ambientMusic);
        }
    }

    public void PlayCombatMusic()
    {
        if (!isInCombat)
        {
            isInCombat = true;
            SwitchMusic(combatMusic);
        }
    }

    private void SwitchMusic(AudioClip newClip)
    {
        if (newClip == null) return;

        // Якщо вже грає потрібна музика - нічого не робимо
        if (currentMusicSource.clip == newClip && currentMusicSource.isPlaying)
            return;

        // Зупиняємо попередній crossfade
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(CrossfadeMusic(newClip));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        // Налаштовуємо наступний source
        nextMusicSource.clip = newClip;
        nextMusicSource.volume = 0f;
        nextMusicSource.Play();

        float elapsed = 0f;
        float startVolume = currentMusicSource.volume; // Запам'ятовуємо поточну гучність

        // Плавний перехід
        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime; // Щоб працювало навіть на паузі
            float t = elapsed / fadeTime;

            currentMusicSource.volume = Mathf.Lerp(startVolume, 0f, t);
            nextMusicSource.volume = Mathf.Lerp(0f, musicVolume, t);

            yield return null;
        }

        // Завершуємо перехід
        currentMusicSource.Stop();
        currentMusicSource.volume = 0f; // Скидаємо в 0
        nextMusicSource.volume = musicVolume; // Встановлюємо повну гучність

        // Міняємо місцями джерела
        (currentMusicSource, nextMusicSource) = (nextMusicSource, currentMusicSource);

        fadeCoroutine = null;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (currentMusicSource) currentMusicSource.volume = musicVolume;
    }

    // ===== ЗВУКОВІ ЕФЕКТИ =====

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeMultiplier = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, sfxVolume * volumeMultiplier);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    // ===== ГОТОВІ МЕТОДИ ДЛЯ ЗВУКІВ =====

    public void PlayEnemyHit() => PlaySFX(enemyHitSound);
    public void PlayPlayerHit() => PlaySFX(playerHitSound);
    public void PlayShoot() => PlaySFX(shootSound);
    public void PlayPickup() => PlaySFX(pickupSound);
    public void PlayBossWarning() => PlaySFX(bossWarningSound);
    public void PlayWaveStart() => PlaySFX(waveStartSound);
    public void PlayWaveClear() => PlaySFX(waveClearSound);

    // ===== ОБРОБНИКИ ПОДІЙ ХВИЛЬ =====

    private void OnWaveStarted(int waveNumber)
    {
        PlayCombatMusic();
        PlayWaveStart();
    }

    private void OnWaveCleared(int waveNumber)
    {
        PlayAmbientMusic();
        PlayWaveClear();
    }

    // ===== ДОДАТКОВІ УТИЛІТИ =====

    public void StopAllMusic()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        musicSource1.Stop();
        musicSource2.Stop();
    }

    public void PauseMusic()
    {
        currentMusicSource.Pause();
    }

    public void ResumeMusic()
    {
        currentMusicSource.UnPause();
    }
}