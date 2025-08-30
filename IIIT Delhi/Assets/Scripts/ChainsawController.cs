using UnityEngine;

public class ChainsawController : MonoBehaviour
{
    public float range = 1.2f;           // used for gizmo
    public float rateOfFire = 1f;        // swings per second
    public float attackDuration = 0.5f;  // how long the chainsaw hit object stays

    public Transform attackPoint;        // where chainsaw hit appears
    public GameObject chainsawPrefab;    // small prefab with collider (no velocity needed)
    public LayerMask enemyLayer;

    float cooldown = 0f;

    void Update()
    {
        cooldown -= Time.deltaTime;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        if (hits == null || hits.Length == 0) return;

        if (cooldown <= 0f)
        {
            SpawnChainsawHit();
            cooldown = 1f / Mathf.Max(0.0001f, rateOfFire);
        }
    }

    void SpawnChainsawHit()
    {
        if (chainsawPrefab == null || attackPoint == null) return;

        GameObject p = Instantiate(chainsawPrefab, attackPoint.position, attackPoint.rotation);
        p.tag = "Chainsaw";

        // if this prefab needs to be attached to the tower temporarily:
        // p.transform.SetParent(transform);

        Destroy(p, attackDuration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
