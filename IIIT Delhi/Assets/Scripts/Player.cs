using UnityEngine;

public class Player : MonoBehaviour
{
    public int health = 100;

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        Debug.Log("Player took damage: " + dmg + " | Remaining Health: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    public void AttackZombie(Zombie zombie)
    {
        // Example: Deal 20 damage
        zombie.GetDamage(20);
    }

    private void Die()
    {
        Debug.Log("Player died!");
        // Add death logic here
    }
}
