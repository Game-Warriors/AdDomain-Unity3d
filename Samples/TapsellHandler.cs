using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameWarriors.AdDomain.Abstraction;
#if TAPSELL
using TapsellSDK;
#endif

namespace Managements.Handlers.Advertise
{
#if TAPSELL
    
    public class TapsellHandler : IAdVideoHnadler, IAdNativeBannerHandler, IAdInterstitialHandler
    {
        private const string TAPSELL_APPID_KEY = "TAPSELL_APPID";
        private const string TAPSELL_VIDEO_KEY = "TAPSELL_VIDEO";
        private const string TAPSELL_INTERSTITIAL_KEY = "TAPSELL_INTERSTITIAL";
        private const string TAPSELL_NATIVE_BANNER_KEY = "TAPSELL_NATIVE_BANNER";


        private readonly string VIDEO_ZONEID;
        private readonly string INTERSTITIAL_ZONEID;
        private readonly string NATIVE_BANNER_ZONEID;
        private GameObject _nativeBannerObject;
        private TapsellNativeBannerAd _nativeAd;
        private TapsellAd _ad = null;
        private TapsellAd _interstitialAd;

        public bool IsVideoAvailable => _ad != null;
        public bool HasInterstitial => _interstitialAd != null;
        public bool HasBanner => _nativeAd != null;


        public TapsellHandler(Func<string, string> unitIdTable, GameObject nativeBannerObject)
        {
            _nativeBannerObject = nativeBannerObject;

            string appId = unitIdTable?.Invoke(TAPSELL_APPID_KEY);
            appId = string.IsNullOrEmpty(appId) ? "hntqcqstnohehgpfimsedqemjoojcpmetqtfhsogqokjbndbjbdnpeotrcmjmpgdsoljpn" : appId;

            VIDEO_ZONEID = unitIdTable?.Invoke(TAPSELL_VIDEO_KEY);
            VIDEO_ZONEID = string.IsNullOrEmpty(VIDEO_ZONEID) ? "5d4d24f931518a00017bdb40" : VIDEO_ZONEID;

            INTERSTITIAL_ZONEID = unitIdTable?.Invoke(TAPSELL_INTERSTITIAL_KEY);
            INTERSTITIAL_ZONEID = string.IsNullOrEmpty(INTERSTITIAL_ZONEID) ? "5e6fa14206f9e200016b2c99" : INTERSTITIAL_ZONEID;

            NATIVE_BANNER_ZONEID = unitIdTable?.Invoke(TAPSELL_NATIVE_BANNER_KEY);
            NATIVE_BANNER_ZONEID = string.IsNullOrEmpty(NATIVE_BANNER_ZONEID) ? "5e6fa12606f9e200016b2c98" : NATIVE_BANNER_ZONEID;

            Tapsell.Initialize(appId);
        }

        public void LoadNativeBannerAd(MonoBehaviour monoBehaviour)
        {
            Tapsell.RequestNativeBannerAd(monoBehaviour, NATIVE_BANNER_ZONEID,
            (onRequestFilled) =>
            {
                _nativeAd = onRequestFilled;
                Debug.Log("++++++++++++ nativeAd : " + Newtonsoft.Json.JsonConvert.SerializeObject(_nativeAd.description));
            },
            (onNoAdAvalable) =>
            {
                Debug.Log("++++++++++++ onNoAdAvalable : " + onNoAdAvalable);

            },
            (OnErorrAction) =>
            {

                Debug.Log("++++++++++++ OnErorrAction : " + OnErorrAction.message);
            },
            (OnNoNetwork) =>
            {

                Debug.Log("++++++++++++ OnNoNetwork : " + OnNoNetwork);
            });
        }

        public void ShowNativeBannerAd()
        {
            if (_nativeAd == null)
                return;
            _nativeBannerObject.SetActive(true);
            Texture2D bannerImage = _nativeAd.GetLandscapeBannerImage();
            if (bannerImage == null)
                bannerImage = _nativeAd.GetPortraitBannerImage();

            _nativeBannerObject.GetComponent<Image>().sprite = Sprite.Create(bannerImage, new Rect(0, 0, bannerImage.width, bannerImage.height), new Vector2(0.5f, 0.5f));
            Button button = _nativeBannerObject.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(_nativeAd.Clicked);
        }

        public void HideNativeBannerAd()
        {
            _nativeBannerObject.SetActive(false);
        }

        public void LoadInterstitialAd()
        {
            if (HasInterstitial)
                return;
            Tapsell.RequestAd(INTERSTITIAL_ZONEID, true,
                (onAdAvailableAction) =>
                {
                    _interstitialAd = onAdAvailableAction;
                },
                (onNoAdAvailableAction) =>
                {
                },
                (onErrorAction) =>
                {
                },
                (onNoNetworkAction) =>
                {
                },
                (onExpiringAction) =>
                {
                });
        }

        public void ShowInterstitialAd()
        {
            if (HasInterstitial)
            {
                TapsellShowOptions showOptions = new TapsellShowOptions
                {
                    backDisabled = false,
                    immersiveMode = false,
                    rotationMode = TapsellShowOptions.ROTATION_UNLOCKED,
                    showDialog = false
                };

                Tapsell.ShowAd(_interstitialAd, showOptions);
                _interstitialAd = null;
            }
        }

        public void LoadVideoAd(Action OnVideoAvailable, Action<int> OnVideoUnavailable)
        {
            _ad = null;
            Tapsell.RequestAd(VIDEO_ZONEID, false, (inputAd) => { _ad = inputAd; OnVideoAvailable?.Invoke(); },

            (string zoneId) =>
            {
                // onNoAdAvailable
                _ad = null;

                OnVideoUnavailable?.Invoke(1);
            },

            (TapsellError error) =>
            {
                // onError
                _ad = null;

                OnVideoUnavailable?.Invoke(2);
            },

            (string zoneId) =>
            {
                // onNoNetwork
                _ad = null;
                OnVideoUnavailable?.Invoke(3);
            },

            (TapsellAd result) =>
            {
                // onExpiring
                _ad = null;
                OnVideoUnavailable?.Invoke(4);

                //Debug.Log("Expiring");
                // this ad is expired, you must download a new ad for this zone
            }
            );
        }

        public int ShowVideoAd(Action<bool, bool> OnAvailable, Action OnAdNotAvailable)
        {
            if (_ad == null)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    _ad = null;
                    OnAdNotAvailable?.Invoke();
                    return 1;
                }

                OnAdNotAvailable?.Invoke();
                return 2;
            }
            TapsellShowOptions showOptions = new TapsellShowOptions
            {
                backDisabled = false,
                immersiveMode = false,
                rotationMode = TapsellShowOptions.ROTATION_UNLOCKED,
                showDialog = true
            };
            Tapsell.ShowAd(_ad, showOptions);
            Tapsell.SetRewardListener((input) => { OnAvailable(input.completed, input.rewarded); });
            _ad = null;
            return 0;
        }

    }
#endif

}

