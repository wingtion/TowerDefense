using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerCard : MonoBehaviour
{
    [SerializeField] private Image towerImage;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text nameText;

    private TowerData _towerData;
    public static event Action<TowerData> OnTowerSelected;

    public void Initiliaze(TowerData data)
    {
        _towerData = data;
        towerImage.sprite = data.sprite;
        costText.text = data.cost.ToString();
        nameText.text = FormatTowerName(data.name);
    }

    private string FormatTowerName(string originalName)
    {
        // Remove level indicators first
        string formattedName = originalName
            .Replace("_Lvl1", "")
            .Replace("_Lvl2", "")
            .Replace("_Lvl3", "");

        // Add spaces before capital letters (except first character)
        formattedName = System.Text.RegularExpressions.Regex.Replace(
            formattedName,
            @"(\B[A-Z])",
            " $1"
        );

        return formattedName;
    }

    public void PlaceTower()
    {
        OnTowerSelected?.Invoke(_towerData);
    }
}