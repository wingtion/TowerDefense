using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private LevelSelectionController levelSelectionController;

    private void Start()
    {
        // Ensure main menu music is playing when starting in main menu
        if (AudioManager.Instance != null && SceneManager.GetActiveScene().name == "MainMenu")
        {
            AudioManager.Instance.PlayMainMenuMusic();
        }
    }

    public void StartNewGame()
    {
        PlayButtonClickSound();
        ResetAllLevelsProgress();
        LevelManager.Instance.LoadLevel(LevelManager.Instance.allLevels[0]);
    }

    private void ResetAllLevelsProgress()
    {
        foreach (LevelData level in LevelManager.Instance.allLevels)
        {
            string completionKey = $"Level_{level.levelName}_Completed";
            PlayerPrefs.SetInt(completionKey, 0);
        }
        PlayerPrefs.Save();
        Debug.Log("All level progress reset.");
    }

    public void ShowLevelSelection()
    {
        PlayButtonClickSound();
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (levelSelectionController != null)
            levelSelectionController.ShowLevelSelection();
    }

    public void ShowMainMenu()
    {
        PlayButtonClickSound();
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (levelSelectionController != null)
            levelSelectionController.HideLevelSelection();
    }

    public void QuitGame()
    {
        PlayButtonClickSound();
        Application.Quit();
    }

    private void PlayButtonClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}