using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class BannerAd : MonoBehaviour
{
    [Header("Placement IDs")]
    [SerializeField] string _androidAdUnitId = "Banner_Android";
    string _adUnitId;

    [Header("UI")]
    [SerializeField] Button _bannerButton;

    [Header("Banner Position")]
    [SerializeField] BannerPosition _bannerPosition = BannerPosition.TOP_CENTER;

    public bool isBannerVisible = false;

    bool isLoading = false;
    bool pendingShow = false;

    void Awake()
    {
        _adUnitId = _androidAdUnitId;

        // ALWAYS set banner position here
        Advertisement.Banner.SetPosition(_bannerPosition);

        if (_bannerButton != null)
        {
            _bannerButton.onClick.RemoveAllListeners();
            _bannerButton.onClick.AddListener(ToggleBanner);
            _bannerButton.interactable = false;
        }
    }

    public void SetButton(Button button)
    {
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ToggleBanner);
        _bannerButton = button;
        _bannerButton.interactable = false;
    }

    public void LoadBanner()
    {
        if (isLoading) return;
        StartCoroutine(LoadBannerRoutine());
    }

    IEnumerator LoadBannerRoutine()
    {
        isLoading = true;

        while (!Advertisement.isInitialized)
            yield return null;

        Debug.Log("Loading Banner ad: " + _adUnitId);

        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        Advertisement.Banner.Load(_adUnitId, options);

        isLoading = false;
    }

    void OnBannerLoaded()
    {
        Debug.Log("Banner ad loaded!");
        if (_bannerButton != null)
            _bannerButton.interactable = true;

        if (pendingShow)
        {
            ShowBannerImmediate();
            pendingShow = false;
        }
    }

    void OnBannerError(string message)
    {
        Debug.LogWarning("Banner Error: " + message);
        pendingShow = false;
        StartCoroutine(RetryLoad());
    }

    IEnumerator RetryLoad()
    {
        yield return new WaitForSeconds(5f);
        LoadBanner();
    }

    public void ToggleBanner()
    {
        if (isBannerVisible)
            HideBannerAd();
        else
            ShowBannerImmediate();
    }

    public void ShowBannerImmediate()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Banner: Ads not initialized yet, will show after load.");
            pendingShow = true;
            LoadBanner();
            return;
        }

        // 🔥 IMPORTANT: set position *right before* showing
        Advertisement.Banner.SetPosition(_bannerPosition);

        BannerOptions options = new BannerOptions
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };

        Advertisement.Banner.Show(_adUnitId, options);
    }

    public void HideBannerAd()
    {
        Advertisement.Banner.Hide();
        isBannerVisible = false;
        pendingShow = false;
    }

    void OnBannerClicked()
    {
        Debug.Log("User clicked on banner ad.");
    }

    void OnBannerHidden()
    {
        Debug.Log("Banner hidden.");
        isBannerVisible = false;
    }

    void OnBannerShown()
    {
        Debug.Log("Banner shown.");
        isBannerVisible = true;
    }

    // Used by AdManager to control visibility per scene
    public void EnsureVisible(bool visible)
    {
        if (visible)
        {
            if (isBannerVisible) return;
            pendingShow = true;
            LoadBanner();
        }
        else
        {
            if (isBannerVisible)
                HideBannerAd();
            pendingShow = false;
        }
    }

    // Optional helper if you ever want to change position at runtime
    public void SetBannerPosition(BannerPosition pos)
    {
        _bannerPosition = pos;
        Advertisement.Banner.SetPosition(_bannerPosition);
        if (isBannerVisible)
        {
            // Force re-show in new position
            HideBannerAd();
            ShowBannerImmediate();
        }
    }
}
