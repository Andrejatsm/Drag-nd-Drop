using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using Unity.VisualScripting;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAd = false;
    private bool firstAdShown = false;

    public RewardedAd rewardedAds;
    [SerializeField] bool turnOffRewardedAds = false;

    public BannerAd bannerAd;
    [SerializeField] bool turnOffBannerAd = false;
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

        // Ensure essential ad controller components exist on this persistent object so ads work
        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAd>();
        if (interstitialAd == null)
            interstitialAd = gameObject.AddComponent<InterstitialAd>();

        if (rewardedAds == null)
            rewardedAds = FindFirstObjectByType<RewardedAd>();
        if (rewardedAds == null)
            rewardedAds = gameObject.AddComponent<RewardedAd>();

        if (bannerAd == null)
            bannerAd = FindFirstObjectByType<BannerAd>();
        if (bannerAd == null)
            bannerAd = gameObject.AddComponent<BannerAd>();

        if (adsInitializer != null)
            adsInitializer.OnAdsInitialized += HandleAdsInitialized;
    }

    private void HandleAdsInitialized()
    {
        // (re-check references; if created dynamically they are present)
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
        // Ensure references to ad controllers (they are persistent or in-scene)
        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAd>() ?? gameObject.GetComponent<InterstitialAd>();

        Button interstitialButton = null;
        var tagged = GameObject.FindGameObjectWithTag("InterstitialAdButton");
        if (tagged != null)
            interstitialButton = tagged.GetComponent<Button>();

        if (interstitialAd != null && interstitialButton != null)
        {
            interstitialAd.SetButton(interstitialButton);
        }

        if (rewardedAds == null)
            rewardedAds = FindAnyObjectByType<RewardedAd>() ?? gameObject.GetComponent<RewardedAd>();

        Button rewardedAdButton = null;
        var rewardTagged = GameObject.FindGameObjectWithTag("RewardedButton");
        if (rewardTagged != null)
            rewardedAdButton = rewardTagged.GetComponent<Button>();

        if (rewardedAds != null && rewardedAdButton != null)
            rewardedAds.SetButton(rewardedAdButton);

        if (bannerAd == null)
            bannerAd = FindFirstObjectByType<BannerAd>() ?? gameObject.GetComponent<BannerAd>();

        Button bannerButton = null;
        var bannerTagged = GameObject.FindGameObjectWithTag("BannerButton");
        if (bannerTagged != null)
            bannerButton = bannerTagged.GetComponent<Button>();

        if (bannerAd != null && bannerButton != null)
        {
            bannerAd.SetButton(bannerButton);
        }

        // Show interstitial on scene change after first load
        if (!firstSceneLoad)
        {
            firstSceneLoad = true;
            Debug.Log("First scene load: skip auto interstitial (already handled).");
        }
        else
        {
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

        // Banner visibility: only show on CityScene and HanojasScene
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
