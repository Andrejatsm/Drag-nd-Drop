using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class InterstitialAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [Header("Placement IDs")]
    [SerializeField] string _androidAdUnitId = "Interstitial_Android";
    string _adUnitId;

    [Header("UI")]
    [SerializeField] Button _interstitialAdButton;

    public event Action OnInterstitialAdReady;
    public bool isReady = false;

    bool isLoading = false;

    void Awake()
    {
        _adUnitId = _androidAdUnitId;

        if (_interstitialAdButton != null)
        {
            _interstitialAdButton.onClick.RemoveAllListeners();
            _interstitialAdButton.onClick.AddListener(OnInterstitialAdButtonClicked);
            _interstitialAdButton.interactable = false;
        }
    }

    public void SetButton(Button button)
    {
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnInterstitialAdButtonClicked);
        _interstitialAdButton = button;
        _interstitialAdButton.interactable = isReady;
    }

    public void OnInterstitialAdButtonClicked()
    {
        Debug.Log("Interstitial ad button clicked!");
        ShowInterstitial();
    }

    // Public entry for manager / other scripts
    public void LoadAd()
    {
        if (isLoading) return;
        StartCoroutine(LoadWhenReady());
    }

    IEnumerator LoadWhenReady()
    {
        isLoading = true;

        // Wait until Ads are initialized
        while (!Advertisement.isInitialized)
        {
            yield return null;
        }

        Debug.Log("Loading interstitial ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);

        isLoading = false;
    }

    public void ShowAd()
    {
        if (!isReady)
        {
            Debug.LogWarning("Interstitial ad is not ready yet!");
            return;
        }

        Debug.Log("Showing interstitial ad: " + _adUnitId);
        isReady = false;
        if (_interstitialAdButton != null)
            _interstitialAdButton.interactable = false;

        Advertisement.Show(_adUnitId, this);
    }

    // Used by your AdManager for “manual or scene change” show
    public void ShowInterstitial()
    {
        if (isReady)
        {
            ShowAd();
        }
        else
        {
            Debug.Log("Interstitial not ready, loading again.");
            LoadAd();
        }
    }

    // ---- IUnityAdsLoadListener ----
    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (!placementId.Equals(_adUnitId)) return;

        Debug.Log("Interstitial ad loaded: " + placementId);
        isReady = true;

        if (_interstitialAdButton != null)
            _interstitialAdButton.interactable = true;

        OnInterstitialAdReady?.Invoke();
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"Failed to load interstitial ad ({placementId}): {error} - {message}");
        isReady = false;
        if (_interstitialAdButton != null)
            _interstitialAdButton.interactable = false;

        // Retry after a short delay
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
        Debug.Log("Showing interstitial ad now: " + placementId);
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("User clicked on interstitial ad: " + placementId);
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log("Interstitial show completed with state: " + showCompletionState);

        // Always resume time
        Time.timeScale = 1f;

        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("Interstitial watched completely -> slowing time temporarily.");
            StartCoroutine(SlowDownTimeTemporarilyRealtime(30f));
        }

        // Preload next one
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning($"Error showing interstitial ad ({placementId}): {error} - {message}");
        Time.timeScale = 1f;
        isReady = false;
        if (_interstitialAdButton != null)
            _interstitialAdButton.interactable = false;

        LoadAd();
    }

    IEnumerator SlowDownTimeTemporarilyRealtime(float seconds)
    {
        Time.timeScale = 0.4f;
        float elapsed = 0f;

        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1f;
        Debug.Log("Time restored to normal after slow motion.");
    }
}
