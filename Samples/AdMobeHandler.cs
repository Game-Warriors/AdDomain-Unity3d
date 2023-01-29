using UnityEngine;
using System;
using GameWarriors.AdDomain.Abstraction;

namespace Managements.Handlers.Advertise
{
#if ADMOB || true
    //ca-app-pub-3940256099942544~3347511713 android test app id
    //ca-app-pub-3940256099942544~1458002511 ios test app id


    using GoogleMobileAds.Api;
    using System.Collections.Generic;

    public class AdMobeHandler : IAdVideoHandler, IAdNativeBannerHandler, IAdInterstitialHandler
    {

        private readonly string INTERSTITIAL_ID;
        //private readonly string NATIVE_BANNER_ID;
        private string REWARD_AD_ID;

        private GameObject _nativeBannerObject;
        private RewardedAd _rewardedAd;
        //private AdLoader _nativeAdLoader;
        private InterstitialAd _interstitial;

        private bool _isUnifiedNativeAdLoaded;
        private Action _onVideoAvailable;
        private Action<EVideoAdState> _onLoadVideoFailed;
        private Action<bool, bool> _onAdVideoDone;

        private List<string> _testDevices;

#if NATIVE_AD
        private UnifiedNativeAd _nativeAd;
        public bool HasBanner => _nativeAd != null;
#else
        public bool HasBanner => false;

        public bool IsVideoAvailable => _rewardedAd?.IsLoaded() ?? false;
#endif
        public bool IsInterstitialAvailable => _interstitial?.IsLoaded() ?? false;
        //https://developers.google.com/admob/unity/test-ads
        [UnityEngine.Scripting.Preserve]
        public AdMobeHandler(IAdvertiseConfig advertiseConfig)
        {
            _nativeBannerObject = advertiseConfig.AdUnitNativeBanner(EAdHandlerType.Admobe);
            REWARD_AD_ID = advertiseConfig.GetAdUnitId(EAdHandlerType.Admobe, EUnitAdType.RewardAdId);
            _testDevices = new List<string>() { { "A4B0860103793A14D30DB96346BD45BA" } };
            Application.RequestAdvertisingIdentifierAsync(GetId);
        }

        private void GetId(string advertisingId, bool trackingEnabled, string errorMsg)
        {
            Debug.Log("advertisingId : " + advertisingId);
        }

        public void Initialization(Action onInitializeDone, Action OnVideoAvailable, Action<EVideoAdState> OnVideoUnavailable)
        {
            _onVideoAvailable = OnVideoAvailable;
            _onLoadVideoFailed = OnVideoUnavailable;
            MobileAds.Initialize((input) =>
            {
                InitializeResult(input, onInitializeDone);
            });
        }

        private void InitializeResult(InitializationStatus initState, Action onInitializeDone)
        {
            Debug.Log("ad mobe InitializeResult");
            Dictionary<string, AdapterStatus> map = initState.getAdapterStatusMap();
            foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
            {
                string className = keyValuePair.Key;
                AdapterStatus status = keyValuePair.Value;
                Debug.Log(status.Description);
                switch (status.InitializationState)
                {

                    case AdapterState.NotReady:
                        // The adapter initialization did not complete.
                        MonoBehaviour.print("Adapter: " + className + " not ready.");
                        break;
                    case AdapterState.Ready:
                        // The adapter was successfully initialized.
                        MonoBehaviour.print("Adapter: " + className + " is initialized.");
                        break;
                }
            }
            _rewardedAd = new RewardedAd(REWARD_AD_ID);
            // Called when an ad request has successfully loaded.
            _rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
            // Called when an ad request failed to load.
            _rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
            // Called when an ad is shown.
            _rewardedAd.OnAdOpening += HandleRewardedAdOpening;
            // Called when an ad request failed to show.
            _rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
            // Called when the user should be rewarded for interacting with the ad.
            _rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
            // Called when the ad is closed.
            _rewardedAd.OnAdClosed += HandleRewardedAdClosed;
            onInitializeDone?.Invoke();
        }


