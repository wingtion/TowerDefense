using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public LevelData[] allLevels;
    public LevelData CurrentLevel { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (allLevels == null || allLevels.Length == 0)
        {
            LoadAllLevelsData();
        }
    }

    private void LoadAllLevelsData()
    {
        allLevels = Resources.LoadAll<LevelData>("LevelData");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndSetCurrentLevel();
        HandleSceneMusic(scene.name);
    }

    private void FindAndSetCurrentLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        foreach (LevelData level in allLevels)
        {
            if (level.levelName == sceneName)
            {
                CurrentLevel = level;
                Debug.Log($"Level set to: {level.levelName}");
                return;
            }
        }

        if (allLevels.Length > 0)
        {
            CurrentLevel = allLevels[0];
            Debug.LogWarning($"No level data found for scene: {sceneName}, using default: {allLevels[0].levelName}");
        }
    }

    private void HandleSceneMusic(string sceneName)
    {
        if (AudioManager.Instance != null)
        {
            if (sceneName == "MainMenu")
            {
                AudioManager.Instance.PlayMainMenuMusic(0.3f);
            }
            else
            {
                // Assume any other scene is a gameplay level
                AudioManager.Instance.PlayInGameMusic(0.08f);
            }
        }
    }

    public void LoadLevel(LevelData levelData)
    {
        if (levelData != null)
        {
            // Reset audio flags before loading new scene
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ResetAllFlags();
            }
            SceneManager.LoadScene(levelData.levelName);
        }
    }

    public void LoadNextLevel()
    {
        MarkLevelCompleted(CurrentLevel);

        // Reset audio flags before loading next level
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResetAllFlags();
        }

        int currentIndex = System.Array.IndexOf(allLevels, CurrentLevel);

        if (currentIndex >= 0 && currentIndex < allLevels.Length - 1)
        {
            LoadLevel(allLevels[currentIndex + 1]);
        }
        else
        {
            Debug.Log("No next level, returning to Main Menu");
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void MarkLevelCompleted(LevelData levelData)
    {
        if (levelData == null) return;

        string completionKey = $"Level_{levelData.levelName}_Completed";
        PlayerPrefs.SetInt(completionKey, 1);
        PlayerPrefs.Save();
        Debug.Log($"Marked {levelData.levelName} as completed.");
    }
}