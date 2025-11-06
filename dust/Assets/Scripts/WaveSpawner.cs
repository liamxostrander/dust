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
            while (t > 0f)
            {
                hud?.SetTimer(FormatTime(t));
                yield return null;
                t -= Time.deltaTime;
            }
            hud?.SetTimer("0:00");
            if (chestSpawner) chestSpawner.SpawnRandomChest();
            AdvanceToNextWave();
        }
        else
        {
            hud?.SetTimer("∞");
            while (GetAliveForWave(waveIndex) > 0)
            {
                yield return null;
            }
            if (chestSpawner) chestSpawner.SpawnRandomChest();
            AdvanceToNextWave();
        }
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
            // if (_alivePerWave[m.waveId] == 0) _alivePerWave.Remove(m.waveId);
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
}
