using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject levelSelectionPanel;
    [SerializeField] private Transform levelButtonsContainer;
    [SerializeField] private GameObject levelButtonPrefab;

    [Header("Extra UI")]
    [SerializeField] private GameObject backButtonPrefab;

    [Header("Level Settings")]
    [SerializeField] private LevelData[] availableLevels;

    [SerializeField] private MainMenuController mainMenuController;

    private void Start()
    {
        if (levelSelectionPanel != null)
            levelSelectionPanel.SetActive(false);
        else
            Debug.LogError("LevelSelectionPanel is not assigned!");

        CreateLevelButtons();
    }

    private void CreateLevelButtons()
    {
        if (levelButtonsContainer == null) return;

        foreach (Transform child in levelButtonsContainer)
            Destroy(child.gameObject);

        System.Array.Sort(availableLevels, (a, b) => a.levelIndex.CompareTo(b.levelIndex));

        foreach (LevelData level in availableLevels)
        {
            if (level == null) continue;

            GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonsContainer);
            LevelButton levelButton = buttonObj.GetComponent<LevelButton>();

            if (levelButton != null)
            {
                levelButton.Initialize(level);
                levelButton.OnLevelSelected += OnLevelSelected;
            }
        }

        CreateBackButton();
    }

    private void CreateBackButton()
    {
        if (backButtonPrefab == null)
        {
            Debug.LogWarning("Back button prefab not assigned!");
            return;
        }

        GameObject backButtonObj = Instantiate(backButtonPrefab, levelButtonsContainer);
        Button backButton = backButtonObj.GetComponent<Button>();

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonPressed);
    }

    private void OnLevelSelected(LevelData levelData)
    {
        PlayButtonClickSound();
        LevelManager.Instance.LoadLevel(levelData);
    }

    public void ShowLevelSelection()
    {
        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(true);
            RefreshLevelButtons();
        }
    }

    public void HideLevelSelection()
    {
        PlayButtonClickSound();
        if (levelSelectionPanel != null)
            levelSelectionPanel.SetActive(false);
    }

    private void RefreshLevelButtons()
    {
        CreateLevelButtons();
    }

    public void OnBackButtonPressed()
    {
        PlayButtonClickSound();
        if (mainMenuController != null)
            mainMenuController.ShowMainMenu();
        else
            Debug.LogWarning("MainMenuController reference not assigned in LevelSelectionController.");
    }

    private void OnDestroy()
    {
        if (levelButtonsContainer != null)
        {
            foreach (Transform child in levelButtonsContainer)
            {
                LevelButton levelButton = child.GetComponent<LevelButton>();
                if (levelButton != null)
                    levelButton.OnLevelSelected -= OnLevelSelected;
            }
        }
    }

    private void PlayButtonClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}