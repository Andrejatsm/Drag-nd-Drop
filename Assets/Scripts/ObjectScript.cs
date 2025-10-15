using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ObjectScript : MonoBehaviour
{
    [Header("Vehicles")]
    public GameObject[] vehicles;
    [HideInInspector] public Vector2[] startCoordinates;

    [Header("Audio")]
    public Canvas can;
    public AudioSource effects;
    public AudioClip[] audioCli;

    [HideInInspector] public bool rightPlace = false;
    public static GameObject lastDragged = null;
    public static bool drag = false;

    [Header("Stars UI")]
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    [Header("Winning UI")]
    public GameObject winningWindow;
    public Text scoreText;

    [Header("Win/Lose Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Timer References")]
    public Timer timer;                   // Reference to Timer script
    public TextMeshProUGUI timeText;      // Reference to "TimeText" inside WinningWindow

    private int totalVehicles;
    private int placedVehicles = 0;
    private int destroyedVehicles = 0;

    private int score = 0;
    private bool gameEnded = false;
    private float internalElapsed = 0f;   // Fallback timer if no external Timer is provided
    private GameObject scorePanel;
    

    private void Awake()
    {
        // Auto-wire the Canvas if not set, using the WinningWindow's parent canvas
        if (can == null && winningWindow != null)
        {
            can = winningWindow.GetComponentInParent<Canvas>();
        }

        // Fallback auto-find for missing UI references by common names
        if (winningWindow == null)
        {
            var ww = GameObject.Find("WinningWindow");
            if (ww != null) winningWindow = ww;
        }
        if (winningWindow != null)
        {
            if (winPanel == null)
            {
                var winT = FindDeepChild(winningWindow.transform, "Win");
                if (winT != null) winPanel = winT.gameObject;
            }
            if (losePanel == null)
            {
                var loseT = FindDeepChild(winningWindow.transform, "Lose");
                if (loseT != null) losePanel = loseT.gameObject;
            }
            if (scoreText == null)
            {
                var scoreT = FindDeepChild(winningWindow.transform, "Score");
                if (scoreT != null) scoreText = scoreT.GetComponent<Text>();
            }
            if (timeText == null)
            {
                var timeT = FindDeepChild(winningWindow.transform, "TimeText");
                if (timeT != null) timeText = timeT.GetComponent<TextMeshProUGUI>();
            }
            if (can == null) can = winningWindow.GetComponentInParent<Canvas>();
        }
    }

    private void Update()
    {
        if (!gameEnded && timer == null)
        {
            internalElapsed += Time.deltaTime;
        }
    }

    public void Initialize()
    {
        if (vehicles == null || vehicles.Length == 0)
        {
            Debug.LogError("[ObjectScript] No vehicles assigned to initialize!");
            return;
        }

        startCoordinates = new Vector2[vehicles.Length];
        totalVehicles = vehicles.Length;
        placedVehicles = 0;
        destroyedVehicles = 0;
        score = 0;
        gameEnded = false;

        for (int i = 0; i < vehicles.Length; i++)
        {
            RectTransform rt = vehicles[i].GetComponent<RectTransform>();
            startCoordinates[i] = rt.anchoredPosition;
        }

        Debug.Log($"[ObjectScript] Initialized with {vehicles.Length} vehicles");
    }

    //  Called when a car is placed correctly
    public void CarPlaced()
    {
        if (gameEnded) return;

        placedVehicles++;
        score += 100;
        CheckWinLoseCondition();
    }

    //  Called when a car is destroyed (you can call this from other scripts)
    public void CarDestroyed()
    {
        if (gameEnded) return;

        destroyedVehicles++;
        CheckWinLoseCondition();
    }

    private void CheckWinLoseCondition()
    {
        if (destroyedVehicles > 0)
        {
            LoseGame();
        }
        else if (placedVehicles >= totalVehicles)
        {
            WinGame();
        }
    }

    private void WinGame()
    {
        gameEnded = true;
        Time.timeScale = 0f;

        if (timer != null)
            timer.PauseTimer();

        if (winningWindow != null)
        {
            ResolveUIRefsIfNeeded();
            // Ensure entire parent chain is active
            var t = winningWindow.transform;
            while (t != null)
            {
                if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
                t = t.parent;
            }
            winningWindow.SetActive(true);
            EnsureWindowVisible();

            if (winPanel != null) winPanel.SetActive(true);
            if (losePanel != null) losePanel.SetActive(false);
            EnsureScoreVisible();
            LogUIState("Win");

            if (scoreText != null) scoreText.text = $"Score: {score}";

            UpdateTimerUI();
            UpdateStars(placedVehicles, GetTotalElapsedTime());

            // Optional: sound or visual effect for victory
            Debug.Log("[ObjectScript] You Win!");
        }
    }

    private void LoseGame()
    {
        gameEnded = true;
        Time.timeScale = 0f;

        if (timer != null)
            timer.PauseTimer();

        if (winningWindow != null)
        {
            ResolveUIRefsIfNeeded();
            // Ensure entire parent chain is active
            var t = winningWindow.transform;
            while (t != null)
            {
                if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
                t = t.parent;
            }
            winningWindow.SetActive(true);
            EnsureWindowVisible();

            if (losePanel != null) losePanel.SetActive(true);
            if (winPanel != null) winPanel.SetActive(false);
            EnsureScoreVisible();
            LogUIState("Lose");

            if (scoreText != null) scoreText.text = $"Score: {score}";

            UpdateTimerUI();
            UpdateStars(placedVehicles, GetTotalElapsedTime());

            // Optional: sound or visual effect for losing
            Debug.Log("[ObjectScript] You Lose!");
        }
    }

    private void EnsureWindowVisible()
    {
        // Make sure the canvas that holds the window is enabled
        if (can != null) can.enabled = true;
        var parentCanvas = winningWindow.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            parentCanvas.enabled = true;
        }
        else
        {
            // No parent canvas found: attach an overlay canvas to ensure visibility
            var localCanvas = winningWindow.GetComponent<Canvas>();
            if (localCanvas == null) localCanvas = winningWindow.AddComponent<Canvas>();
            localCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            localCanvas.overrideSorting = true;
            localCanvas.sortingOrder = 2000;
            if (winningWindow.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                winningWindow.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            parentCanvas = localCanvas;
        }

        // Ensure CanvasGroup shows and receives input
        var cg = winningWindow.GetComponent<CanvasGroup>();
        if (cg == null) cg = winningWindow.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;

        // Ensure UI can receive clicks
        var prCanvas = parentCanvas != null ? parentCanvas.rootCanvas : null;
        var topCanvas = prCanvas != null ? prCanvas : parentCanvas;
        if (topCanvas != null)
        {
            // Bring this UI to the top-most sorting order
            topCanvas.overrideSorting = true;
            topCanvas.sortingOrder = 1000;
            if (topCanvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                topCanvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }

        // Bring to front so it is not hidden behind gameplay UI
        winningWindow.transform.SetAsLastSibling();

        // Ensure any parent CanvasGroups are visible and interactive
        foreach (var group in winningWindow.GetComponentsInParent<CanvasGroup>(true))
        {
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = true;
        }
        
        // Ensure any child CanvasGroups (like Lose/Win panels) are visible and interactive
        foreach (var group in winningWindow.GetComponentsInChildren<CanvasGroup>(true))
        {
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        // Recenter and normalize scale to avoid off-screen or zero-scale cases
        var rt = winningWindow.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.anchoredPosition = Vector2.zero;
        }

        // If the window has no Image, add a faint background so it's visually obvious
        var bg = winningWindow.GetComponent<UnityEngine.UI.Image>();
        if (bg == null)
        {
            bg = winningWindow.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0, 0, 0, 0.4f);
            bg.raycastTarget = true;
        }
    }

    private void LogUIState(string reason)
    {
        string winPath = winningWindow != null ? GetPath(winningWindow.transform) : "<null>";
        string winPanelPath = winPanel != null ? GetPath(winPanel.transform) : "<null>";
        string losePanelPath = losePanel != null ? GetPath(losePanel.transform) : "<null>";
        var rt = winningWindow != null ? winningWindow.GetComponent<RectTransform>() : null;
        string rtInfo = rt != null ? $"size=({rt.rect.width:F0}x{rt.rect.height:F0}) pos={rt.anchoredPosition}" : "no RT";
        Debug.Log($"[ObjectScript] Showing window because: {reason}. " +
                  $"active={winningWindow!=null && winningWindow.activeInHierarchy}, " +
                  $"winPanel={(winPanel!=null?winPanel.activeSelf:false)}, " +
                  $"losePanel={(losePanel!=null?losePanel.activeSelf:false)}, " +
                  $"winWindowPath={winPath}, winPanelPath={winPanelPath}, losePanelPath={losePanelPath}, {rtInfo}");
    }

    private void ResolveUIRefsIfNeeded()
    {
        if (winningWindow == null) return;
        if (winPanel == null)
        {
            var t = FindDeepChild(winningWindow.transform, "Win");
            if (t != null) winPanel = t.gameObject;
        }
        if (losePanel == null)
        {
            var t = FindDeepChild(winningWindow.transform, "Lose");
            if (t != null) losePanel = t.gameObject;
        }
        if (scoreText == null)
        {
            var t = FindDeepChild(winningWindow.transform, "Score");
            if (t != null) scoreText = t.GetComponent<Text>();
            if (scoreText == null)
            {
                // Fallback: find any Text whose name contains "Score"
                foreach (var txt in winningWindow.GetComponentsInChildren<Text>(true))
                {
                    if (txt != null && txt.name.ToLower().Contains("score"))
                    {
                        scoreText = txt;
                        break;
                    }
                }
            }
        }
        if (timeText == null)
        {
            var t = FindDeepChild(winningWindow.transform, "TimeText");
            if (t != null) timeText = t.GetComponent<TextMeshProUGUI>();
        }

        // Ensure ScorePanel (if exists) is active
        var scorePanelT = FindDeepChild(winningWindow.transform, "ScorePanel");
        if (scorePanelT != null)
        {
            scorePanel = scorePanelT.gameObject;
            scorePanel.SetActive(true);
        }
        // Do not touch TimePanel; let prefab layout control it
    }

    private void EnsureScoreVisible()
    {
        // Do not reorder siblings to avoid overlapping the timer text.

        // Clip anything that exceeds the panel bounds
        if (scorePanel != null)
        {
            if (scorePanel.GetComponent<RectMask2D>() == null)
            {
                scorePanel.AddComponent<RectMask2D>();
            }
        }

        if (scoreText != null)
        {
            // Ensure text is fully visible
            var c = scoreText.color;
            c.a = 1f;
            scoreText.color = c;

            // Keep legacy Text from expanding outside panel
            scoreText.resizeTextForBestFit = false;
            scoreText.horizontalOverflow = HorizontalWrapMode.Overflow; // clipped by RectMask2D
            scoreText.verticalOverflow = VerticalWrapMode.Truncate;

            // Make sure parent is active
            var p = scoreText.transform.parent;
            if (p != null && !p.gameObject.activeSelf) p.gameObject.SetActive(true);

            // Force layout update
            var rt = scoreText.GetComponent<RectTransform>();
            if (rt != null)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
            if (scorePanel != null)
            {
                var prt = scorePanel.GetComponent<RectTransform>();
                if (prt != null) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(prt);
            }
        }
    }

    // Removed EnsureTopPanelsLayout; keep original layout as authored in prefab

    private void UpdateTimerUI()
    {
        float totalTime = GetTotalElapsedTime();
        int hours = Mathf.FloorToInt(totalTime / 3600f);
        int minutes = Mathf.FloorToInt((totalTime % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(totalTime % 60f);

        if (timeText != null)
            timeText.text = $"Time: {hours:00}:{minutes:00}:{seconds:00}";
    }

    private float GetTotalElapsedTime()
    {
        return timer != null ? timer.ElapsedTime : internalElapsed;
    }

    private void UpdateStars(int score, float time)
    {
        if (star1 == null || star2 == null || star3 == null)
        {
            Debug.LogWarning("[ObjectScript] One or more star GameObjects are not assigned!");
            return;
        }

        // Hide all stars first
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);

        // Time thresholds
        float twoMinutes = 120f;
        float threeMinutes = 180f;

        int stars = 0;

        // 3 stars only when all vehicles are correctly placed (time-based)
        if (score >= totalVehicles)
        {
            if (time < twoMinutes) stars = 3;
            else if (time < threeMinutes) stars = 2;
            else stars = 1;
        }
        // 2 stars when you are just one car short (time-dependent)
        else if (score >= totalVehicles - 1)
        {
            stars = time < threeMinutes ? 2 : 1;
        }
        // Guarantee at least 1 star if at least two cars were placed (even on loss)
        else if (score >= 2)
        {
            stars = 1;
        }

        if (stars >= 1) star1.SetActive(true);
        if (stars >= 2) star2.SetActive(true);
        if (stars >= 3) star3.SetActive(true);
    }

    public void LeaveGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var found = FindDeepChild(child, name);
            if (found != null) return found;
        }
        return null;
    }

    private string GetPath(Transform t)
    {
        if (t == null) return "<null>";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        var cur = t;
        while (cur != null)
        {
            if (sb.Length == 0) sb.Insert(0, cur.name);
            else sb.Insert(0, cur.name + "/");
            cur = cur.parent;
        }
        return sb.ToString();
    }
}
