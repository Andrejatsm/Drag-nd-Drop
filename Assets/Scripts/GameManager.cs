using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int carsPlaced = 0;
    public int totalCars = 12; // set in Inspector or dynamically

    public GameObject winScreen;
    public GameObject loseScreen;
    public UnityEngine.UI.Text scoreText;

    private int score = 0;
    private bool gameEnded = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CarPlacedCorrectly()
    {
        if (gameEnded) return;

        carsPlaced++;
        score += 100;
        UpdateScoreUI();

        if (carsPlaced >= totalCars)
        {
            WinGame();
        }
    }

    public void CarDestroyed()
    {
        if (gameEnded) return;
        LoseGame();
    }

    void WinGame()
    {
        gameEnded = true;
        winScreen.SetActive(true);
    }

    void LoseGame()
    {
        gameEnded = true;
        loseScreen.SetActive(true);
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
