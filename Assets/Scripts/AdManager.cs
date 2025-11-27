using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;

    [Header("Interstitial")]
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAd = false;
    bool firstAdShown = false;

    [Header("Rewarded")]
    public RewardedAd rewardedAds;
    [SerializeField] bool turnOffRewardedAds = false;

    [Header("Banner")]
    public BannerAd bannerAd;
    [SerializeField] bool turnOffBannerAd = false;

    public static AdManager Instance { get; private set; }

    bool firstSceneLoad = false;

    void Awake()
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

        // Ensure controllers exist (either in scene or added here)
        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAd>() ?? gameObject.AddComponent<InterstitialAd>();

        if (rewardedAds == null)
            rewardedAds = FindFirstObjectByType<RewardedAd>() ?? gameObject.AddComponent<RewardedAd>();

        if (bannerAd == null)
            bannerAd = FindFirstObjectByType<BannerAd>() ?? gameObject.AddComponent<BannerAd>();

        if (adsInitializer != null)
            adsInitializer.OnAdsInitialized += HandleAdsInitialized;
    }

    void Start()
    {
        // If ads are already initialized (e.g. returning from another scene), start loading immediately
        if (Advertisement.isInitialized)
        {
            HandleAdsInitialized();
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (adsInitializer != null)
            adsInitializer.OnAdsInitialized -= HandleAdsInitialized;

        if (interstitialAd != null)
            interstitialAd.OnInterstitialAdReady -= HandleInterstitialReady;
    }

    void HandleAdsInitialized()
    {
        if (!turnOffInterstitialAd && interstitialAd != null)
        {
            interstitialAd.OnInterstitialAdReady += HandleInterstitialReady;
            interstitialAd.LoadAd();
        }

        if (!turnOffRewardedAds && rewardedAds != null)
        {
            rewardedAds.LoadAd();
        }

        if (!turnOffBannerAd && bannerAd != null)
        {
            bannerAd.LoadBanner();
        }
    }

    void HandleInterstitialReady()
    {
        if (!firstAdShown)
        {
            Debug.Log("First interstitial ready: showing automatically.");
            interstitialAd.ShowAd();
            firstAdShown = true;
        }
        else
        {
            Debug.Log("Next interstitial ready for scene-change or manual show.");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-link interstitial button by tag
        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAd>() ?? gameObject.GetComponent<InterstitialAd>();

        Button interstitialButton = null;
        var tagged = GameObject.FindGameObjectWithTag("InterstitialAdButton");
        if (tagged != null)
            interstitialButton = tagged.GetComponent<Button>();

        if (interstitialAd != null && interstitialButton != null)
            interstitialAd.SetButton(interstitialButton);

        // Re-link rewarded button by tag
        if (rewardedAds == null)
            rewardedAds = FindFirstObjectByType<RewardedAd>() ?? gameObject.GetComponent<RewardedAd>();

        Button rewardedAdButton = null;
        var rewardTagged = GameObject.FindGameObjectWithTag("RewardedButton");
        if (rewardTagged != null)
            rewardedAdButton = rewardTagged.GetComponent<Button>();

        if (rewardedAds != null && rewardedAdButton != null)
            rewardedAds.SetButton(rewardedAdButton);

        // Re-link banner button by tag
        if (bannerAd == null)
            bannerAd = FindFirstObjectByType<BannerAd>() ?? gameObject.GetComponent<BannerAd>();

        Button bannerButton = null;
        var bannerTagged = GameObject.FindGameObjectWithTag("BannerButton");
        if (bannerTagged != null)
            bannerButton = bannerTagged.GetComponent<Button>();

        if (bannerAd != null && bannerButton != null)
            bannerAd.SetButton(bannerButton);

        // Interstitial on scene change (after first load)
        if (!firstSceneLoad)
        {
            firstSceneLoad = true;
            Debug.Log("First scene load: skip auto interstitial (handled already if needed).");
        }
        else
        {
            if (!turnOffInterstitialAd && interstitialAd != null)
            {
                if (interstitialAd.isReady)
                {
                    Debug.Log("Scene changed: showing interstitial.");
                    interstitialAd.ShowAd();
                }
                else
                {
                    Debug.Log("Scene changed: interstitial not ready, loading.");
                    interstitialAd.LoadAd();
                }
            }
        }

        // Banner only on CityScene / HanojasScene
        if (!turnOffBannerAd && bannerAd != null)
        {
            if (scene.name == "CityScene" || scene.name == "HanojasScene")
            {
                bannerAd.EnsureVisible(true);
            }
            else
            {
                bannerAd.EnsureVisible(false);
            }
        }
    }
}
