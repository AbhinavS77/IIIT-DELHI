using UnityEngine;

public class FlamethrowerController : MonoBehaviour
{
    public float range = 4f;
    public float rateOfFire = 10f;          // flames spawned per second
    public float coneAngle = 60f;           // total cone angle
    public float projectileSpeed = 6f;
    public float projectileLife = 0.4f;     // short-lived flame sprites

    public Transform firePoint;             // child transform pointing forward
    public GameObject flamePrefab;          // simple prefab with collider (tag set at spawn)
    public LayerMask enemyLayer;

    float cooldown = 0f;

    void Update()
    {
        cooldown -= Time.deltaTime;
        // check if any enemy is in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        if (hits == null || hits.Length == 0) return;

        if (cooldown <= 0f)
        {
            SpawnFlame();
            cooldown = 1f / Mathf.Max(0.0001f, rateOfFire);
        }
    }

    void SpawnFlame()
    {
        if (flamePrefab == null || firePoint == null) return;

        // random direction inside cone
        float half = coneAngle * 0.5f;
        float angleOffset = Random.Range(-half, half);
        Vector3 dir = Quaternion.Euler(0, 0, angleOffset) * firePoint.right;
        GameObject p = Instantiate(flamePrefab, firePoint.position, Quaternion.identity);
        p.tag = "Flame";

        // give it velocity if it has Rigidbody2D
        Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = (Vector2)dir.normalized * projectileSpeed;

        Destroy(p, projectileLife);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, range);
        if (firePoint != null)
        {
            Vector3 right = firePoint.right;
            Quaternion l = Quaternion.Euler(0, 0, -coneAngle * 0.5f);
            Quaternion r = Quaternion.Euler(0, 0, coneAngle * 0.5f);
            Gizmos.DrawLine(firePoint.position, firePoint.position + (l * right) * range);
            Gizmos.DrawLine(firePoint.position, firePoint.position + (r * right) * range);
        }
    }
}
