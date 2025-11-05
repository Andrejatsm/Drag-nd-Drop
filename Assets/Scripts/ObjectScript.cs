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

    [Header("End-Game Interaction Blocking")]
    [Tooltip("All objects with this tag will be disabled when the game ends.")]
    public string bombTag = "Bomb";

    private int totalVehicles;
    private int placedVehicles = 0;
    private int destroyedVehicles = 0;

    private int score = 0;
    private bool gameEnded = false;
    private float internalElapsed = 0f;   // Fallback timer if no external Timer is provided
    private GameObject scorePanel;

    private CameraScript camScript; // cache camera controller

    private void Awake()
    {
        // Auto-wire the Canvas if not set, using the WinningWindow's parent canvas
        if (can == null && winningWindow != null)
        {
            can = winningWindow.GetComponentInParent<Canvas>();
        }

        // Cache camera script
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            camScript = mainCam.GetComponent<CameraScript>();
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

        //  stop bombs first so they can't interact during end-screen setup
        DisableBombs();

        Time.timeScale = 0f;

        // Smoothly focus camera to world center for the end screen (works with timeScale =0)
        if (camScript != null)
        {
            camScript.FocusToCenterForEndScreen(0.35f);
        }

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

            Debug.Log("[ObjectScript] You Win!");
        }
    }

    private void LoseGame()
    {
        gameEnded = true;

        // stop bombs immediately
        DisableBombs();

        Time.timeScale = 0f;

        // Smoothly focus camera to world center for the end screen (works with timeScale =0)
        if (camScript != null)
        {
            camScript.FocusToCenterForEndScreen(0.35f);
        }

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

            Debug.Log("[ObjectScript] You Lose!");
        }
    }

    private void EnsureWindowVisible()
    {
        if (can != null) can.enabled = true;
        var parentCanvas = winningWindow.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            parentCanvas.enabled = true;
        }
        else
        {
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

        var cg = winningWindow.GetComponent<CanvasGroup>();
        if (cg == null) cg = winningWindow.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;

        var prCanvas = parentCanvas != null ? parentCanvas.rootCanvas : null;
        var topCanvas = prCanvas != null ? prCanvas : parentCanvas;
        if (topCanvas != null)
        {
            topCanvas.overrideSorting = true;
            topCanvas.sortingOrder = 1000;
            if (topCanvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                topCanvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }

        winningWindow.transform.SetAsLastSibling();

        foreach (var group in winningWindow.GetComponentsInParent<CanvasGroup>(true))
        {
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = true;
        }
        foreach (var group in winningWindow.GetComponentsInChildren<CanvasGroup>(true))
        {
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        var rt = winningWindow.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.anchoredPosition = Vector2.zero;
        }

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
                  $"active={winningWindow != null && winningWindow.activeInHierarchy}, " +
                  $"winPanel={(winPanel != null ? winPanel.activeSelf : false)}, " +
                  $"losePanel={(losePanel != null ? losePanel.activeSelf : false)}, " +
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

        var scorePanelT = FindDeepChild(winningWindow.transform, "ScorePanel");
        if (scorePanelT != null)
        {
            scorePanel = scorePanelT.gameObject;
            scorePanel.SetActive(true);
        }
    }

    private void EnsureScoreVisible()
    {
        if (scorePanel != null)
        {
            if (scorePanel.GetComponent<RectMask2D>() == null)
            {
                scorePanel.AddComponent<RectMask2D>();
            }
        }

        if (scoreText != null)
        {
            var c = scoreText.color;
            c.a = 1f;
            scoreText.color = c;

            scoreText.resizeTextForBestFit = false;
            scoreText.horizontalOverflow = HorizontalWrapMode.Overflow;
            scoreText.verticalOverflow = VerticalWrapMode.Truncate;

            var p = scoreText.transform.parent;
            if (p != null && !p.gameObject.activeSelf) p.gameObject.SetActive(true);

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

        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);

        float twoMinutes = 120f;
        float threeMinutes = 180f;

        int stars = 0;

        if (score >= totalVehicles)
        {
            if (time < twoMinutes) stars = 3;
            else if (time < threeMinutes) stars = 2;
            else stars = 1;
        }
        else if (score >= totalVehicles - 1)
        {
            stars = time < threeMinutes ? 2 : 1;
        }
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

    private void DisableBombs()
    {
        GameObject[] bombs = string.IsNullOrEmpty(bombTag)
            ? new GameObject[0]
            : GameObject.FindGameObjectsWithTag(bombTag);

        foreach (var b in bombs)
        {
            if (b == null) continue;

            // stop physics
            foreach (var rb2 in b.GetComponentsInChildren<Rigidbody2D>(true))
            {
                rb2.linearVelocity = Vector2.zero;
                rb2.angularVelocity = 0f;
                rb2.simulated = false;
            }
            foreach (var rb in b.GetComponentsInChildren<Rigidbody>(true))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            // stop hits
            foreach (var c2d in b.GetComponentsInChildren<Collider2D>(true)) c2d.enabled = false;
            foreach (var c3d in b.GetComponentsInChildren<Collider>(true)) c3d.enabled = false;

            // stop UI hits if any bomb uses UI
            var cg = b.GetComponentInChildren<CanvasGroup>(true);
            if (cg != null) cg.blocksRaycasts = false;

            // best-effort: turn off any bomb logic scripts you wrote
            foreach (var mb in b.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) continue;
                string n = mb.GetType().Name.ToLower();
                if (n.Contains("bomb") || n.Contains("flying") || n.Contains("explode") || n.Contains("damage"))
                    mb.enabled = false;
            }
        }
    }
}
