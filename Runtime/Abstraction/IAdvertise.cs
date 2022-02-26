using System;

namespace GameWarriors.AdDomain.Abstraction
{
    public enum EAdHandlerType { None, Tapsell, Admobe }
    public enum EUnitAdType { None, AppId, InterstitalId, NativeBannerId, RewardAdId, BannerId }
    public enum EVideoAdState { None, Success, NoExist, NoInternet }

    public interface IAdvertise
    {
        bool IsVideoAdExist { get; }

        event Action OnVideoAvailable;
        void ShowVideoAd(Action<bool, bool> adVideoShowDone, Action adVideoShowFailed, bool isShowInterstitial = false);
        void LoadVideoAd(bool isLoadInterstitial = false);
        IAdBannerHandler RequestAdBanner();
    }
}
