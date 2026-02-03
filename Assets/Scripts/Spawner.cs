using System;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static Spawner Instance { get; private set; }
    public static event Action<int> OnWaveChanged;
    public static event Action OnMissionComplete;

    [SerializeField] private WaveData[] waves;
    private Dictionary<EnemyType, ObjectPooler> _poolDictionary = new Dictionary<EnemyType, ObjectPooler>();

    private int _currentWaveIndex = 0;
    private int _currentGroupIndex = 0;
    private int _enemiesSpawnedInGroup = 0;
    private float _spawnTimer = 0f;
    private float _waveCooldown = 0f;
    private int _waveCounter = 0;
    private bool _isBetweenWaves = false;

    private bool _isSpawningPaused = false;

    private int _activeEnemies = 0;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize all pools (assign these in inspector)
        foreach (var pool in GetComponents<ObjectPooler>())
        {
            _poolDictionary[pool.enemyType] = pool;
        }
    }

    private void Start()
    {
        // Initialize counters
        _waveCounter = 1; // Start counting from Wave 1
        _currentWaveIndex = 0;
        _currentGroupIndex = 0;
        _enemiesSpawnedInGroup = 0;
        _spawnTimer = 0f;
        _isBetweenWaves = false;

        // Start first wave
        OnWaveChanged?.Invoke(_waveCounter);
        StartNextWave();
    }

    private void Update()
    {
        if (_isSpawningPaused) return;


        if (_isBetweenWaves)
        {
            _waveCooldown -= Time.deltaTime;
            if (_waveCooldown <= 0f)
            {
                StartNextWave();
            }
            return;
        }

        WaveData currentWave = waves[_currentWaveIndex];
        WaveData.EnemyGroup currentGroup = currentWave.enemyGroups[_currentGroupIndex];

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f && _enemiesSpawnedInGroup < currentGroup.count)
        {
            SpawnEnemy(currentGroup.enemyType);
            _enemiesSpawnedInGroup++;
            _spawnTimer = 1f / currentGroup.spawnRate;
        }
        else if (_enemiesSpawnedInGroup >= currentGroup.count)
        {
            _currentGroupIndex++;
            if (_currentGroupIndex >= currentWave.enemyGroups.Length)
            {
                EndCurrentWave();
                return;
            }
            _enemiesSpawnedInGroup = 0;
            _spawnTimer = 0f;
        }
    }

    public ObjectPooler GetPoolForType(EnemyType type)
    {
        _poolDictionary.TryGetValue(type, out var pool);
        return pool;
    }


    public void PauseSpawning(bool pause)
    {
        _isSpawningPaused = pause;
    }

    // Add this method to handle enemy destruction
    public void OnEnemyDestroyed()
    {
        _activeEnemies--;

        // Check if we've completed all waves AND all enemies are defeated
        if (_waveCounter > LevelManager.Instance.CurrentLevel.wavesToWin && _activeEnemies <= 0)
        {
            OnMissionComplete?.Invoke();
        }
    }


    // In Spawner.cs
    private void SpawnEnemy(EnemyType type)
    {
        if (_poolDictionary.TryGetValue(type, out ObjectPooler pool))
        {
            GameObject enemyObj = pool.GetPooledObject();
            enemyObj.transform.position = transform.position;

            Enemy enemy = enemyObj.GetComponent<Enemy>();
            float healthMultiplier = 1f + (_waveCounter * waves[_currentWaveIndex].healthMultiplierPerWave);
            enemy.Initialize(healthMultiplier);

            enemyObj.SetActive(true);
            _activeEnemies++; // Increment active enemies count
        }
    }

    private void StartNextWave()
    {
        // LevelManager'ýn mevcut leveline göre wave kontrolü yap
        int wavesToWin = LevelManager.Instance.CurrentLevel.wavesToWin;

        if (_waveCounter > wavesToWin && _activeEnemies <= 0)
        {
            OnMissionComplete?.Invoke();
            return;
        }

        // First check if we have active enemies
        if (_activeEnemies > 0)
        {
            // Don't start next wave until all enemies are defeated
            return;
        }

        // Then check if we've completed all required waves
        if (_waveCounter > LevelManager.Instance.CurrentLevel.wavesToWin)
        {
            // Only complete mission if no enemies are left
            if (_activeEnemies <= 0)
            {
                OnMissionComplete?.Invoke();
            }
            return;
        }



        // Reset wave state
        _currentGroupIndex = 0;
        _enemiesSpawnedInGroup = 0;
        _spawnTimer = 0f;
        _isBetweenWaves = false;

        // Notify UI of new wave
        OnWaveChanged?.Invoke(_waveCounter);
    }

    // Add this method to your Spawner class
    public void ResetSpawner()
    {
        Debug.Log("Resetting spawner for new level");

        // Stop all coroutines first
        StopAllCoroutines();

        // Reset all spawner state
        _currentWaveIndex = 0;
        _currentGroupIndex = 0;
        _enemiesSpawnedInGroup = 0;
        _spawnTimer = 0f;
        _waveCooldown = 0f;
        _waveCounter = 1; // Start from wave 1
        _isBetweenWaves = false;
        _isSpawningPaused = false;
        _activeEnemies = 0;

        // Notify UI of the new wave immediately
        OnWaveChanged?.Invoke(_waveCounter);

        Debug.Log($"Spawner reset. Starting wave: {_waveCounter}");

        // Start the first wave after a short delay
        StartCoroutine(StartFirstWaveAfterDelay());
    }

    private System.Collections.IEnumerator StartFirstWaveAfterDelay()
    {
        yield return new WaitForSeconds(1f); // Wait for scene to stabilize
        StartNextWave();
    }


    private void EndCurrentWave()
    {
        _isBetweenWaves = true;
        _waveCooldown = waves[_currentWaveIndex].timeBetweenWaves;

        // Move to next wave data (without modulo)
        _currentWaveIndex++;
        if (_currentWaveIndex >= waves.Length)
        {
            _currentWaveIndex = 0; // Optionally loop or handle differently
        }

        // Increment wave counter after completing current wave
        _waveCounter++;
    }


    public void RegisterEnemyPool(EnemyType type, ObjectPooler pool)
    {
        _poolDictionary[type] = pool;
    }
}