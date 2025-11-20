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

        if (!turnOffRewardedAds)
        {
            rewardedAds.LoadAd();
        }

        if (!turnOffBannerAd)
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

        if (rewardedAds == null)
            rewardedAds = FindAnyObjectByType<RewardedAd>();

        Button rewardedAdButton = GameObject.FindGameObjectWithTag("RewardedButton").GetComponent<Button>();

        if (rewardedAds != null && rewardedAdButton != null)
            rewardedAds.SetButton(rewardedAdButton);

        if (bannerAd == null)
            bannerAd = FindFirstObjectByType<BannerAd>();

        Button bannerButton =
            GameObject.FindGameObjectWithTag("BannerButton")
            .GetComponent<Button>();
        if (bannerAd != null && bannerButton != null)
        {
            bannerAd.SetButton(bannerButton);
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
    }
}
