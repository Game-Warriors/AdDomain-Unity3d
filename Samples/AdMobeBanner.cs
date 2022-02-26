using GameWarriors.AdDomain.Abstraction;
using System;

namespace Management.Handler.Advertise
{
#if ADMOB
    using GoogleMobileAds.Api;

    public class AdMobeBanner : IAdBannerHandler
    {
        private readonly string BANNER_ID;
        private BannerView _bannerView;

        public event Action<bool> OnLoadDone;
        public event Action OnAdOpen;

        public bool IsLoad { get; private set; }

        public AdMobeBanner(IAdvertiseConfig advertiseConfig)
        {
            BANNER_ID = advertiseConfig.GetAdUnitId(EAdHandlerType.Admobe, EUnitAdType.BannerId);
        }

        public void HideBanner()
        {
            _bannerView?.Hide();
        }

        public void LoadBanner(EBannerType bannerType)
        {
            if (_bannerView != null)
            {
                _bannerView.Destroy();
                _bannerView = null;
            }
            if (!string.IsNullOrEmpty(BANNER_ID))
            {
                IsLoad = false;
                _bannerView = new BannerView(BANNER_ID, AdSize.Banner, AdPosition.Bottom);
                AdRequest request = new AdRequest.Builder().Build();
                _bannerView.OnAdLoaded += AdLoadSuccess;
                _bannerView.OnAdFailedToLoad += AdLoadFailed;
                _bannerView.OnAdOpening += AdOpen;
                _bannerView.LoadAd(request);
            }
        }

        private void AdLoadSuccess(object sender, EventArgs e)
        {
            IsLoad = true;
            OnLoadDone?.Invoke(true);
        }

        private void AdLoadFailed(object sender, EventArgs e)
        {
            IsLoad = false;
            OnLoadDone?.Invoke(false);
        }

        private void AdOpen(object sender, EventArgs e)
        {
            OnAdOpen?.Invoke();
        }

        public void ShowBanner()
        {
            _bannerView?.Show();
        }
    }
#endif
}