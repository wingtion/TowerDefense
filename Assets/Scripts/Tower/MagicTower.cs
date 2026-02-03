using UnityEngine;
using System.Collections;

public class MagicTower : Tower
{
    [Header("Tower Animation")]
    public Animator towerAnimator;
    public string shootAnimationTrigger = "Shoot";
    public float animationDelay = 0.1f;

    [Header("Audio")]
    [SerializeField] private bool playShootSound = true;

    private float _magicShootTimer;
    private bool _isPlayingAnimation = false;
    private int _shootHash;

    protected override void Start()
    {
        base.Start();
        _magicShootTimer = Data.shootInterval;
        _shootHash = Animator.StringToHash(shootAnimationTrigger);

        if (towerAnimator == null)
            towerAnimator = GetComponentInChildren<Animator>();

        Debug.Log($"MagicTower started: isFireballTower={Data.isFireballTower}, projectileSpeed={Data.projectileSpeed}");
    }

    protected override void Update()
    {
        // Clean up null enemies
        _enemiesInRange.RemoveAll(e => e == null);

        if (_enemiesInRange.Count == 0 || _isPlayingAnimation) return;

        _magicShootTimer -= Time.deltaTime;
        if (_magicShootTimer <= 0)
        {
            _magicShootTimer = Data.shootInterval;
            StartShootingAnimation();
        }
    }

    private void StartShootingAnimation()
    {
        if (_isPlayingAnimation) return;
        _isPlayingAnimation = true;

        Debug.Log("Starting shooting animation");

        if (towerAnimator != null)
        {
            towerAnimator.SetTrigger(_shootHash);
        }
        else
        {
            Debug.LogError("Tower Animator not found!");
        }

        StartCoroutine(ShootAfterAnimationDelay());
    }

    private IEnumerator ShootAfterAnimationDelay()
    {
        yield return new WaitForSeconds(animationDelay);

        Debug.Log("Animation delay complete, shooting projectile");

        // Play magic shoot sound
        if (playShootSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMagicShoot();
        }

        if (Data.canChainLightning && _enemiesInRange.Count > 1)
        {
            Debug.Log("Using chain lightning");
            StartCoroutine(ChainLightningCoroutine());
        }
        else if (Data.isFireballTower)
        {
            Debug.Log("Shooting fireball");
            ShootFireball();
        }
        else
        {
            Debug.Log("No special ability, using default behavior");
            _isPlayingAnimation = false;
        }

        if (!HasAnimationEvent())
        {
            StartCoroutine(ResetAnimationState());
        }
    }

    private bool HasAnimationEvent()
    {
        return false;
    }

    private IEnumerator ResetAnimationState()
    {
        yield return new WaitForSeconds(0.5f);
        _isPlayingAnimation = false;
    }

    private void ShootFireball()
    {
        GameObject projectile = projectilePooler.GetProjectile();
        Debug.Log($"ShootFireball: projectile={projectile != null}, enemiesInRange={_enemiesInRange.Count}");

        if (projectile != null && _enemiesInRange.Count > 0)
        {
            projectile.transform.position = transform.position;
            projectile.SetActive(true);

            Vector2 direction = (_enemiesInRange[0].transform.position - transform.position).normalized;
            Debug.Log($"Target enemy: {_enemiesInRange[0].name}, direction={direction}");

            MagicFireballProjectile fireball = projectile.GetComponent<MagicFireballProjectile>();
            if (fireball != null)
            {
                Debug.Log($"Fireball component found, initializing with damage={Data.damage}, explosionRadius={Data.explosionRadius}");

                // Pass all data from TowerData
                fireball.Shoot(
                    this,
                    direction,
                    Data.damage,
                    Data.explosionRadius,
                    Data.explosionDamage
                );
            }
            else
            {
                Debug.LogError("MagicFireballProjectile component not found on projectile!");
                _isPlayingAnimation = false;
            }
        }
        else
        {
            Debug.LogError($"Cannot shoot fireball: projectile null={projectile == null}, no enemies={_enemiesInRange.Count == 0}");
            _isPlayingAnimation = false;
        }
    }

    private IEnumerator ChainLightningCoroutine()
    {
        if (_enemiesInRange.Count < 2)
        {
            _isPlayingAnimation = false;
            yield break;
        }

        Debug.Log($"Starting chain lightning: {_enemiesInRange.Count} enemies");

        int actualChainCount = Mathf.Min(Data.chainCount, _enemiesInRange.Count);
        float currentDamage = Data.damage;

        for (int i = 0; i < actualChainCount; i++)
        {
            if (i >= _enemiesInRange.Count || _enemiesInRange[i] == null) break;

            Debug.Log($"Chain lightning hit {i}: damage={currentDamage}");
            _enemiesInRange[i].TakeDamage(currentDamage);
            currentDamage *= Data.chainDamageReduction;
            yield return new WaitForSeconds(0.1f);
        }

        _isPlayingAnimation = false;
    }

    public void ApplySlowEffect(Enemy enemy)
    {
        if (Data.canSlowEnemies)
        {
            StartCoroutine(SlowEnemyCoroutine(enemy));
        }
    }

    private IEnumerator SlowEnemyCoroutine(Enemy enemy)
    {
        if (enemy == null) yield break;
        yield return new WaitForSeconds(Data.slowDuration);
    }

    // ANIMATION EVENT METHOD - Call this from your shoot animation
    public void OnShootAnimationEvent()
    {
        Debug.Log("Shoot animation event triggered");

        // Play magic shoot sound when animation event is called
        if (playShootSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMagicShoot();
        }

        if (Data.canChainLightning && _enemiesInRange.Count > 1)
        {
            StartCoroutine(ChainLightningCoroutine());
        }
        else if (Data.isFireballTower)
        {
            ShootFireball();
        }
        else
        {
            _isPlayingAnimation = false;
        }
    }

    // ANIMATION EVENT METHOD - Call this when animation completes
    public void OnShootAnimationComplete()
    {
        Debug.Log("Shoot animation complete");
        _isPlayingAnimation = false;
    }
}