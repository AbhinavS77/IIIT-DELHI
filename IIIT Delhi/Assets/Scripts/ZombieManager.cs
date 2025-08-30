using UnityEngine;
using System.Collections;

public class ZombieManager : MonoBehaviour
{
    [Header("Spawner Settings")]
    public ZombieSpawner spawner;
    public float spawnInterval = 3f;           // Time between regular spawns
    public float spawnRateMultiplier = 0.8f;   // Spawn rate multiplier after each wave

    [Header("Wave Settings")]
    public int totalWaves = 3;
    public float timeBetweenWaves = 20f;       // Time before next wave starts
    public int waveZombieCount = 10;           // How many zombies to spawn in a wave burst
    public float burstSpawnDelay = 0.4f;       // Delay between zombies in a burst

    private int currentWave = 0;
    private float nextSpawnTime;
    private float waveTimer;
    private bool isWaveSpawning = false;

    private float initialDelay = 60f; // 1 minute delay before any spawning
    private bool spawningStarted = false;

    void Start()
    {
        waveTimer = timeBetweenWaves;
        nextSpawnTime = Time.time + initialDelay; // Delay first spawn by 1 minute
    }

    void Update()
    {
        if (currentWave >= totalWaves) return; // stop after all waves

        // Wait for initial delay before starting spawning
        if (!spawningStarted && Time.time >= nextSpawnTime)
        {
            spawningStarted = true;
            nextSpawnTime = Time.time + spawnInterval;
        }

        if (!spawningStarted) return;

        // Regular spawning
        if (Time.time >= nextSpawnTime && !isWaveSpawning)
        {
            spawner.SpawnZombie();
            nextSpawnTime = Time.time + spawnInterval;
        }

        // Wave timer countdown
        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0 && !isWaveSpawning)
        {
            StartCoroutine(StartWave());
        }
    }

    IEnumerator StartWave()
    {
        isWaveSpawning = true;
        currentWave++;
        Debug.Log("Wave " + currentWave + " started!");

        // Burst spawn zombies with small delay
        for (int i = 0; i < waveZombieCount; i++)
        {
            spawner.SpawnZombie();
            yield return new WaitForSeconds(burstSpawnDelay);
        }

        // Increase spawn rate for next wave
        spawnInterval *= spawnRateMultiplier;

        // Reset wave timer if more waves remain
        if (currentWave < totalWaves)
            waveTimer = timeBetweenWaves;

        isWaveSpawning = false;
    }
}
