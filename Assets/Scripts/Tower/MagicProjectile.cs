using UnityEngine;

public class MagicFireballProjectile : MonoBehaviour
{
    [Header("Visual Effects")]
    public GameObject explosionEffect;
    public Animator fireballAnimator;

    [Header("Audio")]
    [SerializeField] private bool playExplosionSound = true;

    private Vector3 _direction;
    private float _currentDuration;
    private MagicTower _sourceTower;
    private ProjectilePooler _pooler;
    private bool _hasExploded = false;
    private float _damage;
    private float _explosionRadius;
    private float _explosionDamage;
    private float _speed;

    // Animation parameter IDs
    private int _impactHash;

    private void Awake()
    {
        _pooler = GetComponentInParent<ProjectilePooler>();
        if (fireballAnimator == null)
            fireballAnimator = GetComponent<Animator>();
        _impactHash = Animator.StringToHash("Impact");

        Debug.Log("MagicFireballProjectile Awake called");
    }

    private void Update()
    {
        if (_sourceTower == null)
        {
            Debug.LogError("Source tower is null!");
            return;
        }

        if (_hasExploded) return;

        // Move fireball - use cached speed
        transform.position += _direction * _speed * Time.deltaTime;

        // Rotate to face movement direction
        if (_direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }

        // Duration check
        _currentDuration -= Time.deltaTime;
        if (_currentDuration <= 0 && !_hasExploded)
        {
            Debug.Log("Duration expired, exploding");
            Explode();
        }
    }

    public void Shoot(MagicTower tower, Vector3 direction, float damage, float explosionRadius, float explosionDamage)
    {
        Debug.Log($"Shoot called: tower={tower != null}, direction={direction}, damage={damage}, explosionRadius={explosionRadius}");

        _sourceTower = tower;
        _direction = direction.normalized;
        _damage = damage;
        _explosionRadius = explosionRadius;
        _explosionDamage = explosionDamage;
        _speed = tower.Data.projectileSpeed;
        _currentDuration = tower.Data.projectileDuration;
        _hasExploded = false;

        // Reset position and rotation
        transform.rotation = Quaternion.identity;

        if (fireballAnimator != null)
        {
            fireballAnimator.ResetTrigger(_impactHash);
            // Force play the fireball animation
            fireballAnimator.Play("FireBall", -1, 0f);
        }

        Debug.Log($"Projectile initialized: speed={_speed}, duration={_currentDuration}, damage={_damage}");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_hasExploded) return;

        Debug.Log($"Trigger entered: {collision.tag}");

        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log($"Hit enemy: {enemy.name}, applying damage: {_damage}");
                enemy.TakeDamage(_damage);
                Explode();
            }
        }
    }

    private void Explode()
    {
        if (_hasExploded) return;
        _hasExploded = true;

        Debug.Log("Exploding!");

        // Play explosion sound
        if (playExplosionSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayExplosion();
        }

        // Stop movement
        _speed = 0f;

        // Trigger impact animation
        if (fireballAnimator != null)
        {
            fireballAnimator.SetTrigger(_impactHash);
        }

        // Apply area damage
        ApplyExplosionDamage();

        // Spawn explosion effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Return to pool after delay
        Invoke("ReturnToPool", 1f);
    }

    private void ApplyExplosionDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, _explosionRadius);
        Debug.Log($"Explosion hit {hitEnemies.Length} colliders");

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (enemyCollider.CompareTag("Enemy") && enemyCollider.gameObject != null)
            {
                Enemy enemy = enemyCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Debug.Log($"Explosion damage to {enemy.name}: {_explosionDamage}");
                    enemy.TakeDamage(_explosionDamage);
                }
            }
        }
    }

    private void ReturnToPool()
    {
        Debug.Log("Returning to pool");

        if (_pooler != null)
        {
            transform.rotation = Quaternion.identity;
            _pooler.ReturnProjectile(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        Debug.Log("MagicFireballProjectile enabled");
        _hasExploded = false;

        // Reset speed in case it was set to 0 during explosion
        if (_sourceTower != null)
        {
            _speed = _sourceTower.Data.projectileSpeed;
        }
    }

    private void OnDisable()
    {
        Debug.Log("MagicFireballProjectile disabled");

        _hasExploded = false;
        CancelInvoke("ReturnToPool");

        if (fireballAnimator != null)
        {
            fireballAnimator.ResetTrigger(_impactHash);
        }
    }

    // ANIMATION EVENT - Call this from your Impact animation
    public void OnImpactAnimationComplete()
    {
        Debug.Log("Impact animation complete");
        ReturnToPool();
    }

    // Visual debug for explosion radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}