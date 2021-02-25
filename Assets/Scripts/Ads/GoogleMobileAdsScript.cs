using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Advertisements;
//using AudienceNetwork;

public class GoogleMobileAdsScript : MonoBehaviour, IUnityAdsListener
{
    public static GoogleMobileAds.Api.BannerView bannerView;
    private GoogleMobileAds.Api.InterstitialAd interstitial;
    private GoogleMobileAds.Api.RewardBasedVideoAd rewardBasedVideo;
    private float deltaTime = 0.0f;
    private static string outputMessage = string.Empty;
    public static GoogleMobileAdsScript instance;
    [SerializeField]
    private GoogleMobileAds.Api.AdPosition bannerPosition;
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
    private static readonly string mPlacementBannerUnityAds = "bannerAds";
    private static readonly string mPlacementInterstitialUnityAds = "video";

    [Space()]
    [Header("FAN Settings")]
    [Space()]
    [SerializeField] private AudienceNetwork.AdSize bannerFanSize;// = (AudienceNetwork.AdSize[])Enum.GetValues(typeof(AudienceNetwork.AdSize));
    private AudienceNetwork.AdView adView;
    private AudienceNetwork.RewardedVideoAd rewardedVideoAd;
    private AudienceNetwork.InterstitialAd interstitialAd;

    [Space()]
    [SerializeField] private bool isTurnOnUnity = true;
    [SerializeField] private bool isTurnOnAdsMob = true;
    [SerializeField] private bool isTurnOnFan = true;
    [SerializeField]
    private bool showGUI = false;
    [SerializeField]
    private bool showBanner = false;
    /*Priority show ads : Unity, Admobs, Fan*/

    enum TypeAdsLoaded { Banner, Interstitial, Reward};

    public static string OutputMessage
    {
        set { outputMessage = value; }
    }
    public void RequestVideo()
    {
        this.RequestInterstitialAdmobs();
        this.RequestRewardBasedVideoAdmobs();
    }
    public void Start()
    {
        InitializeAdsMob();
        InitializeUnityAds();

#if !UNITY_EDITOR
        InitializeFan();
#endif
        instance = this;
    }

#region Initialize 
    private void InitializeAdsMob()
    {
#if UNITY_ANDROID
        string appId = (string.IsNullOrEmpty(APP_ANDROID_ID)) ? "ca-app-pub-3940256099942544~3347511713" : APP_ANDROID_ID;

#elif UNITY_IPHONE
        string appId = (string.IsNullOrEmpty(APP_IOS_ID)) ? "ca-app-pub-3940256099942544~1458002511" : APP_IOS_ID;
#else
        string appId = "unexpected_platform";
#endif

        GoogleMobileAds.Api.MobileAds.SetiOSAppPauseOnBackground(true);

        // Initialize the Google Mobile Ads SDK.
        GoogleMobileAds.Api.MobileAds.Initialize(appId);

        // Get singleton reward based video ad reference.
        this.rewardBasedVideo = GoogleMobileAds.Api.RewardBasedVideoAd.Instance;

        // RewardBasedVideoAd is a singleton, so handlers should only be registered once.
        rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
        rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        this.RequestBannerAdmobs();
        this.RequestRewardBasedVideoAdmobs();
        this.RequestInterstitialAdmobs();
    }

    private void InitializeUnityAds()
    {
#if UNITY_ANDROID
        Advertisement.Initialize(UnityAds_GameID_ANDROID, testMode);

#elif UNITY_IPHONE
        Advertisement.Initialize(UnityAds_GameID_IOS,testMode);
#else
        Advertisement.Initialize("unexpected_platform", testMode);
#endif

        Advertisement.AddListener(this);

    }

