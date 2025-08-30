using UnityEngine;

public class TurretController : MonoBehaviour
{
    public float range = 6f;
    public float rateOfFire = 2f;         // shots per second
    public float projectileSpeed = 10f;
    public float projectileLife = 4f;

    public Transform firePoint;           // muzzle transform
    public GameObject bulletPrefab;       // prefab with collider + rigidbody (tag set at spawn)
    public LayerMask enemyLayer;

    float cooldown = 0f;

    void Update()
    {
        cooldown -= Time.deltaTime;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        if (hits == null || hits.Length == 0) return;

        // choose nearest target
        Transform target = hits[0].transform;
        float best = Vector2.Distance(transform.position, target.position);
        foreach (var c in hits)
        {
            float d = Vector2.Distance(transform.position, c.transform.position);
            if (d < best) { best = d; target = c.transform; }
        }

        Vector3 dir = (target.position - firePoint.position).normalized;
        if (firePoint != null) firePoint.right = dir; // aim (assumes sprite faces right)

        if (cooldown <= 0f)
        {
            Shoot(dir);
            cooldown = 1f / Mathf.Max(0.0001f, rateOfFire);
        }
    }

    void Shoot(Vector3 dir)
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject p = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        p.tag = "Bullet";

        // orient and set velocity if Rigidbody2D exists
        p.transform.right = dir;
        Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = (Vector2)dir.normalized * projectileSpeed;

        Destroy(p, projectileLife);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
