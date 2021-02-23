    using System;
using UnityEngine;
using GoogleMobileAds.Api;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.Advertisements;

public class GoogleMobileAdsScript : MonoBehaviour
{

    public static BannerView bannerView;
    private BannerView tempBannerView;
    private InterstitialAd interstitial;
    private RewardBasedVideoAd rewardBasedVideo;
    private float deltaTime = 0.0f;
    private static string outputMessage = string.Empty;
    public static bool isSpaceUI = false;
    public static GoogleMobileAdsScript instance;
    [SerializeField]
    private AdPosition bannerPosition;
    [Header("Android Settings")]
    [Header("When settings empty used Ads default of google")]
    [Space()]
    [SerializeField]
    private string APP_ANDROID_ID = string.Empty;
    [SerializeField]
    private string Admob_Banner_ANDROID_ID = string.Empty;
    [SerializeField]
    private string Admob_Interstitial_ANDROID_ID = string.Empty;
    [SerializeField]
    private string Admob_Reward_ANDROID_ID = string.Empty;

    [Space()]
    [Header("IOS Settings")]
    [Space()]
    [SerializeField]
    private string APP_IOS_ID = string.Empty;
    [SerializeField]
    private string Admob_Banner_IOS_ID = string.Empty;
    [SerializeField]
    private string Admob_Interstitial_IOS_ID = string.Empty;
    [SerializeField]
    private string Admob_Reward_IOS_ID = string.Empty;

    [Space()]
    [Header("Unity Settings")]
    [Space()]
    [SerializeField]
    private string UnityAds_GameID_ANDROID = string.Empty;
    [SerializeField]
    private string UnityAds_GameID_IOS = string.Empty;
    [SerializeField]
    private bool testMode;
    private static readonly string mPlacementRewardUnityAds = "rewardedVideo";
    private static readonly string mPlacementBannerUnityAds = "banner";
    private static readonly string mPlacementInterstitialUnityAds = "interstitialVideo";

    [Space()]
    [SerializeField]
    private bool showGUI = false;
    [SerializeField]
    private bool showBanner = false;


    public static string OutputMessage
    {
        set { outputMessage = value; }
    }
    public void RequestVideo()
    {
        this.RequestInterstitial();
        this.RequestRewardBasedVideo();
    }
    public void Start()
    {

#if UNITY_ANDROID 
        string appId = (string.IsNullOrEmpty(APP_ANDROID_ID)) ? "ca-app-pub-3940256099942544~3347511713" : APP_ANDROID_ID;

#elif UNITY_IPHONE
        string appId = (string.IsNullOrEmpty(APP_IOS_ID)) ? "ca-app-pub-3940256099942544~1458002511" : APP_IOS_ID;
#else
        string appId = "unexpected_platform";
#endif

        MobileAds.SetiOSAppPauseOnBackground(true);

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(appId);

        // Get singleton reward based video ad reference.
        this.rewardBasedVideo = RewardBasedVideoAd.Instance;

        // RewardBasedVideoAd is a singleton, so handlers should only be registered once.
        rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
        rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;

        if (bannerView == null)
        {
            this.RequestBanner();
        }

        if (showBanner)
        {
            if (bannerView != null)
            {
                bannerView.Show();
            }
        }
        else
        {
            if (bannerView != null)
            {
                bannerView.Hide();
            }
        }

        this.RequestRewardBasedVideo();
        this.RequestInterstitial();
        instance = this;

    }



