using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameModeSurvival : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private List<Transform> spawnPoints = new();
    [SerializeField, Min(0f)] private float interSpawnDelay = 0.35f;

    [Header("UI (optional)")]
    [SerializeField] private TextMeshProUGUI waveText;         // "Wave 3"
    [SerializeField] private TextMeshProUGUI enemiesLeftText;  // "Left: 12"
    [SerializeField] private TextMeshProUGUI bossIncomingText; // "BOSS INCOMING!" (опціонально)

    [Header("Mode")]
    [SerializeField] private bool usePredefinedWaves = false;
    [SerializeField] private List<WaveAsset> predefinedWaves = new();

    // ---------- БОСИ ----------
    [System.Serializable]
    public class BossEntry
    {
        public GameObject prefab;
        [Range(1, 100)] public int weight = 10;   // вага випадіння
        [Min(1)] public int unlockWave = 1;      // з якої хвилі доступний
    }

    [Header("Boss Waves")]
    [SerializeField, Min(0)] private int bossEveryNWaves = 5; // 0 = боси вимкнені
    [SerializeField] private List<BossEntry> bosses = new();
    [SerializeField, Min(1)] private int bossCountOnBossWave = 1; // скільки босів на бос-хвилі
    [SerializeField, Min(0f)] private float bossWarningTime = 2.0f;

    // ---------- Автогенерація звичайних хвиль ----------
    [Header("Auto Generation (коли usePredefinedWaves = false)")]
    [SerializeField, Min(1)] private int baseCount = 6;
    [SerializeField, Min(0)] private int addPerWave = 2;

    [SerializeField] private List<GameObject> tier1Enemies = new();
    [SerializeField] private List<GameObject> tier2Enemies = new();
    [SerializeField] private List<GameObject> tier3Enemies = new();

    [SerializeField, Min(1)] private int unlockTier2AtWave = 3;
    [SerializeField, Min(1)] private int unlockTier3AtWave = 7;

    [Tooltip("Розподіл типів у хвилі. Нормалізується автоматично.")]
    [SerializeField] private Vector3 tierWeightsEarly = new Vector3(1f, 0f, 0f);     // лише T1
    [SerializeField] private Vector3 tierWeightsMid = new Vector3(0.7f, 0.3f, 0f); // T1+T2
    [SerializeField] private Vector3 tierWeightsLate = new Vector3(0.5f, 0.3f, 0.2f);// T1+T2+T3

    // ---------- Runtime ----------
    public int CurrentWave { get; private set; } = 0;
    public int AliveEnemies { get; private set; } = 0;
    public bool IsWaveRunning { get; private set; } = false;

    public System.Action<int> OnWaveStarted;
    public System.Action<int> OnWaveCleared;   // коли останній ворог впав
    public System.Action<int> OnReadyForNext;  // коли готові стартувати наступну (ручний старт)

    private Coroutine _waveRoutine;

    private void Start()
    {
        if (spawnPoints.Count == 0)
        {
            var go = new GameObject("SpawnPoint_Auto");
            go.transform.position = Vector3.forward * 5f;
            spawnPoints.Add(go.transform);
        }
        if (bossIncomingText) bossIncomingText.gameObject.SetActive(false);

        UpdateWaveUI();
        UpdateAliveUI();
    }

    /// <summary>Запускає рівно одну хвилю (викликається з тригера).</summary>
    public void RequestStartNextWave()
    {
        if (IsWaveRunning || _waveRoutine != null) return;
        _waveRoutine = StartCoroutine(RunSingleWave());
    }

    private IEnumerator RunSingleWave()
    {
        IsWaveRunning = true;
        CurrentWave++;
        UpdateWaveUI();
        ClearAlive();

        OnWaveStarted?.Invoke(CurrentWave);

        bool isBossWave = (bossEveryNWaves > 0) && (CurrentWave % bossEveryNWaves == 0) && HasAnyAvailableBoss(CurrentWave);

        if (isBossWave)
        {
            if (bossIncomingText)
            {
                bossIncomingText.text = "BOSS INCOMING!";
                bossIncomingText.gameObject.SetActive(true);
            }
            if (bossWarningTime > 0f) yield return new WaitForSeconds(bossWarningTime);
            if (bossIncomingText) bossIncomingText.gameObject.SetActive(false);

            for (int i = 0; i < bossCountOnBossWave; i++)
            {
                var bossPrefab = PickBossForWave(CurrentWave);
                if (bossPrefab)
                {
                    Transform sp = PickSpawn();
                    SpawnOne(bossPrefab, sp.position, sp.rotation);
                    yield return new WaitForSeconds(0.25f);
                }
            }
        }
        else
        {
            if (usePredefinedWaves && CurrentWave - 1 < predefinedWaves.Count)
                yield return StartCoroutine(SpawnPredefinedWave(predefinedWaves[CurrentWave - 1]));
            else
                yield return StartCoroutine(SpawnAutoWave(CurrentWave));
        }

        // чекаємо, поки всіх знищать
        while (AliveEnemies > 0)
            yield return null;

        OnWaveCleared?.Invoke(CurrentWave);
        IsWaveRunning = false;
        _waveRoutine = null;

        // готові до наступної — тригер може показати «Натисни E…»
        OnReadyForNext?.Invoke(CurrentWave + 1);
    }

    // ===== ручна хвиля зі ScriptableObject =====
    private IEnumerator SpawnPredefinedWave(WaveAsset wave)
    {
        foreach (var e in wave.Entries)
        {
            for (int i = 0; i < e.Count; i++)
            {
                Transform sp = PickSpawn();
                SpawnOne(e.Prefab, sp.position, sp.rotation);
                yield return new WaitForSeconds(interSpawnDelay);
            }
        }
    }

    // ===== авто-генерація хвилі =====
    private IEnumerator SpawnAutoWave(int waveNumber)
    {
        int total = Mathf.Max(1, baseCount + (waveNumber - 1) * addPerWave);

        // вибір тиру + ваги
        Vector3 w;
        List<GameObject> t1 = tier1Enemies, t2 = null, t3 = null;
        if (waveNumber >= unlockTier3AtWave && tier3Enemies.Count > 0)
        {
            w = tierWeightsLate; t2 = tier2Enemies.Count > 0 ? tier2Enemies : null; t3 = tier3Enemies;
        }
        else if (waveNumber >= unlockTier2AtWave && tier2Enemies.Count > 0)
        {
            w = tierWeightsMid; t2 = tier2Enemies;
        }
        else
        {
            w = tierWeightsEarly;
        }

        float sum = Mathf.Max(0.0001f, w.x + w.y + w.z);
        w /= sum;

        int c1 = Mathf.RoundToInt(total * w.x);
        int c2 = Mathf.RoundToInt(total * w.y);
        int c3 = total - c1 - c2;

        List<GameObject> queue = new List<GameObject>(total);
        if (t1 != null && t1.Count > 0) for (int i = 0; i < c1; i++) queue.Add(PickRand(t1));
        if (t2 != null && t2.Count > 0) for (int i = 0; i < c2; i++) queue.Add(PickRand(t2));
        if (t3 != null && t3.Count > 0) for (int i = 0; i < c3; i++) queue.Add(PickRand(t3));
        while (queue.Count < total && t1 != null && t1.Count > 0) queue.Add(PickRand(t1));

        // шафл
        for (int i = 0; i < queue.Count; i++)
        {
            int j = Random.Range(i, queue.Count);
            (queue[i], queue[j]) = (queue[j], queue[i]);
        }

        foreach (var prefab in queue)
        {
            Transform sp = PickSpawn();
            SpawnOne(prefab, sp.position, sp.rotation);
            yield return new WaitForSeconds(interSpawnDelay);
        }
    }

    // ===== спавн одиниці + підписка на смерть =====
    private void SpawnOne(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!prefab) return;

        var go = Instantiate(prefab, pos, rot);
        AliveEnemies++;
        UpdateAliveUI();

        var hp = go.GetComponent<Health>() ?? go.GetComponentInChildren<Health>();
        if (hp != null)
        {
            hp.OnDied += () =>
            {
                AliveEnemies = Mathf.Max(0, AliveEnemies - 1);
                UpdateAliveUI();
            };
        }
    }

    // ===== утиліти =====
    private Transform PickSpawn()
    {
        if (spawnPoints.Count == 0) return transform;
        int idx = Random.Range(0, spawnPoints.Count);
        return spawnPoints[idx];
    }

    private GameObject PickRand(List<GameObject> list)
    {
        int idx = Random.Range(0, list.Count);
        return list[idx];
    }

    private void ClearAlive()
    {
        AliveEnemies = 0;
        UpdateAliveUI();
    }

    private void UpdateWaveUI()
    {
        if (waveText) waveText.text = $"Wave {Mathf.Max(1, CurrentWave)}";
    }

    private void UpdateAliveUI()
    {
        if (enemiesLeftText) enemiesLeftText.text = $"Left: {AliveEnemies}";
    }

    // ===== бос-логіка =====
    private bool HasAnyAvailableBoss(int wave)
    {
        foreach (var b in bosses)
        {
            if (b.prefab && wave >= Mathf.Max(1, b.unlockWave))
                return true;
        }
        return false;
    }

    private GameObject PickBossForWave(int wave)
    {
        int total = 0;
        for (int i = 0; i < bosses.Count; i++)
        {
            var b = bosses[i];
            if (!b.prefab) continue;
            if (wave < Mathf.Max(1, b.unlockWave)) continue;
            total += Mathf.Max(0, b.weight);
        }
        if (total == 0) return null;

        int r = Random.Range(0, total);
        for (int i = 0; i < bosses.Count; i++)
        {
            var b = bosses[i];
            if (!b.prefab) continue;
            if (wave < Mathf.Max(1, b.unlockWave)) continue;

            int w = Mathf.Max(0, b.weight);
            if (r < w) return b.prefab;
            r -= w;
        }
        return null;
    }
}
