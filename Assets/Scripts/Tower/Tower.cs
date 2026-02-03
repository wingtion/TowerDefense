using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private TowerData data;
    public TowerData Data => data; // Public getter

    [SerializeField] protected ProjectilePooler projectilePooler;
    protected CircleCollider2D _circleCollider;
    protected List<Enemy> _enemiesInRange;
    protected Archer[] _archers;
    protected float _shootTimer; // Add this line

    // Gizmo settings
    [Header("Gizmo Settings")]
    [SerializeField] private bool showRangeGizmo = true;
    [SerializeField] private Color rangeGizmoColor = Color.cyan;
    [SerializeField] private Color enemiesInRangeColor = Color.red;

    // Add this flag to track if tower can shoot projectiles
    protected bool _canShootProjectiles = false;

    // Track the last targeted position to prevent shooting at empty spots
    private Vector3 _lastTargetPosition;
    private bool _hasValidTarget = false;

    private void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    protected virtual void Start()
    {
        _circleCollider = GetComponent<CircleCollider2D>();
        _circleCollider.radius = data.range;
        _enemiesInRange = new List<Enemy>();
        _shootTimer = data.shootInterval;

        // Only initialize projectile pooler if we have a projectile prefab
        if (data.projectilePrefab != null)
        {
            _canShootProjectiles = true;

            // Initialize projectile pooler if not assigned
            if (projectilePooler == null)
            {
                projectilePooler = gameObject.AddComponent<ProjectilePooler>();
                projectilePooler.projectilePrefab = data.projectilePrefab;
                projectilePooler.poolSize = data.projectilePoolSize;
            }

            // Get all archer components and initialize them if they exist
            _archers = GetComponentsInChildren<Archer>();
            foreach (Archer archer in _archers)
            {
                if (archer != null)
                    archer.Initialize(projectilePooler, data);
            }
        }
        else
        {
            _canShootProjectiles = false;
        }
    }

    protected virtual void Update()
    {
        // Clean up null enemies - IMPROVED CLEANUP
        CleanupEnemiesList();

        if (_enemiesInRange.Count == 0)
        {
            _hasValidTarget = false;
            return;
        }

        // Only handle projectile shooting if this tower can shoot
        if (_canShootProjectiles)
        {
            // If tower has archers, use them
            if (_archers != null && _archers.Length > 0)
            {
                foreach (Archer archer in _archers)
                {
                    if (archer != null)
                    {
                        Enemy target = GetValidTarget();
                        if (target != null)
                        {
                            archer.HandleShooting(target);
                        }
                    }
                }
            }
            else
            {
                // Fallback: direct shooting from tower center
                _shootTimer -= Time.deltaTime;
                if (_shootTimer <= 0)
                {
                    Enemy target = GetValidTarget();
                    if (target != null)
                    {
                        _shootTimer = data.shootInterval;

                        // Safety check for projectile pooler
                        if (projectilePooler != null)
                        {
                            GameObject projectile = projectilePooler.GetProjectile();
                            if (projectile != null)
                            {
                                projectile.transform.position = transform.position;
                                projectile.SetActive(true);

                                Vector2 direction = (target.transform.position - transform.position).normalized;
                                Projectile projectileComponent = projectile.GetComponent<Projectile>();
                                if (projectileComponent != null)
                                {
                                    projectileComponent.Shoot(data, direction);
                                }
                            }
                        }
                    }
                }
            }
        }
        // If tower can't shoot projectiles, derived classes (like IceTower) will handle their own logic
    }

    // IMPROVED: Better enemy cleanup method
    private void CleanupEnemiesList()
    {
        for (int i = _enemiesInRange.Count - 1; i >= 0; i--)
        {
            if (_enemiesInRange[i] == null ||
                !_enemiesInRange[i].gameObject.activeInHierarchy ||
                _enemiesInRange[i].transform == null)
            {
                _enemiesInRange.RemoveAt(i);
            }
        }
    }

    // NEW: Get a valid target with additional checks
    private Enemy GetValidTarget()
    {
        foreach (Enemy enemy in _enemiesInRange)
        {
            if (enemy != null &&
                enemy.gameObject.activeInHierarchy &&
                enemy.transform != null &&
                Vector3.Distance(transform.position, enemy.transform.position) <= data.range)
            {
                _hasValidTarget = true;
                _lastTargetPosition = enemy.transform.position;
                return enemy;
            }
        }

        _hasValidTarget = false;
        return null;
    }

    public bool CanUpgrade()
    {
        return data != null && data.IsUpgradable;
    }

    public TowerData GetNextLevelData()
    {
        return data?.nextLevelData;
    }

    public int GetUpgradeCost()
    {
        return data?.upgradeCost ?? 0;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && !_enemiesInRange.Contains(enemy))
            {
                _enemiesInRange.Add(enemy);
            }
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                _enemiesInRange.Remove(enemy);
            }
        }
    }

    private void HandleEnemyDestroyed(Enemy enemy)
    {
        if (_enemiesInRange.Contains(enemy))
        {
            _enemiesInRange.Remove(enemy);
        }
    }

    public bool HasTarget()
    {
        CleanupEnemiesList();
        return _enemiesInRange.Count > 0;
    }

    public Enemy GetFirstEnemy()
    {
        CleanupEnemiesList();
        return _enemiesInRange.Count > 0 ? _enemiesInRange[0] : null;
    }

    // ========== GIZMOS ==========
    private void OnDrawGizmos()
    {
        if (!showRangeGizmo) return;

        // Draw tower range circle
        float range = data != null ? data.range : 5f; // Default if no data
        DrawRangeGizmo(range);

        // Draw lines to enemies in range (in Scene view during play mode)
        if (Application.isPlaying)
        {
            DrawEnemyTargetingLines();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // When selected, always show range even if showRangeGizmo is false
        float range = data != null ? data.range : 5f;
        DrawRangeGizmo(range, true); // Thicker when selected

        if (Application.isPlaying)
        {
            DrawEnemyTargetingLines();
        }
    }

    private void DrawRangeGizmo(float range, bool isSelected = false)
    {
        Gizmos.color = rangeGizmoColor;

        // Set alpha based on selection
        Color gizmoColor = rangeGizmoColor;
        gizmoColor.a = isSelected ? 0.3f : 0.2f; // More opaque when selected
        Gizmos.color = gizmoColor;

        // Draw filled circle for range
        DrawCircle(transform.position, range, 32);

        // Draw outline
        gizmoColor.a = isSelected ? 0.8f : 0.5f;
        Gizmos.color = gizmoColor;
        DrawCircleOutline(transform.position, range, 32);
    }

    private void DrawEnemyTargetingLines()
    {
        if (_enemiesInRange == null || _enemiesInRange.Count == 0) return;

        Gizmos.color = enemiesInRangeColor;

        foreach (Enemy enemy in _enemiesInRange)
        {
            if (enemy != null)
            {
                // Draw line from tower to enemy
                Gizmos.DrawLine(transform.position, enemy.transform.position);

                // Draw small circle around enemy to highlight it's targeted
                Gizmos.DrawWireSphere(enemy.transform.position, 0.3f);
            }
        }

        // Highlight the first enemy (primary target)
        Enemy firstEnemy = GetFirstEnemy();
        if (firstEnemy != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(firstEnemy.transform.position, 0.4f);

            // Thicker line to primary target
            Gizmos.DrawLine(transform.position, firstEnemy.transform.position);
        }
    }

    // Helper method to draw a filled circle
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
#if UNITY_EDITOR
        // For filled circle, we use Handles for better visualization
        UnityEditor.Handles.color = Gizmos.color;
        UnityEditor.Handles.DrawSolidDisc(center, Vector3.forward, radius);
#endif
    }

    // Helper method to draw circle outline
    private void DrawCircleOutline(Vector3 center, float radius, int segments)
    {
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}