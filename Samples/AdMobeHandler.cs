//#define USE_ADMOBE

using UnityEngine;
using UnityEngine.UI;
using System;

//#if USE_ADMOBE


namespace Managements.Handlers.Advertise
{
    using GameWarriors.AdDomain.Abstraction;

#if ADMOB
    using GoogleMobileAds.Api;
    using System.Collections.Generic;

    public class AdMobeHandler : IAdVideoHandler, IAdNativeBannerHandler
    {

        private readonly string NATIVE_BANNER_ID;

        private readonly string REWARD_AD_ID;
        private GameObject _nativeBannerObject;

        //private AdLoader _nativeAdLoader;
        private bool _isUnifiedNativeAdLoaded;
        private List<string> _testDevices;

#if NATIVE_AD
        private UnifiedNativeAd _nativeAd;
        public bool HasBanner => _nativeAd != null;
#else
        public bool HasBanner => false;
#endif

        //https://developers.google.com/admob/unity/test-ads
        [UnityEngine.Scripting.Preserve]
        public AdMobeHandler(IAdvertiseConfig advertiseConfig)
        {
            _nativeBannerObject = advertiseConfig.AdUnitNativeBanner(EAdHandlerType.Admobe);
            NATIVE_BANNER_ID = advertiseConfig.GetAdUnitId(EAdHandlerType.Admobe, EUnitAdType.InterstitalId);

            REWARD_AD_ID = advertiseConfig.GetAdUnitId(EAdHandlerType.Admobe, EUnitAdType.RewardAdId);
            _testDevices = new List<string>() { { "A4B0860103793A14D30DB96346BD45BA" } };
            Application.RequestAdvertisingIdentifierAsync(GetId);
        }

        private void GetId(string advertisingId, bool trackingEnabled, string errorMsg)
        {
            Debug.Log(advertisingId);
        }


        public void Setup(Action onInitializeDone)
        {
            MobileAds.Initialize((input) => { InitializeResult(input, onInitializeDone); });
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

        public void HideNativeBannerAd()
        {
            _nativeBannerObject.SetActive(false);
        }

        public IRewardedAd LoadVideoAd(IRewardedAdPlace place, IRewardedEventListener listener)
        {
            Rewarded rewarded = new Rewarded(REWARD_AD_ID, place, listener);
            rewarded.Load();
            return rewarded;
        }

#if NATIVE_AD
        private void HandleUnifiedNativeAdLoaded(object sender, UnifiedNativeAdEventArgs args)
        {
            MonoBehaviour.print("Unified native ad loaded.");
            _nativeAd = args.nativeAd;
            _isUnifiedNativeAdLoaded = true;
        }
#endif
    }
#endif
}