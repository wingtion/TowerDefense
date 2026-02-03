using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<int> OnLivesChanged;
    public static event Action<int> OnGoldChanged;

    private int _lives;
    private int _golds;

    private float _gameSpeed = 1f;
    public float GameSpeed => _gameSpeed;

    public int Golds => _golds; 


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Duplicates engelle
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Prefab kalýcý olsun

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetGameState();
    }

    public void ResetGameState()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.CurrentLevel != null)
        {
            _lives = LevelManager.Instance.CurrentLevel.startingLives;
            _golds = LevelManager.Instance.CurrentLevel.startingGolds;
        }
        else
        {
            _lives = 20;
            _golds = 175;
        }

        OnLivesChanged?.Invoke(_lives);
        OnGoldChanged?.Invoke(_golds);
        SetGameSpeed(1f);
    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
    }

    public void SetGameSpeed(float newSpeed)
    {
        _gameSpeed = newSpeed;
        SetTimeScale(_gameSpeed);
    }

    public void SpendGolds(int amount)
    {
        if (_golds >= amount)
        {
            _golds -= amount;
            OnGoldChanged?.Invoke(_golds);
        }
    }

    public void AddGold(int amount)
    {
        _golds += amount;
        OnGoldChanged?.Invoke(_golds);
    }

    public void ChangeLives(int amount)
    {
        _lives = Math.Max(0, _lives + amount);
        OnLivesChanged?.Invoke(_lives);
    }
}