        public void LoadNativeBannerAd()
        {
            if (_isUnifiedNativeAdLoaded)
                return;
#if NATIVE_AD
            if (_nativeAd == null)
            {
                _nativeAdLoader = new AdLoader.Builder(NATIVE_BANNER_ID)
                .ForUnifiedNativeAd()
                .Build();
                _nativeAdLoader.OnUnifiedNativeAdLoaded += HandleUnifiedNativeAdLoaded;
            }
            _nativeAdLoader.LoadAd(new AdRequest.Builder().Build());
#endif
        }

        public void ShowNativeBannerAd()
        {
            MonoBehaviour.print("ShowNativeBannerAd");
#if NATIVE_AD
            if (_isUnifiedNativeAdLoaded)
            {
                _isUnifiedNativeAdLoaded = false;
                // Get Texture2D for icon asset of native ad.
                Texture2D iconTexture = _nativeAd.GetIconTexture();
                _nativeBannerObject.SetActive(true);
                //icon.transform.position = new Vector3(1, 1, 1);
                //icon.transform.localScale = new Vector3(1, 1, 1);
                var renderer = _nativeBannerObject.GetComponentInChildren<Renderer>();
                renderer.material.mainTexture = iconTexture;
                string headlineText = _nativeAd.GetHeadlineText();
                _nativeBannerObject.GetComponentInChildren<Text>().text = headlineText;
                _nativeBannerObject.GetComponent<Image>().sprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
                Debug.Log(renderer);
                // Register GameObject that will display icon asset of native ad.
                if (!_nativeAd.RegisterIconImageGameObject(renderer.gameObject))
                {
                    Debug.LogError("Handle failure to register ad asset.");
                }
                // Get Texture2D for icon asset of native ad.
                //Texture2D iconTexture = _nativeAd.GetIconTexture();

                //GameObject icon = GameObject.CreatePrimitive(PrimitiveType.Quad);
                //icon.transform.position = new Vector3(1, 1, 1);
                //icon.transform.localScale = new Vector3(1, 1, 1);
                //icon.GetComponent<Renderer>().material.mainTexture = iconTexture;
                //_nativeBannerObject = icon;
                //icon.AddComponent<MeshCollider>();
                //// Register GameObject that will display icon asset of native ad.
                //if (!_nativeAd.RegisterIconImageGameObject(icon))
                //{
                //    // Handle failure to register ad asset.
                //}

                _nativeAd = null;
            }
#endif
        }

        public void LoadInterstitialAd()
        {
            if (_interstitial == null)
            {
                CreateAndLoadInterstitialAd();
            }
        }

        private void CreateAndLoadInterstitialAd()
        {
            _interstitial = new InterstitialAd(INTERSTITIAL_ID);
            _interstitial.OnAdClosed += OnInterstitialAdClosed;
            _interstitial.OnAdFailedToLoad += OnInterstitialOnAdFailedToLoad;
            _interstitial.OnPaidEvent += InterstitialOnPaidEvent;
            _interstitial.OnAdLoaded += OnInterstitialAdLoaded;
            _interstitial.OnAdFailedToShow += OnAdFailedToShow;
            _interstitial.OnAdOpening += OnAdOpening;

            AdRequest request = new AdRequest.Builder().Build();
            _interstitial.LoadAd(request);
        }

        private void OnAdFailedToShow(object sender, AdErrorEventArgs e)
        {
            FuntoryEventHandler.OnAdShowResultFail(FuntoryEventHandler.Interstitial, _lastIntersialPlacement, e.AdError.GetCode(), e.AdError.GetMessage());
        }

        private void OnAdOpening(object sender, EventArgs e)
        {
            FuntoryEventHandler.OnAdShowResultSuccess(FuntoryEventHandler.Interstitial, _lastIntersialPlacement, _interstitial?.GetResponseInfo()?.GetMediationAdapterClassName() ?? "", _interstitial?.GetResponseInfo()?.GetResponseId() ?? "");
        }

