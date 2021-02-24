using UnityEngine;
using System;
using GoogleMobileAds.Api;
using System.Collections.Generic;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour
{
    private BannerView bannerView;
    private InterstitialAd interstitial;

    private SaveAndLoadData mSaveAndLoadData;
    

    static public AdsManager Instance
    {
        private set;
        get;
    }

    #region Unity Method



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        mSaveAndLoadData = SaveAndLoadData.Instance;

        //MobileAds.Initialize(initStatus => { });
        MobileAds.Initialize("ca-app-pub-3940256099942544~3347511713");

        //Advertisement.Show();

    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    #endregion

    

    public void RequestBanner()
    {

        if(mSaveAndLoadData != null)
        {
            if (mSaveAndLoadData.IsRemoveAds)
                return;
        }

#if UNITY_ANDROID
            string adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
        string adUnitId = "unexpected_platform";
#endif

        this.bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);

        // Called when an ad request has successfully loaded.
        this.bannerView.OnAdLoaded += this.HandleOnAdLoaded;
        // Called when an ad request failed to load.
        this.bannerView.OnAdFailedToLoad += this.HandleOnAdFailedToLoad;
        // Called when an ad is clicked.
        this.bannerView.OnAdOpening += this.HandleOnAdOpened;
        // Called when the user returned from the app after an ad click.
        this.bannerView.OnAdClosed += this.HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        this.bannerView.OnAdLeavingApplication += this.HandleOnAdLeavingApplication;

        // Create an empty ad request.
        // Load the banner with the request.
        this.bannerView.LoadAd(CreateAdRequest());
    }


    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder()
            .AddTestDevice(AdRequest.TestDeviceSimulator)
            .AddTestDevice("0123456789ABCDEF0123456789ABCDEF")
            .AddKeyword("Bravo")
            .SetGender(Gender.Male)
            .SetBirthday(new DateTime(1991, 5, 14))
            .TagForChildDirectedTreatment(false)
            .AddExtra("color_bg", "FFAF15")
            .Build();
    }

    public void HideBanner()
    {
        if (bannerView != null)
            bannerView.Hide();

    }

    public void RequestInterstitial()
    {
        if (mSaveAndLoadData != null)
        {
            if (mSaveAndLoadData.IsRemoveAds)
                return;
        }

#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
        string adUnitId = "unexpected_platform";
#endif
        Debug.Log("Request interstitial ads");
        // Initialize an InterstitialAd.
        this.interstitial = new InterstitialAd(adUnitId);
        
        // Load the interstitial with the request.
        this.interstitial.LoadAd(CreateAdRequest());
        this.interstitial.Show();   

    }

    public void RemoveAds()
    {
        if (mSaveAndLoadData != null)
            mSaveAndLoadData.UpdateAdsState();

        if (bannerView != null)
            bannerView.Hide();
    }

    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLoaded event received");
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print("HandleFailedToReceiveAd event received with message: "
                            + args.Message);
    }

    public void HandleOnAdOpened(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdOpened event received");
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdClosed event received");
    }

    public void HandleOnAdLeavingApplication(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdLeavingApplication event received");
    }
}