    public void OnGUI()
    {
        if (!showGUI) return;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        style.alignment = TextAnchor.LowerRight;
        style.fontSize = (int)(Screen.height * 0.06);
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        float fps = 1.0f / this.deltaTime;
        string text = string.Format("{0:0.} fps", fps);
        GUI.Label(rect, text, style);

        // Puts some basic buttons onto the screen.
        GUI.skin.button.fontSize = (int)(0.035f * Screen.width);
        float buttonWidth = 0.35f * Screen.width;
        float buttonHeight = 0.15f * Screen.height;
        float columnOnePosition = 0.1f * Screen.width;
        float columnTwoPosition = 0.55f * Screen.width;

        Rect requestBannerRect = new Rect(
            columnOnePosition,
            0.05f * Screen.height,
            buttonWidth,
            buttonHeight);
        if (GUI.Button(requestBannerRect, "Request\nBanner"))
        {
            this.RequestBanner();
        }

        Rect destroyBannerRect = new Rect(
            columnOnePosition,
            0.225f * Screen.height,
            buttonWidth,
            buttonHeight);
        if (GUI.Button(destroyBannerRect, "Destroy\nBanner"))
        {
            bannerView.Destroy();
        }

        Rect requestInterstitialRect = new Rect(
            columnOnePosition,
            0.4f * Screen.height,
            buttonWidth,
            buttonHeight);
        if (GUI.Button(requestInterstitialRect, "Request\nInterstitial"))
        {
            this.RequestInterstitial();
        }

        Rect showInterstitialRect = new Rect(
            columnOnePosition,
            0.575f * Screen.height,
            buttonWidth,
            buttonHeight);
        if (GUI.Button(showInterstitialRect, "Show\nInterstitial"))
        {
            this.ShowInterstitial();
        }

        Rect destroyInterstitialRect = new Rect(
            columnOnePosition,
            0.75f * Screen.height,
            buttonWidth,
            buttonHeight);
        if (GUI.Button(destroyInterstitialRect, "Destroy\nInterstitial"))
        {
            this.interstitial.Destroy();
        }

        Rect requestRewardedRect = new Rect(
            columnTwoPosition,
            0.05f * Screen.height,
            buttonWidth,
            buttonHeight);
        if (GUI.Button(requestRewardedRect, "Request\nRewarded Video"))
        {
            this.RequestRewardBasedVideo();
        }

        Rect showRewardedRect = new Rect(
            columnTwoPosition,
            0.225f * Screen.height,
            buttonWidth,
            buttonHeight);
        if (GUI.Button(showRewardedRect, "Show\nRewarded Video"))
        {
            this.ShowRewardBasedVideo();
        }

        Rect textOutputRect = new Rect(
            columnTwoPosition,
            0.925f * Screen.height,
            buttonWidth,
            0.05f * Screen.height);
        GUI.Label(textOutputRect, outputMessage);

    }

