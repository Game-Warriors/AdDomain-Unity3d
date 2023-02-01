using GameWarriors.AdDomain.Abstraction;

using System;

namespace Managements.Handlers.Advertise
{
#if ADMOB
    using GoogleMobileAds.Api;

    public class Rewarded : IRewardedAd
    {
        private readonly IRewardedAdPlace _place;
        private readonly IRewardedEventListener _listener;
        private readonly RewardedAd _rewardedAd;

        public bool IsAvailable => _rewardedAd?.IsLoaded() ?? false;

        public Rewarded(string id, IRewardedAdPlace place, IRewardedEventListener listener)
        {
            _place = place;
            _listener = listener;
            _rewardedAd = new RewardedAd(id);
            _rewardedAd.OnAdClosed += OnInterstitialAdClosed;
            _rewardedAd.OnAdFailedToLoad += OnInterstitialOnAdFailedToLoad;
            _rewardedAd.OnPaidEvent += InterstitialOnPaidEvent;
            _rewardedAd.OnAdLoaded += OnInterstitialAdLoaded;
            _rewardedAd.OnAdFailedToShow += OnAdFailedToShow;
            _rewardedAd.OnAdOpening += OnAdOpening;
            _rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        }

        public EAdState Show()
        {
            if (_rewardedAd == null)
            {
                return EAdState.NotRequest;
            }

            if (!_rewardedAd.IsLoaded())
            {
                return EAdState.NotLoaded;
            }

            _rewardedAd.Show();
            return EAdState.Success;
        }

        public void Load()
        {
            AdRequest request = new AdRequest.Builder().Build();
            _rewardedAd.LoadAd(request);
        }

        private void OnInterstitialAdLoaded(object sender, EventArgs e)
        {
            _listener.OnRewardedAdLoaded(this, _place);
            _place?.OnVideoLoaded();
        }

        private void OnInterstitialOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
        {
            _listener.OnRewardedAdLoadedFailed(this, _place);
            _place?.OnVideoLoadFailed(EAdState.Failed, e.LoadAdError.GetCode(), e.LoadAdError.GetMessage());
        }

        private void OnAdFailedToShow(object sender, AdErrorEventArgs e)
        {
            //LoadVideoAd(_videoAdPlace);
            _place?.OnVideoShowFailed(EAdState.Failed, e.AdError.GetCode(), e.AdError.GetMessage());
        }

        private void OnAdOpening(object sender, EventArgs e)
        {
            string mediationName = _rewardedAd?.GetResponseInfo()?.GetMediationAdapterClassName() ?? string.Empty;
            string response = _rewardedAd?.GetResponseInfo()?.GetResponseId() ?? string.Empty;
            _place?.OnVideoOpen(mediationName, response);
        }

        private void OnInterstitialAdClosed(object sender, EventArgs e)
        {
            //LoadVideoAd(_videoAdPlace);
            //_place?.
        }

        private void InterstitialOnPaidEvent(object sender, AdValueEventArgs e)
        {
            string mediationName = _rewardedAd?.GetResponseInfo()?.GetMediationAdapterClassName() ?? string.Empty;
            string currencyCode = e.AdValue.CurrencyCode;
            long value = e.AdValue.Value;
            string precision = e.AdValue.Precision.ToString();
            _place?.OnVideoPaidData(mediationName, currencyCode, value, precision);
        }

        private void HandleUserEarnedReward(object sender, Reward e)
        {
            bool hasReward = e.Amount > 0;
            _place?.OnVideoReward(hasReward);
            //_onVideoAvailable?.Invoke(_videoAdPlace);
            //LoadVideoAd(_videoAdPlace);
        }

        public void Dispose()
        {
            _rewardedAd.OnAdClosed += OnInterstitialAdClosed;
            _rewardedAd.OnAdFailedToLoad += OnInterstitialOnAdFailedToLoad;
            _rewardedAd.OnPaidEvent += InterstitialOnPaidEvent;
            _rewardedAd.OnAdLoaded += OnInterstitialAdLoaded;
            _rewardedAd.OnAdFailedToShow += OnAdFailedToShow;
            _rewardedAd.OnAdOpening += OnAdOpening;
            _rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
            _rewardedAd.Destroy();
        }
    }
#endif
}
