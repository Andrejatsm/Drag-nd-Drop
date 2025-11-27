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

    [Header("Game hooks")]
    public FlyingObjectManager flyingObjectManager;
    public Timer timer;                 // in-game timer
    public float bonusSeconds = 60f;    // how many seconds to subtract

    public bool isReady = false;
    bool isLoading = false;

    void Awake()
    {
        _adUnitId = _androidAdUnitId;

        if (flyingObjectManager == null)
            flyingObjectManager = FindFirstObjectByType<FlyingObjectManager>();

        if (timer == null)
            timer = FindFirstObjectByType<Timer>();

        if (_rewardedAdButton != null)
        {
            _rewardedAdButton.onClick.RemoveAllListeners();
            _rewardedAdButton.onClick.AddListener(ShowAd);
            _rewardedAdButton.interactable = false;
        }
    }

    public void SetButton(Button button)
    {
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowAd);
        _rewardedAdButton = button;
        _rewardedAdButton.interactable = isReady;
    }

    // Called by AdManager after initialization
    public void LoadAd()
    {
        if (isLoading) return;
        StartCoroutine(LoadWhenReady());
    }

    IEnumerator LoadWhenReady()
    {
        isLoading = true;

        while (!Advertisement.isInitialized)
        {
            yield return null;
        }

        Debug.Log("Loading rewarded ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);

        isLoading = false;
    }

    public void ShowAd()
    {
        if (!isReady)
        {
            Debug.LogWarning("Rewarded ad not ready.");
            return;
        }

        isReady = false;
        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = false;

        Advertisement.Show(_adUnitId, this);
    }

    // ---- IUnityAdsLoadListener ----
    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (!placementId.Equals(_adUnitId)) return;

        Debug.Log("Rewarded ad loaded: " + placementId);
        isReady = true;
        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = true;
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"Failed to load rewarded ad ({placementId}): {error} - {message}");
        isReady = false;
        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = false;

        StartCoroutine(RetryLoad());
    }

    IEnumerator RetryLoad()
    {
        yield return new WaitForSeconds(5f);
        LoadAd();
    }

    // ---- IUnityAdsShowListener ----

    public void OnUnityAdsShowStart(string placementId)
    {
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("User clicked on rewarded ad: " + placementId);
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log("Rewarded ad completed with state: " + showCompletionState);

        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            // OLD behavior: remove flying objects
            flyingObjectManager?.DestroyAllFlyingObjects();

            // NEW: apply time bonus
            if (timer != null && bonusSeconds > 0f)
            {
                timer.ApplyTimeBonus(bonusSeconds);
                Debug.Log($"Applied time bonus: -{bonusSeconds} seconds.");
            }
        }

        Time.timeScale = 1f;

        isReady = false;
        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = false;

        // load next ad
        StartCoroutine(RetryLoad());
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning($"Failed to show rewarded ad ({placementId}): {error} - {message}");
        Time.timeScale = 1f;
        isReady = false;
        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = false;

        StartCoroutine(RetryLoad());
    }
}