    // Returns an ad request with custom ad targeting.
    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder()
            .AddTestDevice(AdRequest.TestDeviceSimulator)
            .AddTestDevice("0123456789ABCDEF0123456789ABCDEF")
            .AddKeyword("game")
            .SetGender(Gender.Male)
            .SetBirthday(new DateTime(1985, 1, 1))
            .TagForChildDirectedTreatment(false)
            .AddExtra("color_bg", "9B30FF")
            .Build();
    }

    private void RequestBanner()
    {
        // These ad units are configured to always serve test ads.
#if UNITY_EDITOR
        string adUnitId = "unused";

#elif UNITY_ANDROID
         string adUnitId = (Admob_Banner_ANDROID_ID == string.Empty) ? "ca-app-pub-3940256099942544/6300978111" : Admob_Banner_ANDROID_ID;
        //string adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
          string adUnitId = (Admob_Banner_IOS_ID == string.Empty) ? "ca-app-pub-3940256099942544/2934735716" : Admob_Banner_IOS_ID;
       // string adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
        string adUnitId = "unexpected_platform";
#endif
        // Clean up banner ad before creating a new one.
        if (bannerView != null)
        {
            bannerView.Destroy();
        }
        bannerPosition = AdPosition.Bottom;
        // Create a 320x50 banner at the top of the screen.
        bannerView = new BannerView(adUnitId, AdSize.Banner, bannerPosition);

        // Register for ad events.
        bannerView.OnAdLoaded += this.HandleAdLoaded;
        bannerView.OnAdFailedToLoad += this.HandleAdFailedToLoad;
        bannerView.OnAdOpening += this.HandleAdOpened;
        bannerView.OnAdClosed += this.HandleAdClosed;
        bannerView.OnAdLeavingApplication += this.HandleAdLeftApplication;

        // Load a banner ad.
        bannerView.LoadAd(this.CreateAdRequest());
        this.tempBannerView = bannerView;




    }


    public void VisbileBanner(bool visible)
    {
        if (visible)
        {
            bannerView.Show();
        }
        else
        {
            bannerView.Hide();
        }


    }
    public void RequestInterstitial()
    {
        // These ad units are configured to always serve test ads.
#if UNITY_EDITOR
        string adUnitId = "unused";


#elif UNITY_ANDROID
           string adUnitId = (Admob_Interstitial_ANDROID_ID == string.Empty) ? "ca-app-pub-3940256099942544/1033173712" : Admob_Interstitial_ANDROID_ID;
        //string adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
         string adUnitId = (Admob_Interstitial_IOS_ID == string.Empty) ? "ca-app-pub-3940256099942544/4411468910" : Admob_Interstitial_IOS_ID;
        //string adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
        string adUnitId = "unexpected_platform";
#endif


        // Clean up interstitial ad before creating a new one.
        // if (this.interstitial != null)
        // {
        // this.interstitial.Destroy();
        //}

        // Create an interstitial.
        this.interstitial = new InterstitialAd(adUnitId);

        // Register for ad events.
        this.interstitial.OnAdLoaded += this.HandleInterstitialLoaded;
        this.interstitial.OnAdFailedToLoad += this.HandleInterstitialFailedToLoad;
        this.interstitial.OnAdOpening += this.HandleInterstitialOpened;
        this.interstitial.OnAdClosed += this.HandleInterstitialClosed;
        this.interstitial.OnAdLeavingApplication += this.HandleInterstitialLeftApplication;

        // Load an interstitial ad.
        this.interstitial.LoadAd(this.CreateAdRequest());
    }

    public void RequestRewardBasedVideo()
    {


#if UNITY_EDITOR
        string adUnitId = "unused";

#elif UNITY_ANDROID
          string adUnitId = (Admob_Reward_ANDROID_ID == string.Empty) ? "ca-app-pub-3940256099942544/5224354917" : Admob_Reward_ANDROID_ID;
       // string adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE

          string adUnitId = (Admob_Reward_IOS_ID == string.Empty) ? "ca-app-pub-3940256099942544/1712485313" : Admob_Reward_IOS_ID;
      //  string adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
        string adUnitId = "unexpected_platform";
#endif
        AdRequest request = new AdRequest.Builder().Build();
        this.rewardBasedVideo.LoadAd(request, adUnitId);


    }

    public void ShowInterstitial()
    {
        if (this.interstitial.IsLoaded())
        {
            this.interstitial.Show();
            this.RequestInterstitial();
        }
        else
        {
            MonoBehaviour.print("Interstitial is not ready yet");
        }
    }

    public bool CheckRewardBasedVideo()
    {


        return rewardBasedVideo.IsLoaded();
    }

    public void ShowRewardBasedVideo(UnityAction action = null)
    {
        if (CheckRewardBasedVideo())
        {
            rewardVideoAction = action;
            this.rewardBasedVideo.Show();
        }




    }

    #region Banner callback handlers

    public void HandleAdLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLoaded event received");

    }

    public void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print("HandleFailedToReceiveAd event received with message: " + args.Message);
    }

    public void HandleAdOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdOpened event received");

    }

    public void HandleAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdClosed event received");
    }

    public void HandleAdLeftApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLeftApplication event received");
    }

    #endregion

    #region Interstitial callback handlers

    public void HandleInterstitialLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleInterstitialLoaded event received");
    }

    public void HandleInterstitialFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print(
            "HandleInterstitialFailedToLoad event received with message: " + args.Message);
    }

    public void HandleInterstitialOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleInterstitialOpened event received");
    }

    public void HandleInterstitialClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleInterstitialClosed event received");
    }

    public void HandleInterstitialLeftApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleInterstitialLeftApplication event received");
    }

    #endregion

    #region RewardBasedVideo callback handlers


    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        this.RequestRewardBasedVideo();
    }


    private UnityAction rewardVideoAction;
    private bool isUpdate = false;
    private void Update()
    {
        if (!isUpdate) return;
        if (rewardVideoAction != null)
        {
            rewardVideoAction();

        }
        rewardVideoAction = null;
        isUpdate = false;
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        Debug.Log("HandleRewardBasedVideoRewarded");
        isUpdate = true;
        this.RequestRewardBasedVideo();

    }









    public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoLeftApplication event received");
    }


    #endregion
}
