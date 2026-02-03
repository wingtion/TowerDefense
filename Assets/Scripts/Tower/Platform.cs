using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Platform : MonoBehaviour
{
    public static event Action<Platform> OnPlatformClicked;
    public static event Action<Platform> OnTowerSelected;

    [SerializeField] private LayerMask platformLayerMask;
    public static bool towerPanelOpen { get; set; } = false;
    public static bool upgradePanelOpen { get; set; } = false;

    public bool HasTower { get; private set; } = false;
    public Tower CurrentTower { get; private set; }

    void Update()
    {
        if (towerPanelOpen || upgradePanelOpen || Time.timeScale == 0f)
        {
            return;
        }

        // Handle both mouse and touch input
        bool inputDetected = false;
        Vector2 worldPoint = Vector2.zero;

        // Mouse input
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            inputDetected = true;
        }
        // Touch input
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            worldPoint = Camera.main.ScreenToWorldPoint(Touchscreen.current.primaryTouch.position.ReadValue());
            inputDetected = true;
        }

        if (inputDetected)
        {
            RaycastHit2D raycastHit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, platformLayerMask);

            if (raycastHit.collider != null)
            {
                Platform platform = raycastHit.collider.GetComponent<Platform>();
                if (platform != null)
                {
                    if (!platform.HasTower)
                    {
                        OnPlatformClicked?.Invoke(platform);
                    }
                    else
                    {
                        OnTowerSelected?.Invoke(platform);
                    }
                }
            }
        }
    }

    public void PlaceTower(TowerData data)
    {
        GameObject towerObj = Instantiate(data.prefab, transform.position, Quaternion.identity, transform);

        // Counter the parent's scale
        towerObj.transform.localScale = new Vector3(
            1f / transform.localScale.x,
            1f / transform.localScale.y,
            1f / transform.localScale.z
        );

        // Force local position to zero
        towerObj.transform.localPosition = Vector3.zero;

        CurrentTower = towerObj.GetComponent<Tower>();
        HasTower = true;

        //only inactive the sprite renderer so collider and script stays active
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;
    }

    public void UpgradeTower()
    {
        if (HasTower && CurrentTower != null && CurrentTower.CanUpgrade())
        {
            TowerData nextLevelData = CurrentTower.GetNextLevelData();
            if (nextLevelData != null)
            {
                // Remove current tower
                Destroy(CurrentTower.gameObject);
                CurrentTower = null;
                HasTower = false;

                // Place upgraded tower
                PlaceTower(nextLevelData);
            }
        }
    }

    public int GetSellValue()
    {
        if (!HasTower || CurrentTower == null) return 0;

        TowerData towerData = CurrentTower.Data;
        if (towerData == null) return 0;

        // Calculate total investment (tower cost + upgrade costs)
        int totalInvestment = towerData.cost;

        // If this is an upgraded tower, add the previous level costs
        TowerData currentData = towerData;
        while (currentData != null)
        {
            // Check if this tower has a previous level (you might need to add a previousLevelData field)
            // For now, we'll use a simple approach
            if (currentData.nextLevelData != null && currentData.nextLevelData != towerData)
            {
                totalInvestment += currentData.upgradeCost;
            }
            break; // Simplified - you might want more complex logic
        }

        // Return 40% of total investment as sell value
        return Mathf.RoundToInt(totalInvestment * 0.4f);
    }

    public void SellTower()
    {
        if (!HasTower || CurrentTower == null) return;

        // Get sell value before destroying the tower
        int sellValue = GetSellValue();

        // Destroy the tower
        Destroy(CurrentTower.gameObject);
        CurrentTower = null;
        HasTower = false;

        // Re-enable the platform sprite
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = true;

        // Return gold to player (this will be called from UIController)
        GameManager.Instance.AddGold(sellValue);
    }

    public bool CanUpgradeTower()
    {
        return HasTower && CurrentTower != null && CurrentTower.CanUpgrade();
    }

    public int GetUpgradeCost()
    {
        return HasTower && CurrentTower != null ? CurrentTower.GetUpgradeCost() : 0;
    }

    public TowerData GetTowerData()
    {
        return HasTower && CurrentTower != null ? CurrentTower.Data : null;
    }
}