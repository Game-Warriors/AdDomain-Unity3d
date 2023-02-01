using GameWarriors.AdDomain.Abstraction;
using System;

namespace Managements.Handlers.Advertise
{
#if ADMOB
    using GoogleMobileAds.Api;

    public class Interstitial : IInterstitialAd
    {
        private readonly IInterstitialAdPlace _place;
        private readonly IInterstitialEventListener _listener;
        private readonly InterstitialAd _interstitial;

        public bool IsAvailable => _interstitial?.IsLoaded() ?? false;

        public Interstitial(string id, IInterstitialAdPlace place, IInterstitialEventListener listener)
        {
            _place = place;
            _listener = listener;
            _interstitial = new InterstitialAd(id);
            _interstitial.OnAdClosed += OnInterstitialAdClosed;
            _interstitial.OnAdFailedToLoad += OnInterstitialOnAdFailedToLoad;
            _interstitial.OnPaidEvent += InterstitialOnPaidEvent;
            _interstitial.OnAdLoaded += OnInterstitialAdLoaded;
            _interstitial.OnAdFailedToShow += OnAdFailedToShow;
            _interstitial.OnAdOpening += OnAdOpening;
        }

        public void Load()
        {
            AdRequest request = new AdRequest.Builder().Build();
            _interstitial.LoadAd(request);
        }

        private void OnInterstitialAdLoaded(object sender, EventArgs e)
        {
            _listener.OnInterstitialAdLoaded(this, _place);
            _place?.OnInterstitialLoaded();
        }

        private void OnInterstitialOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
        {
            _listener.OnInterstitialAdLoadedFailed(this, _place);
            _place?.OnInterstitialOnAdLoadFailed(e.LoadAdError.GetCode(), e.LoadAdError.GetMessage());
        }

        private void OnAdFailedToShow(object sender, AdErrorEventArgs e)
        {
            _place?.OnInterstitialShowFailed(EAdState.Failed, e.AdError.GetCode(), e.AdError.GetMessage());
        }

        private void OnAdOpening(object sender, EventArgs e)
        {
            string mediationName = _interstitial?.GetResponseInfo()?.GetMediationAdapterClassName() ?? string.Empty;
            string response = _interstitial?.GetResponseInfo()?.GetResponseId() ?? string.Empty;
            _place?.OnInterstitialOpen(mediationName, response);
        }

        private void OnInterstitialAdClosed(object sender, EventArgs e)
        {
            _place?.OnInterstitialAdClosed();
        }

        private void InterstitialOnPaidEvent(object sender, AdValueEventArgs e)
        {
            string mediationName = _interstitial?.GetResponseInfo()?.GetMediationAdapterClassName() ?? string.Empty;
            string currencyCode = e.AdValue.CurrencyCode;
            long value = e.AdValue.Value;
            string precision = e.AdValue.Precision.ToString();
            _place?.OnInterstitialPaidData(mediationName, currencyCode, value, precision);
        }

        public EAdState Show()
        {
            if (_interstitial == null)
            {
                return EAdState.NotRequest;
            }

            if (!_interstitial.IsLoaded())
            {
                return EAdState.NotLoaded;
            }

            _interstitial.Show();
            return EAdState.Success;
        }

        public void Dispose()
        {
            _interstitial.OnAdClosed -= OnInterstitialAdClosed;
            _interstitial.OnAdFailedToLoad -= OnInterstitialOnAdFailedToLoad;
            _interstitial.OnPaidEvent -= InterstitialOnPaidEvent;
            _interstitial.OnAdLoaded -= OnInterstitialAdLoaded;
            _interstitial.OnAdFailedToShow -= OnAdFailedToShow;
            _interstitial.OnAdOpening -= OnAdOpening;
            _interstitial.Destroy();
        }
    }
#endif
}