using System.Net.Mime;
using System.Security.Cryptography;
using UnityEngine;
using Unity;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [HideInInspector] public float speed = 8f;
    [HideInInspector] public float damage = 10f;
    [HideInInspector] public float lifeTime = 4f;
    [HideInInspector] public GameObject hitEffectPrefab;

    [Tooltip("Tags that trigger a hit (zombie / ground)")]
    public string[] hitTags = new string[] { "zombie", "ground" };

    Rigidbody2D rb;
    bool initialized = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        // keep projectile unaffected by gravity and use continuous detection if wanted
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }


    void Update()
    {

        transform.rotation *= Quaternion.Euler(0, 0, 1);
        // Optional: you can add code here for homing, effects, etc.
    }
    // call from PlayerGun when the projectile is spawned
    public void Initialize(float dmg, float spd, float life, GameObject effect)
    {
        damage = dmg;
        speed = spd;
        lifeTime = life;
        hitEffectPrefab = effect;

        // set initial velocity along transform.right
        if (rb != null) 
        {
            rb.velocity = transform.right * speed;
        }

        initialized = true;
        Destroy(gameObject, lifeTime);
    }

    // Safety: if someone forgets to call Initialize, set velocity in Start
    void Start()
    {
        if (!initialized && rb != null)
        {
            rb.velocity = transform.right * speed;
            initialized = true;
            Destroy(gameObject, lifeTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // check tags
        foreach (var t in hitTags)
        {
            if (other.CompareTag(t))
            {
                // spawn hit effect (if assigned)
                if (hitEffectPrefab != null)
                {
                    GameObject fx = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                    // auto-destroy effect after 2s (tweak if needed)
                    Destroy(fx, 2f);
                }

                // attempt to call damage function on target (left as a call; implement on your enemy)
                var dmgComp = other.GetComponent<IDamageable>();
                if (dmgComp != null)
                {
                    dmgComp.TakeDamage(damage); // your enemy will handle the effect
                }
                else
                {
                    // alternative fallback if you didn't implement IDamageable:
                    // var en = other.GetComponent<Enemy>();
                    // if (en != null) en.TakeDamage(damage);
                }

                // destroy projectile after hit
                Destroy(gameObject);
                return;
            }
        }
    }

    // If you prefer manual trigger/collider setup, ensure the projectile prefab has a Collider2D with isTrigger = true
    // This script forces gravityScale = 0 in Awake so gravity won't affect it.
}
