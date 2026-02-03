using System.Collections;
using UnityEngine;

public class ExplosiveProjectile : MonoBehaviour
{
    [Header("Explosion Settings")]
    [HideInInspector] public float explosionRadius;
    [HideInInspector] public float explosionDamage;
    public bool explodeOnContact = true;

    [Header("Movement Settings")]
    public float projectileSpeed = 10f;
    public float projectileDuration = 3f;

    [Header("Audio")]
    [SerializeField] private bool playExplosionSound = true;

    // Private variables
    private Vector3 _shootDirection;
    private float _currentDuration;
    protected bool _hasExploded = false;
    private Animator _animator;
    private ProjectilePooler _pooler;
    private float _originalProjectileSpeed;
    private Coroutine _durationCoroutine;
    private bool _isInitialized = false;

    // Animation hash'leri için
    private static readonly int ImpactHash = Animator.StringToHash("Impact");
    private static readonly int IdleHash = Animator.StringToHash("Stone");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _pooler = GetComponentInParent<ProjectilePooler>();
        _originalProjectileSpeed = projectileSpeed;
    }

    public void Initialize(TowerData data, Vector3 direction, float duration)
    {
        // Eðer zaten aktif deðilse, hiçbir þey yapma
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Initialize called on inactive projectile");
            return;
        }

        // Önceki state'i temizle
        CleanupPreviousState();

        // Yeni deðerleri ayarla
        _shootDirection = direction.normalized;
        _currentDuration = duration;
        _hasExploded = false;
        _isInitialized = true;

        // TowerData'dan deðerleri al
        if (data != null)
        {
            projectileSpeed = data.projectileSpeed;
            explosionDamage = data.damage;
            explosionRadius = data.explosionRadius;
        }

        // Animator'ý baþlangýç state'ine getir - GÜNCELLENMÝÞ
        ResetAnimator();

        // Duration coroutine'ini baþlat
        _durationCoroutine = StartCoroutine(DurationCountdown());

        Debug.Log($"ExplosiveProjectile initialized: speed={projectileSpeed}, duration={_currentDuration}, damage={explosionDamage}");
    }

    private void CleanupPreviousState()
    {
        // Önceki coroutine'i durdur
        if (_durationCoroutine != null)
        {
            StopCoroutine(_durationCoroutine);
            _durationCoroutine = null;
        }

        // Hareketi durdur
        projectileSpeed = _originalProjectileSpeed;
    }

    public void ResetAnimator()
    {
        if (_animator != null && gameObject.activeInHierarchy)
        {
            try
            {
                // Animator'ý tamamen sýfýrla
                _animator.Rebind();
                _animator.Update(0f);

                // Tüm trigger'larý temizle
                _animator.ResetTrigger(ImpactHash);

                // Doðrudan baþlangýç state'ine geç
                _animator.Play(IdleHash, 0, 0f);

                // Bir frame güncelleme yap
                _animator.Update(0.01f);

                Debug.Log("Animator successfully reset to Idle state");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error resetting animator: {e.Message}");
            }
        }
    }

    private IEnumerator DurationCountdown()
    {
        float timer = _currentDuration;

        while (timer > 0 && !_hasExploded && _isInitialized)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        if (!_hasExploded && _isInitialized)
        {
            Debug.Log("Duration expired, exploding");
            Explode();
        }
    }

    private void Update()
    {
        if (_hasExploded || !_isInitialized) return;

        // Projectile'ý hareket ettir
        transform.position += _shootDirection * projectileSpeed * Time.deltaTime;

        // Yönüne doðru döndür
        if (_shootDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(_shootDirection.y, _shootDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
    }

    protected void Explode()
    {
        if (_hasExploded || !_isInitialized) return;

        _hasExploded = true;
        _isInitialized = false;

        Debug.Log($"Explode called");

        // Play explosion sound
        if (playExplosionSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayExplosion();
        }

        // Coroutine'i durdur
        if (_durationCoroutine != null)
        {
            StopCoroutine(_durationCoroutine);
            _durationCoroutine = null;
        }

        // Hareketi durdur
        projectileSpeed = 0f;

        // Hasar uygula
        ApplyExplosionDamage();

        // Animasyonu baþlat
        if (_animator != null && gameObject.activeInHierarchy)
        {
            Debug.Log("Setting Impact trigger...");
            _animator.SetTrigger(ImpactHash);

            // Animasyonun baþlayýp baþlamadýðýný kontrol etmek için coroutine baþlat
            StartCoroutine(WaitForAnimationStart());
        }
        else
        {
            Debug.LogWarning("Animator not found or object inactive, returning to pool directly");
            ReturnToPool();
        }
    }

    private IEnumerator WaitForAnimationStart()
    {
        // 3 frame bekleyelim (animator'ýn tetiði iþlemesi için yeterli süre)
        for (int i = 0; i < 3; i++)
        {
            yield return null;
        }

        // Hala Impact state'inde deðilse, animasyon oynamýyor demektir
        if (_animator != null && gameObject.activeInHierarchy)
        {
            bool isInImpactState = _animator.GetCurrentAnimatorStateInfo(0).IsName("Impact");
            Debug.Log($"Animator state after 3 frames: {isInImpactState}");

            if (!isInImpactState)
            {
                Debug.LogError("Impact animation failed to play! Check animator transitions and state names.");
                // Animasyon baþlamadýysa, direkt pool'a dön
                ReturnToPool();
            }
        }
    }

    private void ApplyExplosionDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            if (enemyCollider.CompareTag("Enemy") && enemyCollider.gameObject != null)
            {
                Enemy enemy = enemyCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(explosionDamage);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!explodeOnContact || _hasExploded || !_isInitialized) return;

        if (collision.CompareTag("Enemy"))
        {
            Debug.Log($"Hit enemy: {collision.name}, exploding on contact");
            Explode();
        }
    }

    private void ReturnToPool()
    {
        if (!gameObject.activeInHierarchy) return;

        Debug.Log("ExplosiveProjectile returning to pool");

        // Tüm state'leri temizle
        CleanupPreviousState();

        _hasExploded = false;
        _isInitialized = false;
        _shootDirection = Vector3.zero;
        transform.rotation = Quaternion.identity;

        // Call the full reset method
        ResetAnimator();

        // Pool'a döndür
        if (_pooler != null)
        {
            _pooler.ReturnProjectile(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void OnImpactAnimationComplete()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[EVENT] {name} received ImpactAnimationComplete but it's already inactive!");
            return;
        }

        Debug.Log($"[EVENT] Impact animation complete on {name}");
        ReturnToPool();
    }

    private void OnEnable()
    {
        Debug.Log("ExplosiveProjectile enabled - waiting for initialization");

        // Burada sadece temel deðiþkenleri sýfýrla
        _hasExploded = false;
        _shootDirection = Vector3.zero;
        transform.rotation = Quaternion.identity;

        // Initialize metodu çaðrýlana kadar hareket etme
        _isInitialized = false;

        // Animator'ý hemen sýfýrlama - Initialize'de yapýlacak
    }

    private void OnDisable()
    {
        Debug.Log("ExplosiveProjectile disabled - cleaning up");
        // Sadece coroutine'leri temizle, animator'a dokunma
        CleanupPreviousState();
        _isInitialized = false;
        _hasExploded = false;
    }

    // Visual debug for explosion radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}