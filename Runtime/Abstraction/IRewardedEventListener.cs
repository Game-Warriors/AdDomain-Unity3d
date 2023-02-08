

namespace GameWarriors.AdDomain.Abstraction
{
    public interface IRewardedEventListener
    {
        void OnRewardedAdLoaded(IRewardedAd interstitialAd, IRewardedAdPlace place);
        void OnRewardedAdFailed(IRewardedAd interstitialAd, IRewardedAdPlace place);
        void OnRewardedAdClosed(IRewardedAd interstitialAd, IRewardedAdPlace place);
    }
}