using UnityEngine;



[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public class EnemyGroup
    {
        public EnemyType enemyType;
        public int count;
        public float spawnRate; // Enemies per second
    }

    public EnemyGroup[] enemyGroups;
    public float timeBetweenWaves = 3f;
    public float healthMultiplierPerWave = 0.14f;
}