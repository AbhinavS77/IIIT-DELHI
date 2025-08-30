using System.Linq;
using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("Tag to look for (set this in Inspector)")]
    public string targetTag = "zombie";
    [Tooltip("How far the player can detect zombies")]
    public float detectionRadius = 12f;

    [Header("Transforms")]
    [Tooltip("Transform that points/aims at the target (assign your gun transform)")]
    public Transform gunTransform;      // used to aim
    [Tooltip("Transform where bullets spawn (assign spawn point)")]
    public Transform spawnPoint;       // used to instantiate projectiles

    [Header("Fire settings")]
    [Tooltip("Shots per second")]
    public float rateOfFire = 2f;      // manager will set this when upgraded 
    [Tooltip("Damage value passed to projectile")]
    public float damage = 10f;         // manager will set this when upgraded

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

        // determine which transform to use to aim (prefer gunTransform)
        Transform aimTransform = gunTransform != null ? gunTransform : (spawnPoint != null ? spawnPoint : transform);

        // aim the gunTransform toward the target (assumes sprite faces right)
        Vector3 dir = (nearest.transform.position - aimTransform.position).normalized;
        if (gunTransform != null) gunTransform.right = dir;

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
        // use gunTransform position for distance checks if available, else spawnPoint, else this.transform
        Vector3 pos = gunTransform != null ? gunTransform.position : (spawnPoint != null ? spawnPoint.position : transform.position);

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
        // prefer spawnPoint for spawn position; fallback to gunTransform or this.transform
        Transform sp = spawnPoint != null ? spawnPoint : (gunTransform != null ? gunTransform : transform);
        if (projectilePrefab == null || sp == null) return;

        GameObject p = Instantiate(projectilePrefab, sp.position, Quaternion.identity);

        // align to direction (assumes projectile sprite faces right)
        p.transform.right = dir;

        // swap projectile sprite according to current upgrade level (if manager exists)
        if (UpgradeManager.I != null)
        {
            Sprite newSprite = UpgradeManager.I.GetProjectileSpriteForDamageLevel(UpgradeManager.I.gunDamageLevel);
            var sr = p.GetComponent<SpriteRenderer>();
            if (sr != null && newSprite != null) sr.sprite = newSprite;
        }

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

    // draw detection radius and helper gizmos in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (gunTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(gunTransform.position, gunTransform.position + (gunTransform.right * 0.6f));
            Gizmos.DrawWireSphere(gunTransform.position, 0.08f);
        }

        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(spawnPoint.position, 0.06f);
        }
    }
}
