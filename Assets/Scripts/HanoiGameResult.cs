using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HanoiGameResult : MonoBehaviour
{
    [Header("Refs")]
    public HanoiManager hanoiManager;
    public Timer timer;                 // your existing Timer script (optional)
    public CamerScript camScript;      // or HanojasCamera – any camera script with FocusToCenterForEndScreen

    [Header("Winning UI root")]
    public GameObject winningWindow;    // panel that contains Win/Lose stuff
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Score / Time UI")]
    public Text scoreText;              // normal Text (UnityEngine.UI)
    public TextMeshProUGUI timeText;    // TMP for time label

    [Header("Stars")]
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    [Header("Score Settings")]
    public int baseScore = 1000;

    [Tooltip("≤ this time -> 3 stars")]
    public float threeStarTime = 90f;   // 1:30

    [Tooltip("≤ this time -> 2 stars, otherwise 1 star")]
    public float twoStarTime = 180f;    // 3:00

    private bool gameEnded = false;

    void Awake()
    {
        if (hanoiManager == null)
            hanoiManager = FindFirstObjectByType<HanoiManager>();

        if (timer == null)
            timer = FindFirstObjectByType<Timer>();

        if (camScript == null && Camera.main != null)
            camScript = Camera.main.GetComponent<CamerScript>();

        if (winningWindow != null)
            winningWindow.SetActive(false);
    }

    void OnEnable()
    {
        if (hanoiManager != null)
            hanoiManager.OnSolved += HandleSolved;
    }

    void OnDisable()
    {
        if (hanoiManager != null)
            hanoiManager.OnSolved -= HandleSolved;
    }

    void HandleSolved()
    {
        if (gameEnded) return;
        gameEnded = true;

        // stop time
        if (timer != null)
            timer.PauseTimer();

        Time.timeScale = 0f;

        // focus camera to center if you want that same effect
        if (camScript != null)
            camScript.FocusToCenterForEndScreen(0.35f);

        ShowWinWindow();
    }

    void ShowWinWindow()
    {
        if (winningWindow == null) return;

        winningWindow.SetActive(true);
        if (winPanel != null) winPanel.SetActive(true);
        if (losePanel != null) losePanel.SetActive(false);

        float elapsed = GetElapsedTime();
        int stars = ComputeStars(elapsed);
        int score = ComputeScore(elapsed, stars);

        // stars
        if (star1 != null) star1.SetActive(stars >= 1);
        if (star2 != null) star2.SetActive(stars >= 2);
        if (star3 != null) star3.SetActive(stars >= 3);

        // score text
        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        // time text (HH:MM:SS)
        if (timeText != null)
        {
            int hours = Mathf.FloorToInt(elapsed / 3600f);
            int minutes = Mathf.FloorToInt((elapsed % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);
            timeText.text = $"Time: {hours:00}:{minutes:00}:{seconds:00}";
        }
    }

    float GetElapsedTime()
    {
        if (timer != null)
            return timer.ElapsedTime;   // assumes your Timer exposes this

        // fallback – from scene start
        return Time.timeSinceLevelLoad;
    }

    int ComputeStars(float time)
    {
        if (time <= threeStarTime) return 3;
        if (time <= twoStarTime) return 2;
        return 1;
    }

    int ComputeScore(float time, int stars)
    {
        // simple example: faster = more bonus
        // you can tune these numbers
        float timeFactor = Mathf.Clamp01(threeStarTime / Mathf.Max(time, 1f));
        int starBonus = (stars - 1) * 250;

        return Mathf.RoundToInt(baseScore * timeFactor) + starBonus;
    }

    // Optional: public button methods
    public void Restart()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager
            .LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void BackToMenu(string menuSceneName = "MainMenu")
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(menuSceneName);
    }
}
