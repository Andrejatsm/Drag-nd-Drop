using UnityEngine;
using TMPro;

public class TimerScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Timer sourceTimer;

    private void Awake()
    {
        // Auto-find Timer if not assigned
        if (sourceTimer == null)
            sourceTimer = FindFirstObjectByType<Timer>();
    }

    private void Update()
    {
        if (sourceTimer == null || timerText == null)
            return;

        float t = sourceTimer.ElapsedTime;
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    // If anything calls ApplyTimeBonus on this script, forward it to the real timer
    public void ApplyTimeBonus(float seconds)
    {
        if (sourceTimer != null)
            sourceTimer.ApplyTimeBonus(seconds);
    }
}
