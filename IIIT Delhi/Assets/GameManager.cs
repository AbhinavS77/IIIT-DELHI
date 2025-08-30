using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Add this

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public bool IsGameOver { get; private set; }
    public Board board; // Assign this in the Inspector
    public GameObject GameOverUI; // Assign this in the Inspector
    public TMP_Text scoreText; // Assign this in the Inspector
    
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
        Debug.Log("Game Over!");
        GameOverUI.SetActive(true); // Replace with actual UI logic
    }

    public int GetScore()
    {
        return board != null ? board.score : 0;
    }
public void Update(){
    scoreText.text = "Score: " + GetScore();
}
    public void RestartGame()
    {
        Time.timeScale = 1f;
        IsGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void DeductScore(int amount)
    {
        if (board != null)
        {
            board.score = Mathf.Max(0, board.score - amount);
            UpdateScoreUI(); // Update score UI on deduction
        }
    }

    public void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + GetScore();
        }
    }
}
