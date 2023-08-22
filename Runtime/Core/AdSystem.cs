using GameWarriors.AdDomain.Abstraction;
using System;
using System.Collections;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GameWarriors.AdDomain.Core
{
    public class AdSystem : IAdvertise, IInterstitialEventListener, IRewardedEventListener
    {
        public event Action<IRewardedAdPlace> OnVideoAvailable;
        public event Action<EAdState> OnVideoUnavailable;

        private IAdVideoHandler _videoHandler;
        private IAdBannerHandler _bannerHandler;
        private IAdNativeBannerHandler _nativeBannerHandler;
        private IAdInterstitialHandler _interstitialHandler;
        private IAdvertiseConfig _adConfig;

        private readonly Dictionary<IInterstitialAdPlace, IInterstitialAd> _interstitialTable;
        private readonly Dictionary<IRewardedAdPlace, IRewardedAd> _rewardedTable;

        public bool IsAnyVideoAdExist
        {
            get
            {
                foreach (IRewardedAd item in _rewardedTable.Values)
                {
                    if (item != null && item.IsAvailable)
                        return true;
                }
                return false;
            }
        }

        public bool IsAnyInterstitialExist
        {
            get
            {
                foreach (IInterstitialAd item in _interstitialTable.Values)
                {
                    if (item != null && item.IsAvailable)
                        return true;
                }
                return false;
            }
        }

        [UnityEngine.Scripting.Preserve]
        public AdSystem(IAdvertiseConfig adConfig, IAdBannerHandler bannerHandler, IAdVideoHandler adVideoHandler, IAdNativeBannerHandler adNativeBannerHandler, IAdInterstitialHandler adInterstitialHandler)
        {
            OnVideoAvailable = null;
            OnVideoUnavailable = null;
            _adConfig = adConfig;
            _bannerHandler = bannerHandler;
            _videoHandler = adVideoHandler;
            _nativeBannerHandler = adNativeBannerHandler;
            _interstitialHandler = adInterstitialHandler;
            _interstitialTable = new Dictionary<IInterstitialAdPlace, IInterstitialAd>();
            _rewardedTable = new Dictionary<IRewardedAdPlace, IRewardedAd>();
        }

        [UnityEngine.Scripting.Preserve]
        public async Task WaitForLoading()
        {
            if (_adConfig.DefaultVideoAdPlace == null)
                Debug.LogError("The Default VideoAd Place is null");

            if (_adConfig.DefaultInterstitialPlace == null)
                Debug.LogError("The Default Interstitial Place is null");

            if (_videoHandler != null)
            {
                await Task.Delay(200);
                _videoHandler.Setup(() => LoadVideoAd(_adConfig.DefaultVideoAdPlace));
            }
        }


        public bool IsVideoAdExist(IRewardedAdPlace place)
        {
            place ??= _adConfig.DefaultVideoAdPlace;
            if (place == null)
                return false;
            if (_rewardedTable.TryGetValue(place, out var rewardedAd) && rewardedAd != null)
            {
                return rewardedAd.IsAvailable;
            }
            return false;
        }

        public bool IsInterstitialExist(IInterstitialAdPlace place)
        {
            place ??= _adConfig.DefaultInterstitialPlace;
            if (place == null)
                return false;
            if (_interstitialTable.TryGetValue(place, out var interstitialAd) && interstitialAd != null)
            {
                return interstitialAd.IsAvailable;
            }
            return false;
        }

        public void LoadVideoAd(IRewardedAdPlace place)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return;
            }
            place ??= _adConfig.DefaultVideoAdPlace;
            if (place != null)
            {
                bool isAdd = AddRewarded(place);
                if (isAdd)
                    _videoHandler?.LoadVideoAd(place, this);
            }
        }

        EAdState IAdvertise.ShowVideoAd(IRewardedAdPlace place)
        {
            place ??= _adConfig.DefaultVideoAdPlace;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                _adConfig.CoroutineHandler.StartCoroutine(CheckVideo(_adConfig.WaitAfterNoInternet, place));
                //place?.OnVideoShowFailed(EAdState.NoInternet, -1, "NoConnection");
                return EAdState.NoInternet;
            }
            if (place != null)
            {
                if (!IsVideoAdExist(place))
                    return EAdState.NotRequest;
                IRewardedAd rewarded = FindRewardedAd(place);
                if (rewarded != null)
                {
                    EAdState result = rewarded.Show();
                    if (result == EAdState.Success)
                    {
                        LoadVideoAd(place);
                    }
                    else
                    {
                        _adConfig.CoroutineHandler.StartCoroutine(CheckVideo(_adConfig.WaitAfterNotExsit, place));
                        //place?.OnVideoShowFailed(EAdState.NoExist, -1, "Unavailable");
                        OnVideoUnavailable?.Invoke(result);
                    }
                    return result;
                }
                return EAdState.NotLoaded;
            }
            return EAdState.None;
        }

        public void LoadInterstitialAd(IInterstitialAdPlace place)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return;
            }

            place ??= _adConfig.DefaultInterstitialPlace;
            if (place != null)
            {
                bool isAdd = AddInterstitial(place);
                if (isAdd)
                    _interstitialHandler?.LoadInterstitialAd(place, this);
            }
        }

        EAdState IAdvertise.ShowInterstitialAd(IInterstitialAdPlace place)
        {
            place ??= _adConfig.DefaultInterstitialPlace;
            if (place != null)
            {
                if (!IsInterstitialExist(place))
                    return EAdState.NotRequest;
                IInterstitialAd interstitialAd = FindInterstitialAd(place);
                if (interstitialAd != null)
                {
                    return interstitialAd.Show();
                }
                return EAdState.NotLoaded;
            }
            return EAdState.None;
        }

        void IInterstitialEventListener.OnInterstitialAdLoaded(IInterstitialAd interstitialAd, IInterstitialAdPlace place)
        {
            UpdateInterstitial(interstitialAd, place);
        }

        void IInterstitialEventListener.OnInterstitialAdFailed(IInterstitialAd interstitialAd, IInterstitialAdPlace place)
        {
            if (place != null)
                RemoveInterstitial(place);
            interstitialAd?.Dispose();
        }

        void IInterstitialEventListener.OnInterstitialAdClosed(IInterstitialAd interstitialAd, IInterstitialAdPlace place)
        {
            if (place != null)
                RemoveInterstitial(place);
            interstitialAd?.Dispose();
        }

        void IRewardedEventListener.OnRewardedAdLoaded(IRewardedAd rewardedAd, IRewardedAdPlace place)
        {
            OnVideoAvailable?.Invoke(place);
            UpdateRewarded(rewardedAd, place);
        }

        void IRewardedEventListener.OnRewardedAdFailed(IRewardedAd rewardedAd, IRewardedAdPlace place)
        {
            if (place != null)
                RemoveRewarded(place);
            rewardedAd?.Dispose();
        }


        void IRewardedEventListener.OnRewardedAdClosed(IRewardedAd rewardedAd, IRewardedAdPlace place)
        {
            if (place != null)
                RemoveRewarded(place);
            rewardedAd?.Dispose();
        }

        private bool AddInterstitial(IInterstitialAdPlace place)
        {
            bool isAdd = false;
            lock (_interstitialTable)
                isAdd = _interstitialTable.TryAdd(place, null);
            return isAdd;
        }

        private void UpdateInterstitial(IInterstitialAd interstitialAd, IInterstitialAdPlace place)
        {
            lock (_interstitialTable)
            {
                _interstitialTable[place] = interstitialAd;
            }
        }

        private void RemoveInterstitial(IInterstitialAdPlace place)
        {
            lock (_interstitialTable)
                _interstitialTable.Remove(place);
        }

        private IInterstitialAd FindInterstitialAd(IInterstitialAdPlace place)
        {
            IInterstitialAd interstitialAd = null;
            lock (_interstitialTable)
                _interstitialTable.TryGetValue(place, out interstitialAd);
            return interstitialAd;
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

        private IEnumerator CheckVideo(float seconds, IRewardedAdPlace place)
        {
            yield return new WaitForSeconds(seconds);
            if (!IsVideoAdExist(place))
                LoadVideoAd(place);
            else
                _adConfig.CoroutineHandler.StartCoroutine(CheckVideo(_adConfig.DelayWhenIsRequesting, place));
        }

        private bool AddRewarded(IRewardedAdPlace place)
        {
            bool isAdd = false;
            lock (_rewardedTable)
                isAdd = _rewardedTable.TryAdd(place, null);
            return isAdd;
        }

        private IRewardedAd FindRewardedAd(IRewardedAdPlace place)
        {
            IRewardedAd rewardedAd = null;
            lock (_rewardedTable)
                _rewardedTable.TryGetValue(place, out rewardedAd);
            return rewardedAd;
        }

        private void UpdateRewarded(IRewardedAd interstitialAd, IRewardedAdPlace place)
        {
            lock (_rewardedTable)
            {
                _rewardedTable[place] = interstitialAd;
            }
        }

        private void RemoveRewarded(IRewardedAdPlace place)
        {
            lock (_rewardedTable)
                _rewardedTable.Remove(place);
        }


    }
}
