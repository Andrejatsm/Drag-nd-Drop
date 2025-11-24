using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class BannerAd : MonoBehaviour
{
    [SerializeField] string _androidAdUnitId = "Banner_Android";
    string _adUnitId;

    [SerializeField] Button _bannerButton;
    public bool isBannerVisible = false;

    [SerializeField] BannerPosition _bannerPosition = BannerPosition.BOTTOM_CENTER;

    // Internal state for show-after-load
    private bool pendingShow = false;

    private void Awake()
    {
        _adUnitId = _androidAdUnitId;
        Advertisement.Banner.SetPosition(_bannerPosition);
    }

    public void LoadBanner()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.Log("Tried to load banner ad before Unity ads was initialized!");
            return;
        }

        Debug.Log("Loading Banner ad!");
        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        Advertisement.Banner.Load(_adUnitId, options);
    }

    void OnBannerLoaded()
    {
        Debug.Log("Banner ad loaded!");
        if (_bannerButton != null) _bannerButton.interactable = true;

        if (pendingShow)
        {
            ShowBannerImmediate();
            pendingShow = false;
        }
    }

    void OnBannerError(string message)
    {
        Debug.LogWarning("Banner Error: " + message);
        // retry after a short delay
        pendingShow = false;
        LoadBanner();
    }

    public void ShowBannerAd()
    {
        if (isBannerVisible)
        {
            HideBannerAd();

        }
        else
        {
            ShowBannerImmediate();
        }
    }

    public void ShowBannerImmediate()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Banner: Advertisement not initialized yet.");
            pendingShow = true;
            return;
        }

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
        Debug.Log("User clicked on banner ad!");
    }

    void OnBannerHidden()
    {
        Debug.Log("Banner is hidden!");
        isBannerVisible = false;
    }

    void OnBannerShown()
    {
        Debug.Log("Banner ad is visible!");
        isBannerVisible = true;
    }

    public void SetButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowBannerAd);
        _bannerButton = button;
        _bannerButton.interactable = false;
    }

    // Ensure banner is visible or hidden; will load and show if necessary.
    public void EnsureVisible(bool visible)
    {
        if (visible)
        {
            if (isBannerVisible)
                return;

            // If already loading, set pendingShow
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
}