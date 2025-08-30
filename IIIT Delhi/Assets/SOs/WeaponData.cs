//// Weapon ScriptableObject + usage examples for a 2D tower-zombie game
//// Put this file in Assets/Scripts (you can split into files later).

//using UnityEngine;

//public enum WeaponType { Flamethrower, Turret, Chainsaw }

//[CreateAssetMenu(menuName = "Tower/Weapon Data", fileName = "NewWeaponData")]
//public class WeaponData : ScriptableObject
//{
//    public string weaponName = "New Weapon";
//    public WeaponType weaponType;

//    [Tooltip("Damage per hit or damage-per-second depending on weapon logic")]
//    public float damage = 10f;

//    [Tooltip("Effective range in world units")]
//    public float range = 5f;

//    [Tooltip("Shots per second (for turret) or attack cycles per second")]
//    public float rateOfFire = 1f;

//    // Optional utility fields
//    public GameObject projectilePrefab; // for turret
//    public float coneAngle = 45f; // for flamethrower cone
//    public LayerMask enemyLayer = ~0;
//    public Sprite icon;
//}

//// Simple interface for enemies (so weapons can call TakeDamage)
//public interface IDamageable
//{
//    void TakeDamage(float amount);
//}

//// -----------------------------
//// Projectile script for turret bullets
//// -----------------------------
//public class Projectile : MonoBehaviour
//{
//    public float speed = 10f;
//    public float damage = 10f;
//    public float lifeTime = 5f;
//    public LayerMask hitLayer;

//    void Start()
//    {
//        Destroy(gameObject, lifeTime);
//    }

//    void Update()
//    {
//        transform.Translate(Vector3.right * speed * Time.deltaTime);
//    }

//    void OnTriggerEnter2D(Collider2D other)
//    {
//        if (((1 << other.gameObject.layer) & hitLayer) == 0) return;

//        IDamageable d = other.GetComponent<IDamageable>();
//        if (d != null)
//        {
//            d.TakeDamage(damage);
//        }
//        Destroy(gameObject);
//    }
//}

//// -----------------------------
//// TowerWeaponController: attach to tower. It will read the WeaponData and use the chosen behaviour script.
//// You can either add a specific behaviour (FlamethrowerController, TurretController, ChainsawController)
//// or use this single controller and switch by weapon type. Below I show three small behaviour components.
//// -----------------------------
//public class TurretController : MonoBehaviour
//{
//    public WeaponData data;
//    public Transform firePoint;
//    float cooldown = 0f;

//    void Update()
//    {
//        if (data == null) return;

//        cooldown -= Time.deltaTime;
//        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, data.range, data.enemyLayer);
//        if (hits.Length == 0) return;

//        // pick nearest
//        Collider2D target = hits[0];
//        float bestDist = Vector2.Distance(transform.position, target.transform.position);
//        foreach (var c in hits)
//        {
//            float d = Vector2.Distance(transform.position, c.transform.position);
//            if (d < bestDist) { bestDist = d; target = c; }
//        }

//        if (cooldown <= 0f)
//        {
//            FireAt(target.transform.position);
//            cooldown = 1f / Mathf.Max(0.0001f, data.rateOfFire);
//        }
//    }

//    void FireAt(Vector3 targetPos)
//    {
//        if (data.projectilePrefab == null || firePoint == null) return;
//        Vector3 dir = (targetPos - firePoint.position).normalized;
//        GameObject p = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.LookRotation(Vector3.forward, dir));

//        // assume Projectile script is on prefab
//        Projectile proj = p.GetComponent<Projectile>();
//        if (proj != null)
//        {
//            proj.damage = data.damage;
//            proj.hitLayer = data.enemyLayer;
//            // orient projectile sprite to travel right in local space
//            p.transform.right = dir;
//        }
//    }

//    void OnDrawGizmosSelected()
//    {
//        if (data == null) return;
//        Gizmos.color = Color.yellow;
//        Gizmos.DrawWireSphere(transform.position, data.range);
//    }
//}

////public class FlamethrowerController : MonoBehaviour
////{
////    public WeaponData data;
////    public Transform firePoint;

////    void Update()
////    {
////        if (data == null) return;

////        // find all enemies in range
////        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, data.range, data.enemyLayer);
////        if (hits.Length == 0) return;

////        foreach (var c in hits)
////        {
////            Vector2 dirTo = (c.transform.position - firePoint.position).normalized;
////            float angle = Vector2.Angle(firePoint.right, dirTo);
////            if (angle <= data.coneAngle * 0.5f)
////            {
////                IDamageable d = c.GetComponent<IDamageable>();
////                if (d != null)
////                {
////                    // continuous damage per second
////                    d.TakeDamage(data.damage * Time.deltaTime);
////                }
////            }
////        }
////    }

////    void OnDrawGizmosSelected()
////    {
////        if (data == null || firePoint == null) return;
////        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
////        // draw cone approx with two rays
////        Gizmos.DrawWireSphere(transform.position, data.range);
////        Vector3 right = firePoint.right;
////        Quaternion leftRot = Quaternion.Euler(0, 0, -data.coneAngle * 0.5f);
////        Quaternion rightRot = Quaternion.Euler(0, 0, data.coneAngle * 0.5f);
////        Gizmos.DrawLine(firePoint.position, firePoint.position + leftRot * right * data.range);
////        Gizmos.DrawLine(firePoint.position, firePoint.position + rightRot * right * data.range);
////    }
////}

//public class ChainsawController : MonoBehaviour
//{
//    public WeaponData data;
//    public Collider2D damageTrigger; // set this to a circular trigger placed at chainsaw mouth

//    void OnTriggerStay2D(Collider2D other)
//    {
//        if (data == null) return;
//        if (((1 << other.gameObject.layer) & data.enemyLayer) == 0) return;

//        IDamageable d = other.GetComponent<IDamageable>();
//        if (d != null)
//        {
//            d.TakeDamage(data.damage * Time.deltaTime);
//        }
//    }

//    void OnDrawGizmosSelected()
//    {
//        if (data == null) return;
//        Gizmos.color = Color.red;
//        Gizmos.DrawWireSphere(transform.position, data.range);
//    }
//}

//// -----------------------------
//// Example enemy implementing IDamageable
//// -----------------------------
//public class Enemy : MonoBehaviour, IDamageable
//{
//    public float maxHp = 50f;
//    float hp;

//    void Start() { hp = maxHp; }

//    public void TakeDamage(float amount)
//    {
//        hp -= amount;
//        if (hp <= 0) Die();
//    }

//    void Die()
//    {
//        Destroy(gameObject);
//    }
//}

//// -----------------------------
//// Quick notes: split into files for cleanliness in production:
//// - WeaponData.cs (ScriptableObject)
//// - TurretController.cs
//// - FlamethrowerController.cs
//// - ChainsawController.cs
//// - Projectile.cs
//// - Enemy.cs
//// -----------------------------

//// End of file
