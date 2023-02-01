

namespace GameWarriors.AdDomain.Abstraction
{
    public interface IRewardedEventListener
    {
        void OnRewardedAdLoaded(IRewardedAd interstitialAd, IRewardedAdPlace place);
        void OnRewardedAdLoadedFailed(IRewardedAd interstitialAd, IRewardedAdPlace place);
    }
}