using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class RewardedAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [Header("Placement IDs")]
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    string _adUnitId;

    [Header("UI")]
    [SerializeField] Button _rewardedAdButton;

    [Header("Reward Settings")]
    [Tooltip("How many seconds to remove from the timer when the ad is fully watched.")]
    public float bonusSeconds = 60f;

    [Header("Scene References (optional)")]
    public FlyingObjectManager flyingObjectManager;   // used in CityScene
    [SerializeField] private Timer timer;             // gameplay Timer

    public bool isReady { get; private set; } = false;

    private void Awake()
    {
        _adUnitId = _androidAdUnitId;

        // Try to auto-wire scene stuff on first load
        if (flyingObjectManager == null)
            flyingObjectManager = FindFirstObjectByType<FlyingObjectManager>();

        if (timer == null)
            timer = FindFirstObjectByType<Timer>();
    }

    /// <summary>
    /// Helper so we always get the current scene's Timer, even after scene changes.
    /// </summary>
    private Timer GetTimer()
    {
        if (timer != null) return timer;

        timer = FindFirstObjectByType<Timer>();
        if (timer == null)
        {
            Debug.LogWarning("RewardedAd: No Timer found in this scene.");
        }
        else
        {
            Debug.Log("RewardedAd: Timer reference refreshed.");
        }
        return timer;
    }

    /// <summary>
    /// Same idea for FlyingObjectManager (first game only).
    /// </summary>
    private FlyingObjectManager GetFlyingObjectManager()
    {
        if (flyingObjectManager != null) return flyingObjectManager;

        flyingObjectManager = FindFirstObjectByType<FlyingObjectManager>();
        if (flyingObjectManager == null)
        {
            Debug.Log("RewardedAd: No FlyingObjectManager found in this scene (probably not CityScene).");
        }
        else
        {
            Debug.Log("RewardedAd: FlyingObjectManager reference refreshed.");
        }
        return flyingObjectManager;
    }

    // ------------------ LOAD / SHOW ------------------

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("RewardedAd: Tried to load before Unity Ads was initialized.");
            return;
        }

        Debug.Log("RewardedAd: Loading...");
        Advertisement.Load(_adUnitId, this);
    }

    public void ShowAd()
    {
        if (!isReady)
        {
            Debug.LogWarning("RewardedAd: Not ready when ShowAd called.");
            return;
        }

        isReady = false;
        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = false;

        Advertisement.Show(_adUnitId, this);
    }

    public void SetButton(Button button)
    {
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowAd);
        _rewardedAdButton = button;
        _rewardedAdButton.interactable = isReady;
    }

    // ------------------ UNITY ADS CALLBACKS ------------------

    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId != _adUnitId) return;

        Debug.Log("RewardedAd: Loaded.");
        isReady = true;
        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = true;
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        if (placementId != _adUnitId) return;

        Debug.LogWarning($"RewardedAd: Failed to load ({error}) {message}");
        isReady = false;
        StartCoroutine(WaitAndLoad(5f));
    }

    public IEnumerator WaitAndLoad(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadAd();
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        if (placementId != _adUnitId) return;

        Debug.Log("RewardedAd: Show started. Pausing game time.");
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        if (placementId != _adUnitId) return;
        Debug.Log("RewardedAd: Clicked.");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        if (placementId != _adUnitId) return;

        Debug.LogWarning($"RewardedAd: Show failed ({error}) {message}");
        Time.timeScale = 1f;
        isReady = false;
        StartCoroutine(WaitAndLoad(5f));
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId != _adUnitId) return;

        Debug.Log($"RewardedAd: Show complete, state = {showCompletionState}.");
        Time.timeScale = 1f;

        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            // 1) Old behaviour: clear flying objects in first game
            GetFlyingObjectManager()?.DestroyAllFlyingObjects();

            // 2) NEW behaviour: apply time bonus in whatever scene we are in now
            if (bonusSeconds > 0f)
            {
                var t = GetTimer();
                if (t != null)
                {
                    t.ApplyTimeBonus(bonusSeconds);
                    Debug.Log($"RewardedAd: Applied time bonus -{bonusSeconds} seconds. New elapsed = {t.ElapsedTime:F1}s");
                }
                else
                {
                    Debug.LogWarning("RewardedAd: No Timer to apply bonus to.");
                }
            }
        }
        else
        {
            Debug.Log("RewardedAd: Ad was skipped or not completed, no reward.");
        }

        isReady = false;
        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = false;

        StartCoroutine(WaitAndLoad(10f));
    }
}