    private void InitializeFan()
    {
        AudienceNetwork.AudienceNetworkAds.Initialize();
        RequestBannerFan();
        RequestInterstitialFan();
        RequestRewardedVideoFan();
    }    

#endregion

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
            //this.RequestBanner();
            //ShowBannerUnityAds();
            RequestBannerFan();
        }

        Rect destroyBannerRect = new Rect(
            columnOnePosition,
            0.225f * Screen.height,
            buttonWidth,
            buttonHeight);
        if (GUI.Button(destroyBannerRect, "Destroy\nBanner"))
        {
            //bannerView.Destroy();
            //HideBannerUnityAds();
        }

        Rect requestInterstitialRect = new Rect(
            columnOnePosition,
            0.4f * Screen.height,
            buttonWidth,
            buttonHeight);
        if (GUI.Button(requestInterstitialRect, "Request\nInterstitial"))
        {
            //this.RequestInterstitial();
            //ShowInterstitialUnityAds();
            RequestInterstitialFan();
        }

        Rect showInterstitialRect = new Rect(
            columnOnePosition,
            0.575f * Screen.height,
            buttonWidth,
            buttonHeight);
        if (GUI.Button(showInterstitialRect, "Show\nInterstitial"))
        {
            //this.ShowInterstitial();
            ShowInterstitialFan();
        }

        Rect destroyInterstitialRect = new Rect(
            columnOnePosition,
            0.75f * Screen.height,
            buttonWidth,
            buttonHeight);
        if (GUI.Button(destroyInterstitialRect, "Destroy\nInterstitial"))
        {
            //this.interstitial.Destroy();
        }

        Rect requestRewardedRect = new Rect(
            columnTwoPosition,
            0.05f * Screen.height,
            buttonWidth,
            buttonHeight);
        if (GUI.Button(requestRewardedRect, "Request\nRewarded Video"))
        {
            //this.RequestRewardBasedVideo();
            //ShowVideoRewardUnityAds();
            RequestRewardedVideoFan();
        }

        Rect showRewardedRect = new Rect(
            columnTwoPosition,
            0.225f * Screen.height,
            buttonWidth,
            buttonHeight);
        if (GUI.Button(showRewardedRect, "Show\nRewarded Video"))
        {
            //this.ShowRewardBasedVideo();
            //ShowVideoRewardUnityAds();
            ShowRewardedVideoFan();
        }

        Rect textOutputRect = new Rect(
            columnTwoPosition,
            0.925f * Screen.height,
            buttonWidth,
            0.05f * Screen.height);
        GUI.Label(textOutputRect, outputMessage);

    }


#region AdMobs Request And Show
    // Returns an ad request with custom ad targeting.
    private GoogleMobileAds.Api.AdRequest CreateAdRequest()
    {
        return new GoogleMobileAds.Api.AdRequest.Builder()
            .AddTestDevice(GoogleMobileAds.Api.AdRequest.TestDeviceSimulator)
            .AddTestDevice("0123456789ABCDEF0123456789ABCDEF")
            .AddKeyword("game")
            .SetGender(GoogleMobileAds.Api.Gender.Male)
            .SetBirthday(new DateTime(1985, 1, 1))
            .TagForChildDirectedTreatment(false)
            .AddExtra("color_bg", "9B30FF")
            .Build();
    }

    private void RequestBannerAdmobs()
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
        bannerPosition = GoogleMobileAds.Api.AdPosition.Bottom;
        // Create a 320x50 banner at the top of the screen.
        bannerView = new GoogleMobileAds.Api.BannerView(adUnitId, GoogleMobileAds.Api.AdSize.Banner, bannerPosition);

        // Register for ad events.
        bannerView.OnAdLoaded += this.HandleAdLoaded;
        bannerView.OnAdFailedToLoad += this.HandleAdFailedToLoad;
        bannerView.OnAdOpening += this.HandleAdOpened;
        bannerView.OnAdClosed += this.HandleAdClosed;
        bannerView.OnAdLeavingApplication += this.HandleAdLeftApplication;

        // Load a banner ad.
        bannerView.LoadAd(this.CreateAdRequest());

        ShowBannerAdmobs(false);
    }

    public bool CheckBannerAdmobs()
    {
        if(bannerView != null)
        {
            return isTurnOnAdsMob;
        }

        return false;
    }

    public void ShowBannerAdmobs(bool visible)
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

    public void RequestInterstitialAdmobs()
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
        this.interstitial = new GoogleMobileAds.Api.InterstitialAd(adUnitId);

        // Register for ad events.
        this.interstitial.OnAdLoaded += this.HandleInterstitialLoaded;
        this.interstitial.OnAdFailedToLoad += this.HandleInterstitialFailedToLoad;
        this.interstitial.OnAdOpening += this.HandleInterstitialOpened;
        this.interstitial.OnAdClosed += this.HandleInterstitialClosed;
        this.interstitial.OnAdLeavingApplication += this.HandleInterstitialLeftApplication;

        // Load an interstitial ad.
        this.interstitial.LoadAd(this.CreateAdRequest());
    }

    public void RequestRewardBasedVideoAdmobs()
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
        GoogleMobileAds.Api.AdRequest request = new GoogleMobileAds.Api.AdRequest.Builder().Build();
        this.rewardBasedVideo.LoadAd(request, adUnitId);


    }

    public bool CheckInterstitialAdmobs()
    {
        return isTurnOnAdsMob && interstitial.IsLoaded();
    }

    public void ShowInterstitialAdmobs()
    {
        this.interstitial.Show();
        this.RequestInterstitialAdmobs();
    }

    public bool CheckRewardBasedVideoAdmobs()
    {
        return isTurnOnAdsMob && rewardBasedVideo.IsLoaded();
    }

    public void ShowRewardBasedVideoAdmobs(UnityAction action = null)
    {
        rewardVideoAction = action;
        this.rewardBasedVideo.Show();
        RequestRewardBasedVideoAdmobs();
    }

    public void EnableAdsMob()
    {
        isTurnOnAdsMob = true;
    }

    public void DisableAdsMob()
    {
        isTurnOnAdsMob = false;
    }

