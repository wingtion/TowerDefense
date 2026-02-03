using UnityEngine;

public class Archer : MonoBehaviour
{
    [SerializeField] private float shootInterval = 1f;
    [SerializeField] private string shootTriggerParameter = "Shoot";
    [SerializeField] private bool faceRightByDefault = true;

    // Add sound field
    [Header("Audio")]
    [SerializeField] private bool playShootSound = true;

    private ProjectilePooler _projectilePooler;
    private TowerData _towerData;
    private float _shootTimer;
    private Animator _animator;
    private bool _isShooting = false;
    private Vector3 _originalScale;
    private Enemy _currentTarget;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _originalScale = transform.localScale;
    }

    public void Initialize(ProjectilePooler projectilePooler, TowerData towerData)
    {
        _projectilePooler = projectilePooler;
        _towerData = towerData;
        _shootTimer = shootInterval;
    }

    private void Update()
    {
        if (_shootTimer > 0)
        {
            _shootTimer -= Time.deltaTime;
        }
    }

    public void HandleShooting(Enemy target)
    {
        if (target == null) return;
        if (_shootTimer > 0) return;
        if (_isShooting) return;

        StartShooting(target);
    }

    private void StartShooting(Enemy target)
    {
        _isShooting = true;
        _currentTarget = target;

        // Face the target
        FaceTarget(target.transform.position);

        // Trigger the shoot animation
        if (_animator != null)
        {
            _animator.SetTrigger(shootTriggerParameter);
        }

        _shootTimer = shootInterval;
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;

        if (direction.x < 0) // Target is to the left
        {
            if (faceRightByDefault)
            {
                transform.localScale = new Vector3(-Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
            }
            else
            {
                transform.localScale = _originalScale;
            }
        }
        else // Target is to the right
        {
            if (faceRightByDefault)
            {
                transform.localScale = _originalScale;
            }
            else
            {
                transform.localScale = new Vector3(Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
            }
        }
    }

    // Animation Event - Call this from your animation at the exact frame when arrow should be released
    public void OnShootEvent()
    {
        if (_currentTarget != null)
        {
            Shoot(_currentTarget);
        }
    }

    // Animation Event - Call this when animation ends
    public void OnAnimationEnd()
    {
        _isShooting = false;
        _currentTarget = null;
    }

    private void Shoot(Enemy target)
    {
        if (_projectilePooler == null || _towerData == null || target == null) return;

        // Play shoot sound
        if (playShootSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayArcherShoot();
        }

        GameObject projectile = _projectilePooler.GetProjectile();
        if (projectile == null) return;

        projectile.transform.position = transform.position;
        projectile.SetActive(true);

        Vector2 direction = (target.transform.position - transform.position).normalized;
        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent != null)
        {
            projectileComponent.Shoot(_towerData, direction);
        }
    }
}