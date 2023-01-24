using GameWarriors.AdDomain.Abstraction;
using System;
using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

namespace GameWarriors.AdDomain.Core
{
    public class AdSystem : IAdvertise
    {
        public event Action OnVideoAvailable;
        public event Action<EVideoAdState> OnVideoUnavailable;

        private IAdVideoHandler _videoHandler;
        private IAdBannerHandler _bannerHandler;
        private IAdNativeBannerHandler _nativeBannerHandler;
        private IAdInterstitialHandler _interstitialHandler;
        private IAdvertiseConfig _adConfig;
        private bool _isRequestingVideo;

        public bool IsVideoAdExist => _videoHandler?.IsVideoAvailable ?? false;
        public bool IsInterstitialAdExist => _interstitialHandler?.IsInterstitialAvailable ?? false;

        [UnityEngine.Scripting.Preserve]
        public AdSystem(IAdvertiseConfig adConfig,IAdBannerHandler bannerHandler, IAdVideoHandler adVideoHandler, IAdNativeBannerHandler adNativeBannerHandler, IAdInterstitialHandler adInterstitialHandler)
        {
            OnVideoAvailable = null;
            OnVideoUnavailable = null;
            _adConfig = adConfig;
            _bannerHandler = bannerHandler;
            _videoHandler = adVideoHandler;
            _nativeBannerHandler = adNativeBannerHandler;
            _interstitialHandler = adInterstitialHandler;
        }

        [UnityEngine.Scripting.Preserve]
        public async Task WaitForLoading()
        {
            if (_videoHandler != null)
            {
                await Task.Delay(200);
                _videoHandler.Setup(() => { LoadVideoAd(); }, OnLoadVideoAdSuccess, OnLoadVideoAdFailed);
            }
        }

        public void LoadVideoAd(bool isLoadInterstitial = false)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return;
            }

            if (_videoHandler == null || (_videoHandler?.IsVideoAvailable ?? false) || _isRequestingVideo)
                return;
            if (isLoadInterstitial)
                LoadInterstitialAd();
            _isRequestingVideo = true;
            _videoHandler?.LoadVideoAd();
        }

        public void ShowVideoAd(Action<bool, bool> onShowVideoAdDone, Action onShowVideoAdFailed, bool isShowInterstitial = false)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                onShowVideoAdFailed?.Invoke();
                return;
            }

            EVideoAdState result = _videoHandler?.ShowVideoAd(onShowVideoAdDone) ?? EVideoAdState.None;
            Debug.Log("VideoAdState: " + result);
            if (result == EVideoAdState.Success)
                LoadVideoAd();
            else
                OnVideoUnavailable?.Invoke(result);
        }

        public void LoadInterstitialAd()
        {
            _interstitialHandler?.LoadInterstitialAd();
        }

        public void ShowInterstitialAd()
        {
            _interstitialHandler?.ShowInterstitialAd();
        }

        public void LoadNativeBannerAd()
        {
            _nativeBannerHandler?.LoadNativeBannerAd();
        }

        public IAdBannerHandler RequestAdBanner()
        {
            return _bannerHandler;
        }

        public void ShowNativeBannerAd()
        {
            _nativeBannerHandler?.ShowNativeBannerAd();
        }

        public void DisableNativeBanner()
        {
            _nativeBannerHandler?.HideNativeBannerAd();
        }

        private void OnLoadVideoAdSuccess()
        {
            _isRequestingVideo = false;
            OnVideoAvailable?.Invoke();
        }

        private void OnLoadVideoAdFailed(EVideoAdState state)
        {
            _isRequestingVideo = false;
            Debug.Log("OnLoadVideoAdFailed: " + state);
            switch (state)
            {
                case EVideoAdState.NoExist: _adConfig.CoroutineHandler.StartCoroutine(CheckVideo(_adConfig.WaitAfterNotExsit)); break;
                case EVideoAdState.NoInternet: _adConfig.CoroutineHandler.StartCoroutine(CheckVideo(_adConfig.WaitAfterNoInternet)); break;
                case EVideoAdState.Success: LoadVideoAd(); break;
                default:
                    break;
            }
        }

        private IEnumerator CheckVideo(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (!_isRequestingVideo)
                LoadVideoAd();
            else
                _adConfig.CoroutineHandler.StartCoroutine(CheckVideo(_adConfig.DelayWhenIsRequesting));
        }
    }
}
