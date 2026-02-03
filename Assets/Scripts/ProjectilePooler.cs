using System.Collections.Generic;
using UnityEngine;

public class ProjectilePooler : MonoBehaviour
{
    [Header("Pool Settings")]
    [Tooltip("The projectile prefab to pool")]
    public GameObject projectilePrefab; // Changed from [SerializeField] to public

    [Tooltip("Initial size of the projectile pool")]
    public int poolSize = 10; // Changed from [SerializeField] to public

    private Queue<GameObject> _projectilePool;

    private void Awake()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab not assigned!", this);
            enabled = false;
            return;
        }

        _projectilePool = new Queue<GameObject>();
    }

    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewProjectile();
        }
    }

    private GameObject CreateNewProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, transform);
        projectile.SetActive(false);

        // Yeni oluþturulan mermiyi hemen sýfýrla
        ExplosiveProjectile explosive = projectile.GetComponent<ExplosiveProjectile>();
        if (explosive != null)
        {
            // Yeni merminin baþlangýç durumunda olmasýný saðla
            explosive.ResetAnimator();
        }

        _projectilePool.Enqueue(projectile);
        return projectile;
    }

    public GameObject GetProjectile()
    {
        return _projectilePool.Count == 0 ?
            CreateNewProjectile() :
            _projectilePool.Dequeue();
    }

    public void ReturnProjectile(GameObject projectile)
    {
        if (projectile == null) return;

        projectile.SetActive(false);
        projectile.transform.SetParent(transform);
        _projectilePool.Enqueue(projectile);
    }
}