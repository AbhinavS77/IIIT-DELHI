using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public bool IsGameOver { get; private set; }
    public Board board; // Assign this in the Inspector
    public GameObject GameOverUI; // Assign this in the Inspector
    
    private void Awake()
    {
        if (I == null) I = this; else Destroy(gameObject);
    }
    public void GameOver()
    {
        IsGameOver = true;
        Time.timeScale = 0f; // or show UI
        ShowGameOver();
    }
    
    public void ShowGameOver()
    {
        Debug.Log("Game Over!"); // Replace with actual UI logic
    }

    public int GetScore()
    {
        return board != null ? board.score : 0;
    }
}
