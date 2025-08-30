using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public Transform[] spawnPoints;

    public void SpawnZombie()
    {
        if (spawnPoints.Length == 0) return;

        int randIndex = Random.Range(0, spawnPoints.Length);
        Instantiate(zombiePrefab, spawnPoints[randIndex].position, Quaternion.identity);
    }
}
