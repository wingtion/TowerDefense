using UnityEngine;

[CreateAssetMenu(fileName = "TowerData", menuName = "Scriptable Objects/TowerData")]
public class TowerData : ScriptableObject
{
    // In TowerData.cs, add these to the header:
    [Header("Tower Type Identification")]
    public bool isArcherTower = false;
    public bool isMagicTower = false;
    public bool isIceTower = false;
    public bool isStoneTower = false;

    [Header("Basic Tower Stats")]
    public float range;
    public float shootInterval;
    public float projectileSpeed;
    public float projectileDuration;
    public float damage;
    public int cost;
    public Sprite sprite;
    public GameObject prefab;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    [Min(1)] public int projectilePoolSize = 10;

    [Header("Special Tower Types")]
    [Header("Archer Tower")]
    public float archerShootInterval = 1f;

    [Header("Magic Tower")]
    public bool canSlowEnemies = false;
    public float slowAmount = 0.5f;
    public float slowDuration = 2f;

    public bool canChainLightning = false;
    public int chainCount = 3;
    public float chainDamageReduction = 0.7f;

    public bool isFireballTower = false;
    public float explosionRadius = 2f;
    public float explosionDamage = 8f;

    [Header("Ice Tower")]
    
    public float freezeRadius = 3f;
    public float freezeSlowAmount = 0.6f; // Stronger slow than regular magic tower
    public float freezeDuration = 3f;
    public float freezeInterval = 2f; // How often it applies freeze

    [Header("Upgrade Settings")]
    public TowerData nextLevelData;
    public int upgradeCost;
    public bool IsUpgradable => nextLevelData != null;

}