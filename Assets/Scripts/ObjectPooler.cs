using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public EnemyType enemyType;
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private Transform poolParent; // Optional parent for organization

    private Queue<GameObject> _pool = new Queue<GameObject>();

    private void Awake()
    {
        if (prefab == null)
        {
            Debug.LogError($"Prefab not assigned in ObjectPooler on {gameObject.name}");
            enabled = false; // Disable the script if prefab is missing
            return;
        }

        if (poolParent == null)
        {
            poolParent = transform; // Default to this object's transform
        }
    }

    private void Start()
    {
        InitializePool();
        
        // Only register if this is an enemy pool
        if (enemyType != EnemyType.None && Spawner.Instance != null)
        {
            Spawner.Instance.RegisterEnemyPool(enemyType, this);
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewObject();
        }
    }

    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab, poolParent);
        obj.SetActive(false);
        _pool.Enqueue(obj);
        return obj;
    }

    public GameObject GetPooledObject()
    {
        if (_pool.Count == 0)
        {
            Debug.LogWarning("Pool empty, creating new object");
            return CreateNewObject();
        }
        return _pool.Dequeue();
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;
        
        obj.SetActive(false);
        obj.transform.SetParent(poolParent);
        _pool.Enqueue(obj);
    }
}