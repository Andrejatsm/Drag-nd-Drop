using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAd = false;
    [SerializeField] bool turnOffBannerAd = false;
    [SerializeField] string _androidBannerPlacementId = "Banner_Android";
    private string _bannerPlacementId;
    private bool bannerLoaded = false;
    private bool firstAdShown = false;

    public static AdManager Instance { get; private set; }

    private void Awake()
    {
        if (adsInitializer == null)
            adsInitializer = FindFirstObjectByType<AdsInitializer>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _bannerPlacementId = _androidBannerPlacementId;

        if (adsInitializer != null)
            adsInitializer.OnAdsInitialized += HandleAdsInitialized;
    }

    private void HandleAdsInitialized()
    {
        if (!turnOffInterstitialAd && interstitialAd != null)
        {
            interstitialAd.OnInterstitialAdReady += HandleInterstitialReady;
            interstitialAd.LoadAd();
        }

        if (!turnOffBannerAd)
        {
            LoadAndShowBanner();
        }
    }

    private void HandleInterstitialReady()
    {
        if (!firstAdShown)
        {
            Debug.Log("Showing first time interstitial ad automatically!");
            interstitialAd.ShowAd();
            firstAdShown = true;
        }
        else
        {
            Debug.Log("Next interstitial ad is ready for manual or scene-load show.");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (adsInitializer != null)
            adsInitializer.OnAdsInitialized -= HandleAdsInitialized;
        if (interstitialAd != null)
            interstitialAd.OnInterstitialAdReady -= HandleInterstitialReady;
    }

    private bool firstSceneLoad = false;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAd>();

        Button interstitialButton = null;
        var tagged = GameObject.FindGameObjectWithTag("InterstitialAdButton");
        if (tagged != null)
            interstitialButton = tagged.GetComponent<Button>();

        if (interstitialAd != null && interstitialButton != null)
        {
            interstitialAd.SetButton(interstitialButton);
        }

        if (!firstSceneLoad)
        {
            firstSceneLoad = true;
            Debug.Log("First scene load: skip auto interstitial (already handled).");
        }
        else
        {
            // Show interstitial each subsequent scene load if ready
            if (!turnOffInterstitialAd && interstitialAd != null)
            {
                if (interstitialAd.isReady)
                {
                    Debug.Log("Scene change: showing interstitial ad.");
                    interstitialAd.ShowAd();
                }
                else
                {
                    Debug.Log("Scene change: interstitial not ready, loading now.");
                    interstitialAd.LoadAd();
                }
            }
        }

        // Ensure banner stays visible across scenes
        if (!turnOffBannerAd && bannerLoaded)
        {
            Advertisement.Banner.Show(_bannerPlacementId);
        }
        else if (!turnOffBannerAd && !bannerLoaded && Advertisement.isInitialized)
        {
            LoadAndShowBanner();
        }
    }

    void LoadAndShowBanner()
    {
        if (Advertisement.isInitialized)
        {
            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
            Advertisement.Banner.Load(_bannerPlacementId, new BannerLoadOptions
            {
                loadCallback = () =>
                {
                    bannerLoaded = true;
                    Advertisement.Banner.Show(_bannerPlacementId);
                    Debug.Log("Banner loaded and shown.");
                },
                errorCallback = (msg) =>
                {
                    bannerLoaded = false;
                    Debug.LogWarning("Banner load failed: " + msg);
                }
            });
        }
    }
}