        private void OnInterstitialAdLoaded(object sender, EventArgs e)
        {
            FuntoryEventHandler.OnAdLoadResult(FuntoryEventHandler.Interstitial, 1);
        }

        private void OnInterstitialOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
        {
            _interstitial = null;
            FuntoryEventHandler.OnAdLoadResult(FuntoryEventHandler.Interstitial, 0, e.LoadAdError.GetCode(), e.LoadAdError.GetMessage());
        }

        private void OnInterstitialAdClosed(object sender, EventArgs e)
        {

        }

        private void InterstitialOnPaidEvent(object sender, AdValueEventArgs e)
        {
            FuntoryEventHandler.OnAdRevenue(FuntoryEventHandler.Interstitial, _interstitial?.GetResponseInfo()?.GetMediationAdapterClassName() ?? "", e.AdValue.CurrencyCode, e.AdValue.Value, e.AdValue.Precision.ToString());
        }

        public EAdState ShowInterstitialAd()
        {
            if (_interstitial == null)
                return EAdState.NotRequest;

            if (!_interstitial.IsLoaded())
                return EAdState.NotLoaded;



            _interstitial.Show();
            _interstitial = null;
            return EAdState.Success;
            //if (_interstitial != null && _interstitial.IsLoaded())
            //{
            //    _interstitial.Show();
            //    _gameService.GameDataModel.InterstitialCount++;
            //    _event.BroadcastEvent(EEventType.OnAdSeen);
            //}
            //else
            //{
            //    if (_interstitial == null)
            //    {
            //        FuntoryEventHandler.OnAdShowResultFail(FuntoryEventHandler.Interstitial, placement, 0, "InterstitialAd is null");
            //    }
            //    else if (!_interstitial.IsLoaded())
            //    {
            //        FuntoryEventHandler.OnAdShowResultFail(FuntoryEventHandler.Interstitial, placement, 0, "InterstitialAd not loaded");
            //    }
            //    Debug.LogError("show Interstitial error");
            //}


            //if (_interstitial.IsLoaded())
            //    _interstitial.Show();
            //else
            //    Debug.LogError("show Interstitial error");
        }



        public void HideNativeBannerAd()
        {
            _nativeBannerObject.SetActive(false);
        }

        public void LoadVideoAd()
        {
            if (_rewardedAd != null)
            {
                AdRequest request = new AdRequest.Builder().Build();
                //request.se.Add("A4B0860103793A14D30DB96346BD45BA");
                Debug.Log("_rewardedAd: " + _rewardedAd);
                _rewardedAd.LoadAd(request);
            }
        }

        public EVideoAdState ShowVideoAd(Action<bool, bool> OnAdVideoDone)
        {
            _onAdVideoDone = OnAdVideoDone;
            if (_rewardedAd.IsLoaded())
            {
                _rewardedAd.Show();
                return EVideoAdState.Success;
            }
            return EVideoAdState.NoExist;
        }

#if NATIVE_AD
        private void HandleUnifiedNativeAdLoaded(object sender, UnifiedNativeAdEventArgs args)
        {
            MonoBehaviour.print("Unified native ad loaded.");
            _nativeAd = args.nativeAd;
            _isUnifiedNativeAdLoaded = true;
        }
#endif

        private void HandleRewardedAdClosed(object sender, EventArgs e)
        {
            return;
        }

        private void HandleUserEarnedReward(object sender, Reward e)
        {
            bool hasReward = e.Amount > 0;
            bool isComplete = true;
            _onAdVideoDone?.Invoke(isComplete, hasReward);
        }

        private void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs e)
        {
            return;
        }

        private void HandleRewardedAdOpening(object sender, EventArgs e)
        {
            return;
        }

        private void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs arg)
        {
            Debug.LogWarning(arg.LoadAdError);
            _onLoadVideoFailed?.Invoke(0);
            return;
        }

        private void HandleRewardedAdLoaded(object sender, EventArgs e)
        {
            Debug.LogWarning("HandleRewardedAdLoaded");
            _onVideoAvailable?.Invoke();
        }


    }
#endif
}

