using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    public EnemyData Data => data;

    public static event Action<EnemyData> OnEnemyReachedEnd;
    public static event Action<Enemy> OnEnemyDestroyed;

    private Path _currentPath;

    private Vector3 _targetPosition;
    private int _currentWayPoint;

    private float _lives;
    private float _maxLives;

    [SerializeField] private Transform healthBar;
    private Vector3 _healthBarOriginalScale;

    private bool _hasBeenCounted = false;

    private float _currentSpeed;
    private float _originalSpeed;
    private Coroutine _slowCoroutine;

    private SpriteRenderer _spriteRenderer; // Flash efekti için
    private Color _originalColor;

    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;

    private void Awake()
    {
        _currentPath = GameObject.Find("Path1").GetComponent<Path>();
        _healthBarOriginalScale = healthBar.localScale;

        _originalSpeed = data.speed;
        _currentSpeed = _originalSpeed;

        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
    }

    private void OnEnable()
    {
        _currentWayPoint = 0;
        _targetPosition = _currentPath.GetPosition(0);
        _hasBeenCounted = false;

        _currentSpeed = _originalSpeed;


        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor; // Rengi spawn baþýnda sýfýrla
            _originalColor = _spriteRenderer.color; // Orijinal rengi güncelle
        }
    }


    void Update()
    {
        if (_hasBeenCounted) return;

        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _currentSpeed * Time.deltaTime);

        if ((transform.position - _targetPosition).magnitude < 0.1f)
        {
            if (_currentWayPoint < _currentPath.Waypoints.Length - 1)
            {
                _currentWayPoint++;
                _targetPosition = _currentPath.GetPosition(_currentWayPoint);
            }
            else
            {
                _hasBeenCounted = true;
                OnEnemyReachedEnd?.Invoke(data);
                Despawn();
            }
        }
    }

    public void ApplySlow(float slowAmount, float duration)
    {
        if (_slowCoroutine != null)
        {
            StopCoroutine(_slowCoroutine);
        }

        _slowCoroutine = StartCoroutine(SlowCoroutine(slowAmount, duration));
    }

    private IEnumerator SlowCoroutine(float slowAmount, float duration)
    {
        _currentSpeed = _originalSpeed * (1f - slowAmount);
        yield return new WaitForSeconds(duration);
        _currentSpeed = _originalSpeed;
        _slowCoroutine = null;
    }

    public void TakeDamage(float damage)
    {
        if (_hasBeenCounted) return;

        _lives -= damage;
        _lives = Mathf.Max(_lives, 0);
        UpdateHealthBar();

        StartCoroutine(FlashDamage()); // Flash efekti burada tetikleniyor

        if (_lives <= 0)
        {
            _hasBeenCounted = true;
            OnEnemyDestroyed?.Invoke(this);
            Despawn();
        }
    }

    private IEnumerator FlashDamage()
    {
        if (_spriteRenderer == null) yield break;

        _spriteRenderer.color = flashColor; // Flash rengi
        yield return new WaitForSeconds(flashDuration);
        _spriteRenderer.color = _originalColor; // Orijinal renk geri dönsün
    }

    private void Despawn()
    {
        if (Spawner.Instance != null)
        {
            var pool = Spawner.Instance.GetPoolForType(data.enemyType);
            if (pool != null)
            {
                pool.ReturnToPool(gameObject);
                Spawner.Instance.OnEnemyDestroyed();
                return;
            }
        }
    }

    private void UpdateHealthBar()
    {
        float healthPercent = _lives / _maxLives;
        Vector3 scale = _healthBarOriginalScale;
        scale.x = _healthBarOriginalScale.x * healthPercent;
        healthBar.localScale = scale;
    }

    public void Initialize(float healthMultiplier)
    {
        _hasBeenCounted = false;
        _maxLives = data.lives * healthMultiplier;
        _lives = _maxLives;
        UpdateHealthBar();
    }
}
