using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Gun Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint; // Where bullets spawn
    public float fireRate = 1f; // Bullets per second
    public float bulletSpeed = 10f;
    public float detectionRange = 20f;
    public int Health = 100;


    [Header("Enemy Settings")]
    public string enemyTag = "Enemy"; // Tag enemies with "Enemy"

    private float fireCooldown = 0f;

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        // Find nearest enemy
        GameObject nearestEnemy = FindNearestEnemy();

        if (nearestEnemy != null && fireCooldown <= 0f)
        {
            Shoot(nearestEnemy);
            fireCooldown = 1f / fireRate; // Reset cooldown
        }

        if (Health <= 0)
        {
            OnDeath();
        }
    }

    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        if (enemies.Length == 0) return null;

        GameObject nearest = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(currentPos, enemy.transform.position);
            if (dist < minDist && dist <= detectionRange)
            {
                minDist = dist;
                nearest = enemy;
            }
        }

        return nearest;
    }

    void Shoot(GameObject target)
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Create bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // Aim at enemy
        Vector3 direction = (target.transform.position - firePoint.position).normalized;
        bullet.transform.forward = direction;

        // Add velocity
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }
    }

    void OnDeath()
    {
        // Handle character death (e.g., play animation, remove from game)
        Destroy(gameObject);
        Debug.Log("Character Died");
    }
}



