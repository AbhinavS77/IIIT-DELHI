using UnityEngine;

public class Zombie : MonoBehaviour
{
    [Header("Zombie Stats")]
    public float speed = 2f;
    public int damage = 10;
    public int health = 50;

    private Transform player;

    void Start()
    {
        // Find player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Ensure zombie tag is set
        gameObject.tag = "Zombie";
    }

        void Update()
        {
            if (player == null) return;

            // Move towards player
            transform.position += (Vector3)moveDirection * speed * Time.deltaTime;
        }

    // Zombie takes damage
    public void GetDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    // When zombie touches the player
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Call Player script’s TakeDamage method
            Player playerScript = collision.gameObject.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
            }
        }
    }
}