#endregion

#region UnityAds Request And Show

    public bool CheckInterstitialUnityAds()
    {
        return isTurnOnUnity && Advertisement.IsReady(mPlacementInterstitialUnityAds);
    }

    public void ShowInterstitialUnityAds()
    {
        Advertisement.Show(mPlacementInterstitialUnityAds);

    }

    public bool CheckBannerUnityAds()
    {
        return isTurnOnUnity && (Advertisement.IsReady(mPlacementBannerUnityAds) || Advertisement.Banner.isLoaded);
    }

    public void ShowBannerUnityAds()
    {
        ShowOrHideBannerUnityAds(true);
    }

    public void HideBannerUnityAds()
    {
        Advertisement.Banner.Hide();
    }

    public void ShowOrHideBannerUnityAds(bool isVisible, BannerPosition position = BannerPosition.BOTTOM_CENTER)
    {
        if (CheckBannerUnityAds() || Advertisement.Banner.isLoaded)
        {
            Advertisement.Banner.SetPosition(position);

            if (isVisible)
            {
                BannerLoadOptions loadOptions = new BannerLoadOptions
                {
                    loadCallback = OnBannerLoaded,
                    errorCallback = OnBannerError
                };

                Advertisement.Banner.Load(mPlacementBannerUnityAds, loadOptions);

#if UNITY_EDITOR
                Advertisement.Banner.Show(mPlacementBannerUnityAds);
#endif
            }
            else
            {
                Advertisement.Banner.Hide();
            }
        }
    }

    void OnBannerLoaded()
    {
        Debug.Log("Banner Loaded");
        Advertisement.Banner.Show(mPlacementBannerUnityAds);
    }

    void OnBannerError(string error)
    {
        Debug.Log("Banner Error: " + error);
    }

    public bool CheckVideoRewardUnityAds()
    {
        return isTurnOnUnity && Advertisement.IsReady(mPlacementRewardUnityAds);
    }

    public void ShowVideoRewardUnityAds()
    {
        Advertisement.Show(mPlacementRewardUnityAds);
    }

    public void EnableUnityAds()
    {
        isTurnOnUnity = true;
    }

    public void DisableUnityAds()
    {
        isTurnOnUnity = false;
    }
    
    public bool IsReadyUnityAds(string placementId)
    {
        return isTurnOnUnity && Advertisement.IsReady(placementId);
    }

#endregion

