using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected TowerData _data; // protected yap
    protected Vector3 _shootDirection; // protected yap
    protected float _projectileDuration; // protected yap
    protected ProjectilePooler _pooler;
    protected float _travelTime;
    protected Vector3 _initialPosition;

    private void Awake()
    {
        _pooler = GetComponentInParent<ProjectilePooler>();
    }

    protected virtual void Update()
    {
        _projectileDuration -= Time.deltaTime;
        _travelTime += Time.deltaTime;

        if (_data == null)
        {
            ReturnToPool();
            return;
        }

        // Move projectile
        transform.position += new Vector3(_shootDirection.x, _shootDirection.y) * _data.projectileSpeed * Time.deltaTime;

        // Add parabolic arc for arrows (more realistic)
        if (gameObject.name.ToLower().Contains("arrow"))
        {
            // Calculate arc based on travel time
            float arcHeight = 0.5f;
            float arc = -arcHeight * _travelTime * (_travelTime - _projectileDuration);
            transform.position += new Vector3(0, arc * Time.deltaTime, 0);
        }

        // Rotate to face movement direction
        if (_shootDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(_shootDirection.y, _shootDirection.x) * Mathf.Rad2Deg;

            // Add slight tilt for arrows for more natural look
            if (gameObject.name.ToLower().Contains("arrow"))
            {
                angle -= 10f; // Slight downward tilt
            }

            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }

        if (_projectileDuration <= 0)
        {
            ReturnToPool();
        }
    }

    public void Shoot(TowerData data, Vector3 shootDirection)
    {
        _data = data;
        _shootDirection = shootDirection.normalized;
        _projectileDuration = data.projectileDuration;
        _travelTime = 0f;
        _initialPosition = transform.position;

        // Initial rotation
        if (_shootDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(_shootDirection.y, _shootDirection.x) * Mathf.Rad2Deg;

            // Add slight tilt for arrows
            if (gameObject.name.ToLower().Contains("arrow"))
            {
                angle -= 10f;
            }

            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_data == null) return; //Bu satýrý ekle


        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(_data.damage);
                ReturnToPool();
            }
        }
    }

    protected void ReturnToPool()
    {
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
        // Reset values when projectile is reused
        _travelTime = 0f;
    }
}