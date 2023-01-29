using System;
using System.Collections.Generic;

namespace GameWarriors.AdDomain.Abstraction
{
    public enum EAdHandlerType { None, Tapsell, Admobe }
    public enum EUnitAdType { None, AppId, InterstitalId, NativeBannerId, RewardAdId, BannerId }
    public enum EAdState { None, Success, NoExist, NoInternet, NotLoaded, NotRequest, Failed }


    public interface IAdvertise
    {
        bool IsAnyVideoAdExist { get; }
        bool IsAnyInterstitialExist { get; }

        event Action<IRewardedAdPlace> OnVideoAvailable;

        bool IsVideoAdExist(IRewardedAdPlace place);
        EAdState ShowVideoAd(IRewardedAdPlace place);
        void LoadVideoAd(IRewardedAdPlace place);

        bool IsInterstitialExist(IInterstitialAdPlace place);
        void LoadInterstitialAd(IInterstitialAdPlace place);
        EAdState ShowInterstitialAd(IInterstitialAdPlace place);

        IAdBannerHandler RequestAdBanner();
    }
}
