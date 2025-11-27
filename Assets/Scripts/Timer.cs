using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private float elapsedTime = 0f;
    private bool isPaused = false;

    // Public read-only access for other scripts (ObjectScript, TimerScript, etc.)
    public float ElapsedTime
    {
        get => elapsedTime;
        private set => elapsedTime = Mathf.Max(0f, value); // never below 0
    }

    private void Start()
    {
        if (timerText == null)
            Debug.LogWarning("Timer: timerText is not assigned in the Inspector!");

        UpdateText();
    }

    private void Update()
    {
        if (isPaused)
            return;

        ElapsedTime += Time.deltaTime;   // uses setter -> clamps at 0
        UpdateText();
    }

    private void UpdateText()
    {
        if (timerText == null)
            return;

        int minutes = Mathf.FloorToInt(ElapsedTime / 60f);
        int seconds = Mathf.FloorToInt(ElapsedTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void PauseTimer()
    {
        isPaused = true;
    }

    public void ResumeTimer()
    {
        isPaused = false;
    }

    public void ResetTimer()
    {
        ElapsedTime = 0f;
        UpdateText();
    }

    // -------------- TIME BONUS --------------
    // seconds should be positive: we subtract from elapsed
    public void ApplyTimeBonus(float seconds)
    {
        ElapsedTime -= seconds;  // property clamps it
        UpdateText();
    }
}