#region FAN Request And Show

    public void RequestBannerFan()
    {
        if (adView)
        {
            adView.Dispose();
        }


        // Create a banner's ad view with a unique placement ID
        // (generate your own on the Facebook app settings).
        // Use different ID for each ad placement in your app.
        adView = new AudienceNetwork.AdView("183898216451495_183899426451374", bannerFanSize);


        adView.Register(gameObject);

        // Set delegates to get notified on changes or when the user interacts
        // with the ad.
        adView.AdViewDidLoad = delegate ()
        {
            Debug.Log("Banner loaded");
        };
        adView.AdViewDidFailWithError = delegate (string error)
        {
            Debug.Log("Banner failed to load with error: " + error);
        };
        adView.AdViewWillLogImpression = delegate ()
        {
            Debug.Log("Banner logged impression.");
        };
        adView.AdViewDidClick = delegate ()
        {
            Debug.Log("Banner clicked.");
        };

        // Initiate a request to load an ad.
        adView.LoadAd();
    }

    public bool CheckBannerFan()
    {
        return isTurnOnFan && adView.IsValid();
    }

    public void ShowBannerFan()
    {
        adView.Show(AudienceNetwork.AdPosition.BOTTOM);
    }

    public void HideBannerFan()
    {
        adView.Dispose();
        RequestBannerFan();
    }

    public void RequestRewardedVideoFan()
    {

        // Create the rewarded video unit with a placement ID (generate your own on the Facebook app settings).
        // Use different ID for each ad placement in your app.
        rewardedVideoAd = new AudienceNetwork.RewardedVideoAd("183898216451495_183899426451374");

        // For S2S validation you can create the rewarded video ad with the reward data
        // Refer to documentation here:
        // https://developers.facebook.com/docs/audience-network/android/rewarded-video#server-side-reward-validation
        // https://developers.facebook.com/docs/audience-network/ios/rewarded-video#server-side-reward-validation
        AudienceNetwork.RewardData rewardData = new AudienceNetwork.RewardData
        {
            UserId = "USER_ID",
            Currency = "REWARD_ID"
        };
#pragma warning disable 0219
        AudienceNetwork.RewardedVideoAd s2sRewardedVideoAd = new AudienceNetwork.RewardedVideoAd("YOUR_PLACEMENT_ID", rewardData);
#pragma warning restore 0219

        rewardedVideoAd.Register(gameObject);

        // Set delegates to get notified on changes or when the user interacts with the ad.
        rewardedVideoAd.RewardedVideoAdDidLoad = delegate ()
        {
            Debug.Log("RewardedVideo ad loaded.");
        };
        rewardedVideoAd.RewardedVideoAdDidFailWithError = delegate (string error)
        {
            Debug.Log("RewardedVideo ad failed to load with error: " + error);
        };
        rewardedVideoAd.RewardedVideoAdWillLogImpression = delegate ()
        {
            Debug.Log("RewardedVideo ad logged impression.");
        };
        rewardedVideoAd.RewardedVideoAdDidClick = delegate ()
        {
            Debug.Log("RewardedVideo ad clicked.");
        };

        // For S2S validation you need to register the following two callback
        // Refer to documentation here:
        // https://developers.facebook.com/docs/audience-network/android/rewarded-video#server-side-reward-validation
        // https://developers.facebook.com/docs/audience-network/ios/rewarded-video#server-side-reward-validation
        rewardedVideoAd.RewardedVideoAdDidSucceed = delegate ()
        {
            Debug.Log("Rewarded video ad validated by server");
        };

        rewardedVideoAd.RewardedVideoAdDidFail = delegate ()
        {
            Debug.Log("Rewarded video ad not validated, or no response from server");
        };

        rewardedVideoAd.RewardedVideoAdDidClose = delegate ()
        {
            Debug.Log("Rewarded video ad did close.");
            if (rewardedVideoAd != null)
            {
                rewardedVideoAd.Dispose();
            }
        };

#if UNITY_ANDROID
        /*
         * Only relevant to Android.
         * This callback will only be triggered if the Rewarded Video activity
         * has been destroyed without being properly closed. This can happen if
         * an app with launchMode:singleTask (such as a Unity game) goes to
         * background and is then relaunched by tapping the icon.
         */
        rewardedVideoAd.RewardedVideoAdActivityDestroyed = delegate ()
        {
                Debug.Log("Rewarded video activity destroyed without being closed first.");
                Debug.Log("Game should resume. User should not get a reward.");
        };
#endif

        // Initiate the request to load the ad.
        rewardedVideoAd.LoadAd();
    }

    public bool CheckRewardVideoFan()
    {
        return isTurnOnFan && rewardedVideoAd.IsValid();
    }

    public void ShowRewardedVideoFan() // need to add check
    {
        rewardedVideoAd.Show();
        RequestRewardedVideoFan();
    }

    public void RequestInterstitialFan()
    {
        // Create the interstitial unit with a placement ID (generate your own on the Facebook app settings).
        // Use different ID for each ad placement in your app.
        interstitialAd = new AudienceNetwork.InterstitialAd("183898216451495_183899426451374");

        interstitialAd.Register(gameObject);

        // Set delegates to get notified on changes or when the user interacts with the ad.
        interstitialAd.InterstitialAdDidLoad = delegate ()
        {
            Debug.Log("Interstitial ad loaded.");
        };
        interstitialAd.InterstitialAdDidFailWithError = delegate (string error)
        {
            Debug.Log("Interstitial ad failed to load with error: " + error);
        };
        interstitialAd.InterstitialAdWillLogImpression = delegate ()
        {
            Debug.Log("Interstitial ad logged impression.");
        };
        interstitialAd.InterstitialAdDidClick = delegate ()
        {
            Debug.Log("Interstitial ad clicked.");
        };
        interstitialAd.InterstitialAdDidClose = delegate ()
        {
            Debug.Log("Interstitial ad did close.");
            if (interstitialAd != null)
            {
                interstitialAd.Dispose();
            }
        };

#if UNITY_ANDROID
        /*
         * Only relevant to Android.
         * This callback will only be triggered if the Interstitial activity has
         * been destroyed without being properly closed. This can happen if an
         * app with launchMode:singleTask (such as a Unity game) goes to
         * background and is then relaunched by tapping the icon.
         */
        interstitialAd.interstitialAdActivityDestroyed = delegate () {
                Debug.Log("Interstitial activity destroyed without being closed first.");
                Debug.Log("Game should resume.");
        };
#endif

        // Initiate the request to load the ad.
        interstitialAd.LoadAd();
    }

    public bool CheckInterstitialFan()
    {
        return isTurnOnFan && interstitialAd.IsValid();
    }

    public void ShowInterstitialFan()
    {
        interstitialAd.Show();
        RequestInterstitialFan();
    }

    public void EnableFan()
    {
        isTurnOnFan = true;
    }

    public void DisableFan()
    {
        isTurnOnFan = false;
    }

