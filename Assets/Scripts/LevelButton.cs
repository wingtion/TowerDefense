using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LevelButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private TMP_Text levelDescriptionText;
    [SerializeField] private Image levelPreviewImage;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private GameObject completedOverlay;
    [SerializeField] private Image[] starIcons;
    [SerializeField] private Button button;
    [SerializeField] private Image buttonBackground;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.gray;
    [SerializeField] private Color lockedColor = Color.red;

    private LevelData _levelData;
    private bool _isUnlocked = false;

    public System.Action<LevelData> OnLevelSelected;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (buttonBackground == null)
            buttonBackground = GetComponent<Image>();

        if (button != null)
            button.onClick.AddListener(OnButtonClicked);
    }

    public void Initialize(LevelData levelData)
    {
        _levelData = levelData;

        if (levelNameText != null)
            levelNameText.text = $"Level {GetLevelNumber(levelData)}";

        if (levelDescriptionText != null)
            levelDescriptionText.text = $"Waves: {levelData.wavesToWin}";

        _isUnlocked = IsLevelUnlocked(levelData);

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!_isUnlocked);

        if (button != null)
            button.interactable = _isUnlocked;

        UpdateButtonVisuals();
        UpdateCompletionStatus();
    }

    private int GetLevelNumber(LevelData levelData)
    {
        for (int i = 0; i < LevelManager.Instance.allLevels.Length; i++)
        {
            if (LevelManager.Instance.allLevels[i] == levelData)
                return i + 1;
        }
        return 1;
    }

    private bool IsLevelUnlocked(LevelData levelData)
    {
        if (levelData == LevelManager.Instance.allLevels[0])
            return true;

        int levelIndex = System.Array.IndexOf(LevelManager.Instance.allLevels, levelData);
        if (levelIndex <= 0) return true;

        LevelData previousLevel = LevelManager.Instance.allLevels[levelIndex - 1];
        string previousLevelKey = $"Level_{previousLevel.levelName}_Completed";
        bool unlocked = PlayerPrefs.GetInt(previousLevelKey, 0) == 1;
        Debug.Log($"{levelData.levelName} unlocked: {unlocked}");
        return unlocked;
    }

    private void UpdateCompletionStatus()
    {
        if (_levelData == null) return;

        string completionKey = $"Level_{_levelData.levelName}_Completed";
        bool isCompleted = PlayerPrefs.GetInt(completionKey, 0) == 1;

        if (completedOverlay != null)
            completedOverlay.SetActive(isCompleted);

        if (isCompleted)
        {
            int stars = PlayerPrefs.GetInt($"Level_{_levelData.levelName}_Stars", 1);
            UpdateStarDisplay(stars);
        }
    }

    private void UpdateStarDisplay(int stars)
    {
        for (int i = 0; i < starIcons.Length; i++)
        {
            if (starIcons[i] != null)
            {
                starIcons[i].color = i < stars ? Color.yellow : Color.gray;
                starIcons[i].gameObject.SetActive(true);
            }
        }
    }

    private void UpdateButtonVisuals()
    {
        if (buttonBackground != null)
        {
            buttonBackground.color = _isUnlocked ? normalColor : lockedColor;
        }
    }

    private void OnButtonClicked()
    {
        if (_isUnlocked && _levelData != null)
        {
            OnLevelSelected?.Invoke(_levelData);
        }
    }

    public void OnPointerClick(PointerEventData eventData) { }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isUnlocked && buttonBackground != null)
            buttonBackground.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UpdateButtonVisuals();
    }

    public void LoadLevel()
    {
        if (_isUnlocked && _levelData != null)
            LevelManager.Instance.LoadLevel(_levelData);
    }
}
