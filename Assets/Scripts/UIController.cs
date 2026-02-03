using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text warningText;

    [SerializeField] private Image livesImage;
    [SerializeField] private Image waveImage;
    [SerializeField] private Image goldImage;

    [SerializeField] private GameObject towerPanel;
    [SerializeField] private GameObject towerCardPrefab;
    [SerializeField] private Transform cardsContainer;

    [SerializeField] private TowerData[] towers;
    private List<GameObject> activeCards = new List<GameObject>();

    private Platform _currentPlatform;

    [SerializeField] private Button speed1Button;
    [SerializeField] private Button speed2Button;
    [SerializeField] private Button speed3Button;

    [SerializeField] private Color normalSpeedButtonColor = Color.white;
    [SerializeField] private Color selectedSpeedButtonColor = Color.blue;
    [SerializeField] private Color normalSpeedButtonTextColor = Color.black;
    [SerializeField] private Color selectedSpeedButtonTextColor = Color.white;

    [SerializeField] private TMP_Text objectiveText;

    [SerializeField] private GameObject pausePanel;
    private bool _isGamePaused = false;

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject missionCompletePanel;

    [SerializeField] private Button nextLevelButton;

    [Header("TowerUpgrade UI")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TMP_Text upgradeCostText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button sellButton;

    // Add these serialized fields to UIController
    [Header("Tower Sell UI")]
    [SerializeField] private TMP_Text sellValueText;

    [Header("Floating Text Feedback")]
    [SerializeField] private TMP_Text sellFeedbackText;
    [SerializeField] private float floatTextDuration = 2f;
    [SerializeField] private float floatTextSpeed = 1f;

    private void OnEnable()
    {
        Spawner.OnWaveChanged += UpdateWaveText;
        GameManager.OnLivesChanged += UpdateLivesText;
        GameManager.OnGoldChanged += UpdateGoldText;
        Platform.OnPlatformClicked += HandlePlatformClick;
        Platform.OnTowerSelected += HandleTowerSelected;
        TowerCard.OnTowerSelected += HandleTowerSelectedFromCard;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Spawner.OnMissionComplete += ShowMissionComplete;
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
        Enemy.OnEnemyReachedEnd += HandleEnemyReachedEnd;
    }

    private void OnDisable()
    {
        Spawner.OnWaveChanged -= UpdateWaveText;
        GameManager.OnLivesChanged -= UpdateLivesText;
        GameManager.OnGoldChanged -= UpdateGoldText;
        Platform.OnPlatformClicked -= HandlePlatformClick;
        Platform.OnTowerSelected -= HandleTowerSelected;
        TowerCard.OnTowerSelected -= HandleTowerSelectedFromCard;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Spawner.OnMissionComplete -= ShowMissionComplete;
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
        Enemy.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        // Add back button handling for Android/iOS
        if (Application.isMobilePlatform && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // On mobile, back button should act like escape
            TogglePause();
        }
    }

    private void Start()
    {
        // Add button click sounds to speed buttons
        speed1Button.onClick.AddListener(() => {
            PlayButtonClickSound();
            SetGameSpeed(0.2f);
        });
        speed2Button.onClick.AddListener(() => {
            PlayButtonClickSound();
            SetGameSpeed(1f);
        });
        speed3Button.onClick.AddListener(() => {
            PlayButtonClickSound();
            SetGameSpeed(2.5f);
        });

        HighlightSelectedSpeedButton(GameManager.Instance.GameSpeed);

        // Add button click sound to next level button
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(() => {
                PlayButtonClickSound();
                OnNextLevelButtonClicked();
            });
        }

        // Initialize floating text visibility
        if (sellFeedbackText != null)
            sellFeedbackText.gameObject.SetActive(false);

    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        GameManager.Instance.AddGold((int)enemy.Data.goldReward);
    }

    private void HandleEnemyReachedEnd(EnemyData enemyData)
    {
        GameManager.Instance.ChangeLives(-enemyData.damage);
    }

    private void SetHUDVisibility(bool isVisible)
    {
        waveText.gameObject.SetActive(isVisible);
        livesText.gameObject.SetActive(isVisible);
        goldText.gameObject.SetActive(isVisible);

        if (livesImage != null)
            livesImage.gameObject.SetActive(isVisible);

        if (waveImage != null)
            waveImage.gameObject.SetActive(isVisible);

        if (goldImage != null)
            goldImage.gameObject.SetActive(isVisible);

        speed1Button.gameObject.SetActive(isVisible);
        speed2Button.gameObject.SetActive(isVisible);
        speed3Button.gameObject.SetActive(isVisible);
    }

    private void UpdateWaveText(int currentWave)
    {
        waveText.text = $" {currentWave}/{LevelManager.Instance.CurrentLevel.wavesToWin}";
    }

    private void UpdateLivesText(int currentLives)
    {
        livesText.text = $"{currentLives}";

        if (currentLives <= 0)
        {
            ShowGameOver();
        }
    }

    private void UpdateGoldText(int currentGold)
    {
        goldText.text = $" {currentGold}";
    }

    private void HandlePlatformClick(Platform platform)
    {
        if (!platform.HasTower)
        {
            _currentPlatform = platform;
            ShowTowerPanel();
        }
    }

    private void ShowTowerPanel()
    {
        towerPanel.SetActive(true);
        Platform.towerPanelOpen = true;
        Spawner.Instance.PauseSpawning(true);
        GameManager.Instance.SetTimeScale(0f);
        PopulateTowerCards();
    }

    public void HideTowerPanel()
    {
        PlayButtonClickSound(); // Play sound when closing panel
        towerPanel.SetActive(false);
        Platform.towerPanelOpen = false;
        Spawner.Instance.PauseSpawning(false);
        GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);
    }

    private void PopulateTowerCards()
    {
        foreach (var card in activeCards)
        {
            Destroy(card);
        }

        foreach (var data in towers)
        {
            GameObject cardGameObject = Instantiate(towerCardPrefab, cardsContainer);
            TowerCard card = cardGameObject.GetComponent<TowerCard>();
            card.Initiliaze(data);
            activeCards.Add(cardGameObject);
        }
    }

    // Update the ShowUpgradePanel method to include sell value
    private void ShowUpgradePanel()
    {
        if (_currentPlatform == null || !_currentPlatform.HasTower) return;

        upgradePanel.SetActive(true);
        Platform.upgradePanelOpen = true;
        Spawner.Instance.PauseSpawning(true);
        GameManager.Instance.SetTimeScale(0f);

        TowerData currentTowerData = _currentPlatform.GetTowerData();

        Debug.Log($"Current Tower Data: {currentTowerData?.name}");
        Debug.Log($"Is Upgradable: {currentTowerData?.IsUpgradable}");
        Debug.Log($"Next Level Data: {currentTowerData?.nextLevelData?.name}");
        Debug.Log($"Upgrade Cost: {currentTowerData?.upgradeCost}");

        if (currentTowerData != null && currentTowerData.IsUpgradable)
        {
            upgradeCostText.text = "Cost:" + currentTowerData.upgradeCost.ToString();
            upgradeButton.interactable = GameManager.Instance.Golds >= currentTowerData.upgradeCost;
            Debug.Log("Upgrade panel shows upgradable tower");
        }
        else
        {
            upgradeCostText.text = "MAX LEVEL";
            upgradeButton.interactable = false;
            Debug.Log("Upgrade panel shows MAX level tower");
        }

        // Update sell value display
        UpdateSellValueDisplay();
    }

    private void UpdateSellValueDisplay()
    {
        if (_currentPlatform != null && sellValueText != null) // Added null check
        {
            int sellValue = _currentPlatform.GetSellValue();
            sellValueText.text = $"Sell: +{sellValue}";
        }
        else
        {
            Debug.LogWarning("sellValueText is not assigned in the Inspector!");
        }
    }


    private void HandleTowerSelected(Platform platform)
    {
        _currentPlatform = platform;
        ShowUpgradePanel();
    }

    private void HandleTowerSelectedFromCard(TowerData towerData)
    {
        if (GameManager.Instance.Golds >= towerData.cost)
        {
            // Play tower placement sound only - NO button click sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTowerPlace();
            }

            GameManager.Instance.SpendGolds(towerData.cost);
            _currentPlatform.PlaceTower(towerData);
            HideTowerPanel();
        }
        else
        {
            // Play not enough gold sound only - NO button click sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayNotEnoughGold();
            }
            
        }
    }


    public void ShowSellFeedbackMessage(string message)
    {
        if (sellFeedbackText != null)
        {
            StartCoroutine(ShowFloatingText(sellFeedbackText, message, Color.green));
        }
    }

    private IEnumerator ShowFloatingText(TMP_Text textElement, string message, Color color)
    {
        textElement.text = message;
        textElement.color = color;
        textElement.gameObject.SetActive(true);

        // Set initial position (center of screen)
        RectTransform rectTransform = textElement.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;

        Vector3 startPosition = rectTransform.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < floatTextDuration)
        {
            elapsedTime += Time.deltaTime;

            // Float upward
            float newY = startPosition.y + (elapsedTime * floatTextSpeed * 100f);
            rectTransform.anchoredPosition = new Vector2(startPosition.x, newY);

            // Fade out
            float alpha = 1f - (elapsedTime / floatTextDuration);
            Color newColor = color;
            newColor.a = alpha;
            textElement.color = newColor;

            yield return null;
        }

        textElement.gameObject.SetActive(false);
    }

    public void OnUpgradeButtonClicked()
    {
        PlayButtonClickSound(); // Play sound when clicking upgrade

        if (_currentPlatform != null && _currentPlatform.CanUpgradeTower())
        {
            int upgradeCost = _currentPlatform.GetUpgradeCost();
            if (GameManager.Instance.Golds >= upgradeCost)
            {
                // Play upgrade sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayTowerUpgrade();
                }

                GameManager.Instance.SpendGolds(upgradeCost);
                _currentPlatform.UpgradeTower();
                HideUpgradePanel();
            }
            else
            {
                // Play not enough gold sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayNotEnoughGold();
                }
                StartCoroutine(ShowWarningMessage("Not Enough Golds for Upgrade!"));
            }
        }
    }

    public void HideUpgradePanel()
    {
        PlayButtonClickSound(); // Play sound when closing upgrade panel
        upgradePanel.SetActive(false);
        Platform.upgradePanelOpen = false;
        Spawner.Instance.PauseSpawning(false);
        GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);
        _currentPlatform = null;
    }

    // Replace the current OnSellButtonClicked method
    public void OnSellButtonClicked()
    {
        PlayButtonClickSound();

        if (_currentPlatform != null && _currentPlatform.HasTower)
        {
            // Show confirmation dialog or sell directly
            SellTowerWithConfirmation();
        }
        else
        {
            
        }
    }

    // Method to handle selling with confirmation
    private void SellTowerWithConfirmation()
    {
        // For now, we'll sell directly. You can add a confirmation dialog later
        int sellValue = _currentPlatform.GetSellValue();

        // Play sell sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTowerSell(); // You'll need to add this method to AudioManager
        }

        _currentPlatform.SellTower();
        HideUpgradePanel();

        // Show sell feedback with floating text
        ShowSellFeedbackMessage($"+{sellValue} Gold");
    }

    private IEnumerator ShowWarningMessage(string message)
    {
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        warningText.gameObject.SetActive(false);
    }

    private void SetGameSpeed(float timeScale)
    {
        HighlightSelectedSpeedButton(timeScale);
        GameManager.Instance.SetGameSpeed(timeScale);
    }

    private void UpdateSpeedButtonVisual(Button button, bool isSelected)
    {
        button.image.color = isSelected ? selectedSpeedButtonColor : normalSpeedButtonColor;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.color = isSelected ? selectedSpeedButtonTextColor : normalSpeedButtonTextColor;
        }
    }

    private void HighlightSelectedSpeedButton(float selectedSpeed)
    {
        UpdateSpeedButtonVisual(speed1Button, selectedSpeed == 0.2f);
        UpdateSpeedButtonVisual(speed2Button, selectedSpeed == 1f);
        UpdateSpeedButtonVisual(speed3Button, selectedSpeed == 2.5f);
    }

    public void TogglePause()
    {
        if (towerPanel.activeSelf)
        {
            return;
        }

        if (_isGamePaused)
        {
            PlayButtonClickSound(); // Play sound when unpausing
            pausePanel.SetActive(false);
            SetHUDVisibility(true);
            _isGamePaused = false;
            GameManager.Instance.SetTimeScale(GameManager.Instance.GameSpeed);

            // Resume music when unpausing
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ResumeMusic();
            }
        }
        else
        {
            PlayButtonClickSound(); // Play sound when pausing
            pausePanel.SetActive(true);
            SetHUDVisibility(false);
            _isGamePaused = true;
            GameManager.Instance.SetTimeScale(0);

            // Pause music when pausing
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PauseMusic();
            }
        }
    }

    public void RestartLevel()
    {
        PlayButtonClickSound(); // Play sound when restarting

        SetHUDVisibility(true);
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        missionCompletePanel.SetActive(false);
        LevelManager.Instance.LoadLevel(LevelManager.Instance.CurrentLevel);
    }

    public void QuitGame()
    {
        PlayButtonClickSound(); // Play sound when quitting
        Application.Quit();
    }

    public void GoToMainMenu()
    {
        PlayButtonClickSound(); // Play sound when going to main menu
        GameManager.Instance.SetTimeScale(1f);

        // Music will automatically switch in LevelManager's OnSceneLoaded
        SceneManager.LoadScene("MainMenu");
    }

    private void ShowGameOver()
    {
        // Play game over sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseMusic(); // Pause in-game music
            AudioManager.Instance.PlayGameOver();
        }

        GameManager.Instance.SetTimeScale(0f);
        gameOverPanel.SetActive(true);
        SetHUDVisibility(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ShowObjective());
    }

    private IEnumerator ShowObjective()
    {
        objectiveText.text = $"Survive {LevelManager.Instance.CurrentLevel.wavesToWin} waves to win!";
        objectiveText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        objectiveText.gameObject.SetActive(false);
    }

    private void ShowMissionComplete()
    {
        if (missionCompletePanel != null)
        {
            // Play victory sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PauseMusic(); // Pause in-game music

                AudioManager.Instance.PlayVictory();
            }

            missionCompletePanel.SetActive(true);
            SetHUDVisibility(false);
            GameManager.Instance.SetTimeScale(0f);

            LevelManager.Instance.MarkLevelCompleted(LevelManager.Instance.CurrentLevel);

            int currentIndex = System.Array.IndexOf(LevelManager.Instance.allLevels,
                                                  LevelManager.Instance.CurrentLevel);
            bool hasNextLevel = currentIndex < LevelManager.Instance.allLevels.Length - 1;

            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(hasNextLevel);
            }
        }
        else
        {
            Debug.LogError("Mission completed panel reference is missing!");
        }
    }

    public void OnNextLevelButtonClicked()
    {
        // Resume music when going to next level
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeMusic();
        }
        LevelManager.Instance.LoadNextLevel();
    }

    // Helper method to play button click sound
    private void PlayButtonClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}