#endregion

#region Request And Show Main Function

    public void ShowInterstitial()
    {
        if(CheckInterstitialUnityAds())
        {
            ShowInterstitialUnityAds();
        }
        else if(CheckInterstitialAdmobs())
        {
            ShowInterstitialAdmobs();
        }
        #if !UNITY_EDITOR
        else if(CheckInterstitialFan())
        {
            ShowInterstitialFan();
        }
        #endif
    }

    public void ShowRewardVideo()
    {
        if (CheckVideoRewardUnityAds())
        {
            ShowVideoRewardUnityAds();
        }
        else if (CheckRewardBasedVideoAdmobs())
        {
            ShowRewardBasedVideoAdmobs();
        }
        #if !UNITY_EDITOR
        else if (CheckRewardVideoFan())
        {
            ShowRewardedVideoFan();
        }
        #endif
    }

    public void ShowBanner()
    {
        if(CheckBannerUnityAds())
        {
            ShowBannerUnityAds();
        }
        else if(CheckBannerAdmobs())
        {
            ShowBannerAdmobs(true);
        }

        #if !UNITY_EDITOR
        else if(CheckBannerFan())
        {
            ShowBannerFan();
        }
        #endif
    }    

    public void HideBanner()
    {
        if (CheckBannerUnityAds())
        {
            HideBannerUnityAds();
        }

        if (CheckBannerAdmobs())
        {
            ShowBannerAdmobs(false);
        }
        #if !UNITY_EDITOR
        if (CheckBannerFan())
        {
            HideBannerFan();
        }
        #endif
    }

    #endregion


    #region Banner callback handlers

    public void HandleAdLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLoaded event received");

    }

    public void HandleAdFailedToLoad(object sender, GoogleMobileAds.Api.AdFailedToLoadEventArgs args)
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

    public void HandleInterstitialFailedToLoad(object sender, GoogleMobileAds.Api.AdFailedToLoadEventArgs args)
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
        this.RequestRewardBasedVideoAdmobs();
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

    public void HandleRewardBasedVideoRewarded(object sender, GoogleMobileAds.Api.Reward args)
    {
        Debug.Log("HandleRewardBasedVideoRewarded");
        isUpdate = true;
        this.RequestRewardBasedVideoAdmobs();

    }

    public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardBasedVideoLeftApplication event received");
    }

    public void OnUnityAdsReady(string placementId)
    {
    }

    public void OnUnityAdsDidError(string message)
    {
    }

    public void OnUnityAdsDidStart(string placementId)
    {
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
    }


#endregion
}
