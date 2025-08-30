using UnityEngine;

public class Zombie : MonoBehaviour
{
    [Header("Zombie Stats")]
    public float speed = 2f;
    public float damage = 10f;
    public float health = 50f;

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

        // Move towards player only in x direction
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);
        Vector2 direction = (targetPosition - transform.position).normalized;
        transform.position += (Vector3)direction * speed * Time.deltaTime;
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
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("bullet"))
        {
            GetDamage(25); // Assuming each bullet does 25 damage
        }

        if (collision.gameObject.CompareTag("Block"))
        {
            // Call Block's TakeDamage method
            BlockHealth block = collision.gameObject.GetComponent<BlockHealth>();
            block.TakeDamage(damage); //typecast to float  

        }
    }
}
