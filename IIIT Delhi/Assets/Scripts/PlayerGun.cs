using System.Linq;
using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("Tag to look for (set this in Inspector)")]
    public string targetTag = "zombie";
    [Tooltip("How far the player can detect zombies")]
    public float detectionRadius = 12f;

    [Header("Fire settings")]
    public Transform firePoint;              // assign in Inspector
    [Tooltip("Shots per second")]
    public float rateOfFire = 2f;
    [Tooltip("Damage value passed to projectile")]
    public float damage = 10f;

    [Header("Projectile")]
    public GameObject projectilePrefab;      // assign prefab in Inspector
    [Tooltip("Base speed; final speed = baseProjectileSpeed * rateOfFire")]
    public float baseProjectileSpeed = 6f;
    public float projectileLifeTime = 4f;
    public GameObject hitEffectPrefab;       // particle prefab spawned on hit

    float cooldown = 0f;

    void Update()
    {
        cooldown -= Time.deltaTime;

        // find nearest target with the target tag
        GameObject nearest = FindNearestWithTag(targetTag);
        if (nearest == null) return;

        // check distance
        float dist = Vector2.Distance(transform.position, nearest.transform.position);
        if (dist > detectionRadius) return;

        // aim the firePoint toward the target (assumes sprite faces right)
        Vector3 dir = (nearest.transform.position - firePoint.position).normalized;
        if (firePoint != null) firePoint.right = dir;

        // auto-fire
        if (cooldown <= 0f)
        {
            Shoot(dir);
            cooldown = 1f / Mathf.Max(0.0001f, rateOfFire);
        }
    }

    GameObject FindNearestWithTag(string tag)
    {
        var all = GameObject.FindGameObjectsWithTag(tag);
        if (all == null || all.Length == 0) return null;

        GameObject best = null;
        float bestDist = float.MaxValue;
        Vector3 pos = firePoint != null ? firePoint.position : transform.position;

        foreach (var g in all)
        {
            if (g == null) continue;
            float d = Vector2.Distance(pos, g.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = g;
            }
        }

        return best;
    }

    void Shoot(Vector3 dir)
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        // align to direction (assumes projectile sprite faces right)
        p.transform.right = dir;

        // configure projectile
        Projectile proj = p.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Initialize(
                damage,
                baseProjectileSpeed * Mathf.Max(0.0001f, rateOfFire), // shoot speed increases with rateOfFire
                projectileLifeTime,
                hitEffectPrefab
            );
        }

        // optional: tag the spawned projectile if you want
        // p.tag = "PlayerProjectile";
    }

    // draw detection radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        if (firePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(firePoint.position, firePoint.position + (firePoint.right * 0.6f));
        }
    }
}
