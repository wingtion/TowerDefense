using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IceTower : Tower
{
    [Header("Ice Tower Visuals")]
    public GameObject freezeAreaEffect; // Assign your FreezeArea prefab here
    public string freezeAnimationTrigger = "Freeze";

    [Header("Freeze Area Scaling")]
    public float freezeAreaScaleMultiplier = 0.7f;

    private float _freezeTimer;
    private bool _isFreezing = false;
    private GameObject _currentFreezeArea; // Only one freeze area at a time
    private Animator _freezeAreaAnimator;
    private List<Enemy> _frozenEnemies = new List<Enemy>();

    [Header("Audio")]
    [SerializeField] private bool playFreezeSound = true;

    protected override void Start()
    {
        base.Start();
        _freezeTimer = Data.freezeInterval;
    }

    protected override void Update()
    {
        // Clean up null enemies
        _enemiesInRange.RemoveAll(e => e == null);
        _frozenEnemies.RemoveAll(e => e == null);

        // Handle freezing logic - only at regular intervals
        if (!_isFreezing && _enemiesInRange.Count > 0)
        {
            _freezeTimer -= Time.deltaTime;
            if (_freezeTimer <= 0)
            {
                StartFreeze();
            }
        }
    }

    private void StartFreeze()
    {
        if (_isFreezing || _enemiesInRange.Count == 0) return;

        _isFreezing = true;
        _freezeTimer = Data.freezeInterval; // Reset timer for next freeze

        Debug.Log("Starting freeze effect on enemies");

        // Play freeze sound
        if (playFreezeSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayIceFreeze();
        }

        // Clear previous frozen enemies list
        _frozenEnemies.Clear();

        // Apply freeze and create ONE visual effect
        ApplyFreezeToEnemies();

        // Reset freezing state after animation duration
        StartCoroutine(ResetFreezeState(1.5f));
    }

    private void ApplyFreezeToEnemies()
    {
        // Remove any existing freeze area
        if (_currentFreezeArea != null)
        {
            Destroy(_currentFreezeArea);
        }

        // Apply slow to all enemies currently in range
        foreach (Enemy enemy in _enemiesInRange)
        {
            if (enemy != null)
            {
                // Only apply slow if enemy is not already frozen
                if (!_frozenEnemies.Contains(enemy))
                {
                    ApplySlowToEnemy(enemy);
                    _frozenEnemies.Add(enemy);
                }
            }
        }

        // Create ONE freeze area centered on the enemy group
        CreateFreezeAreaCenteredOnEnemies();

        Debug.Log($"Frozen {_frozenEnemies.Count} enemies with single freeze area");
    }
    private void CreateFreezeAreaCenteredOnEnemies()
    {
        if (freezeAreaEffect != null && _enemiesInRange.Count > 0)
        {
            // Calculate center point of all enemies
            Vector3 center = Vector3.zero;
            int validEnemies = 0;

            foreach (Enemy enemy in _enemiesInRange)
            {
                if (enemy != null)
                {
                    center += enemy.transform.position;
                    validEnemies++;
                }
            }

            if (validEnemies > 0)
            {
                center /= validEnemies;

                // Create ONE large freeze area in the center
                _currentFreezeArea = Instantiate(freezeAreaEffect, center, Quaternion.identity);
                _freezeAreaAnimator = _currentFreezeArea.GetComponent<Animator>();

                // Scale based on freeze radius
                float scale = Data.freezeRadius * freezeAreaScaleMultiplier;
                _currentFreezeArea.transform.localScale = Vector3.one * scale;

                if (_freezeAreaAnimator != null)
                {
                    _freezeAreaAnimator.SetTrigger(freezeAnimationTrigger);
                }
            }
        }
    }

    private void ApplySlowToEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            enemy.ApplySlow(Data.freezeSlowAmount, Data.freezeDuration);
        }
    }

    private IEnumerator ResetFreezeState(float delay)
    {
        yield return new WaitForSeconds(delay);
        _isFreezing = false;

        // Optional: Clear the frozen enemies list after freeze duration
        // This prevents new enemies from getting special treatment
        StartCoroutine(ClearFrozenEnemiesAfterDuration(Data.freezeDuration));
    }

    private IEnumerator ClearFrozenEnemiesAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        _frozenEnemies.Clear();
    }

    // Handle new enemies entering range - BUT DON'T FREEZE THEM IMMEDIATELY
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        // New enemies will be handled in the next regular freeze interval
        // This prevents spam and makes the tower balanced
    }

    // Visual debug for freeze radius
    private void OnDrawGizmosSelected()
    {
        if (Data != null && Data.isIceTower)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, Data.freezeRadius);
        }
    }
}