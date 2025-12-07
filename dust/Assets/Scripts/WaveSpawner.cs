using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [Header("Waves")]
    [Tooltip("Configure waves here. Can have 5 or any number.")]
    public Wave[] waves;

    [Tooltip("Start spawning as soon as the scene starts.")]
    public bool autoStart = true;

    [Header("Spawn Points")]
    [Tooltip("If a SpawnSet has no points, these are used instead.")]
    public Transform[] defaultSpawnPoints;

    [Header("Rewards")]
    public ChestSpawner chestSpawner;

    [Header("HUD")]
    public WaveHUD hud;

    public int CurrentWaveIndex { get; private set; } = -1;
    public int TotalWaves => waves != null ? waves.Length : 0;

    public int EnemiesAlive => _aliveTotal;

    private bool _running;
    private Coroutine _waveRoutine;

    private int _aliveTotal = 0;
    private readonly Dictionary<int, int> _alivePerWave = new();

    [Header("Audio")]
    [Tooltip("Where SFX will play from. Can be this same GameObject.")]
    public AudioSource sfxSource;
    public AudioClip countdownBeep;
    public AudioClip waveStartSfx;

    [Header("Timer Visuals")]
    [Tooltip("Turn the timer value red on last 3 seconds.")]
    public bool colorLastThreeSeconds = true;

    [Header("Intermission / Shop")]
    [Tooltip("If true, a shop intermission runs between waves.")]
    public bool useIntermission = true;

    [Tooltip("Seconds between waves where the shopkeeper is available.")]
    public float intermissionDuration = 30f;

    [Tooltip("Shopkeeper that appears during intermission (usually starts disabled).")]
    public Shopkeeper shopkeeper;

    [Tooltip("Optional SFX when intermission starts.")]
    public AudioClip intermissionStartSfx;

    [Tooltip("Optional SFX when intermission ends / next wave begins.")]
    public AudioClip intermissionEndSfx;

    [Header("Intermission Camera Focus")]
    [Tooltip("Camera that will be steered to show the shopkeeper when he first appears.")]
    public CameraFollow intermissionCamera;

    [Tooltip("How long to pan to/from the shopkeeper (seconds, unscaled).")]
    public float shopFocusPanTime = 0.7f;

    [Tooltip("How long to hold on the shopkeeper (seconds, unscaled).")]
    public float shopFocusHoldTime = 0.6f;

    [Tooltip("If true, only play the camera focus the first time the shopkeeper appears.")]
    public bool onlyShowShopFocusOnce = true;

    private bool _hasShownShopFocus = false;


    [Serializable]
    public class Wave
    {
        [Tooltip("Name")]
        public string waveName = "Wave";

        [Tooltip("If true, use a timer (waveDuration). If false, wait until all enemies are dead.")]
        public bool useTimer = true;

        [Tooltip("Seconds before advancing (only used if useTimer = true).")]
        public float waveDuration = 30f;

        [Tooltip("What to spawn in this wave.")]
        public SpawnSet[] spawns;

        [Tooltip("If true, an intermission will occur AFTER this wave (if global intermissions are enabled and there's a next wave).")]
        public bool intermissionAfterThisWave = true;
    }

    [Serializable]
    public class SpawnSet
    {
        public GameObject enemyPrefab;

        [Min(1)] public int count = 1;

        [Tooltip("Specific spawn points for this enemy type. If empty, defaults are used.")]
        public Transform[] spawnPoints;

        [Tooltip("Small delay between spawns of this set.")]
        public float perSpawnDelay = 0.1f;
    }

    private void Start()
    {
        if (intermissionCamera == null)
            intermissionCamera = FindFirstObjectByType<CameraFollow>();

        if (autoStart && TotalWaves > 0)
        {
            StartWaves();
        }
    }

    public void StartWaves()
    {
        if (_running || TotalWaves == 0) return;
        _running = true;
        AdvanceToNextWave();
    }

    private void AdvanceToNextWave()
    {
        hud?.HideIntermission();

        CurrentWaveIndex++;

        if (CurrentWaveIndex >= TotalWaves)
        {
            _running = false;
            hud?.SetWaveText(TotalWaves, TotalWaves);
            hud?.SetEnemies(_aliveTotal);
            hud?.SetTimer("--");
            return;
        }

        if (_waveRoutine != null) StopCoroutine(_waveRoutine);
        _waveRoutine = StartCoroutine(RunWave(waves[CurrentWaveIndex], CurrentWaveIndex));
    }

    private IEnumerator RunWave(Wave wave, int waveIndex)
    {
        hud?.SetWaveText(waveIndex + 1, TotalWaves);

        if (sfxSource && waveStartSfx)
            sfxSource.PlayOneShot(waveStartSfx, 0.3f);
        hud?.SetTimerUrgent(false);

        if (!_alivePerWave.ContainsKey(waveIndex))
            _alivePerWave[waveIndex] = 0;

        foreach (var set in wave.spawns)
        {
            if (set.enemyPrefab == null || set.count <= 0) continue;

            var points = (set.spawnPoints != null && set.spawnPoints.Length > 0)
                ? set.spawnPoints
                : defaultSpawnPoints;

            if (points == null || points.Length == 0)
            {
                Debug.LogWarning($"WaveSpawner: No spawn points defined for {set.enemyPrefab?.name}. " +
                                "Add defaultSpawnPoints or per-set spawnPoints.");
                continue;
            }

            for (int i = 0; i < set.count; i++)
            {
                var p = points[UnityEngine.Random.Range(0, points.Length)];
                var go = Instantiate(set.enemyPrefab, p.position, p.rotation);

                var marker = go.GetComponent<EnemyMarker>();
                if (marker == null) marker = go.AddComponent<EnemyMarker>();
                marker.waveId = waveIndex;
                marker.OnEnemyDied += HandleEnemyDied;

                _aliveTotal++;
                _alivePerWave[waveIndex] = _alivePerWave[waveIndex] + 1;

                hud?.SetEnemies(_aliveTotal);

                if (set.perSpawnDelay > 0f)
                    yield return new WaitForSeconds(set.perSpawnDelay);
            }
        }


        if (wave.useTimer)
        {
            float t = Mathf.Max(0f, wave.waveDuration);
            int lastWhole = Mathf.CeilToInt(t);

            while (t > 0f)
            {
                hud?.SetTimer(FormatTime(t));
                bool urgent = colorLastThreeSeconds && (Mathf.CeilToInt(t) <= 3) && (t > 0f);
                hud?.SetTimerUrgent(urgent);

                t -= Time.deltaTime;

                int currWhole = Mathf.CeilToInt(Mathf.Max(0f, t));
                if (currWhole < lastWhole)
                {
                    if (currWhole > 0 && currWhole <= 3)
                    {
                        if (sfxSource && countdownBeep)
                            sfxSource.PlayOneShot(countdownBeep, 0.3f);
                    }
                    lastWhole = currWhole;
                }

                yield return null;
            }

            hud?.SetTimer("0:00");
            hud?.SetTimerUrgent(false);
        }
        else
        {
            hud?.SetTimer("∞");
            hud?.SetTimerUrgent(false);

            while (GetAliveForWave(waveIndex) > 0)
            {
                yield return null;
            }
        }

        if (chestSpawner) chestSpawner.SpawnRandomChest();

        bool hasNextWave = (waveIndex + 1) < TotalWaves;
        bool shouldRunIntermission =
            hasNextWave &&
            useIntermission &&
            intermissionDuration > 0f &&
            wave.intermissionAfterThisWave;

        if (shouldRunIntermission)
        {
            yield return StartCoroutine(RunIntermission());
        }

        AdvanceToNextWave();
    }

    private IEnumerator RunIntermission()
    {
        hud?.SetWaveText(CurrentWaveIndex + 1, TotalWaves);
        hud?.ShowIntermission("Intermission");

        if (shopkeeper != null)
            shopkeeper.gameObject.SetActive(true);

        if (sfxSource && intermissionStartSfx)
            sfxSource.PlayOneShot(intermissionStartSfx, 0.3f);

        if (shopkeeper != null && intermissionCamera != null &&
            (!_hasShownShopFocus || !onlyShowShopFocusOnce))
        {
            _hasShownShopFocus = true;
            yield return StartCoroutine(FocusCameraOnShopkeeper());
        }

        float t = Mathf.Max(0f, intermissionDuration);
        int lastWhole = Mathf.CeilToInt(t);

        while (t > 0f)
        {
            hud?.SetTimer(FormatTime(t));
            bool urgent = colorLastThreeSeconds && Mathf.CeilToInt(t) <= 3;
            hud?.SetTimerUrgent(urgent);

            t -= Time.deltaTime;

            int currWhole = Mathf.CeilToInt(Mathf.Max(0f, t));
            if (currWhole < lastWhole)
            {
                if (currWhole > 0 && currWhole <= 3)
                {
                    if (sfxSource && countdownBeep)
                        sfxSource.PlayOneShot(countdownBeep, 0.3f);
                }
                lastWhole = currWhole;
            }

            yield return null;
        }

        hud?.SetTimer("0:00");
        hud?.SetTimerUrgent(false);
        hud?.HideIntermission();

        if (sfxSource && intermissionEndSfx)
            sfxSource.PlayOneShot(intermissionEndSfx, 0.3f);

        if (shopkeeper != null)
            shopkeeper.gameObject.SetActive(false);
    }

    private int GetAliveForWave(int waveId)
    {
        return _alivePerWave.TryGetValue(waveId, out var v) ? v : 0;
    }

    private void HandleEnemyDied(EnemyMarker m)
    {
        _aliveTotal = Mathf.Max(0, _aliveTotal - 1);

        if (m != null && _alivePerWave.ContainsKey(m.waveId))
        {
            _alivePerWave[m.waveId] = Mathf.Max(0, _alivePerWave[m.waveId] - 1);
        }

        hud?.SetEnemies(_aliveTotal);
    }

    private static string FormatTime(float seconds)
    {
        seconds = Mathf.Max(0f, seconds);
        int s = Mathf.FloorToInt(seconds % 60f);
        int m = Mathf.FloorToInt(seconds / 60f);
        return $"{m}:{s:00}";
    }

        private IEnumerator FocusCameraOnShopkeeper()
    {
        if (shopkeeper == null || intermissionCamera == null)
            yield break;

        Transform camTransform = intermissionCamera.transform;
        Transform originalTarget = intermissionCamera.target;
        float originalSmooth = intermissionCamera.smooth;

        float totalCutsceneTime = shopFocusPanTime * 2f + shopFocusHoldTime;
        var player = FindFirstObjectByType<PlayerMovementSM>();
        if (player != null)
        {
            player.DisableMovement(totalCutsceneTime);
        }

        intermissionCamera.enabled = false;

        Vector3 startPos = camTransform.position;
        Vector3 shopPos = shopkeeper.transform.position;
        Vector3 targetPos = new Vector3(shopPos.x, shopPos.y, startPos.z);

        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f; 

        float elapsed = 0f;
        while (elapsed < shopFocusPanTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / shopFocusPanTime);
            camTransform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < shopFocusHoldTime)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < shopFocusPanTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / shopFocusPanTime);
            camTransform.position = Vector3.Lerp(targetPos, startPos, t);
            yield return null;
        }

        Time.timeScale = originalTimeScale;
        intermissionCamera.enabled = true;
        intermissionCamera.target = originalTarget;
        intermissionCamera.smooth = originalSmooth;
    }

}
