
namespace GameWarriors.AdDomain.Abstraction
{
    public interface IInterstitialEventListener
    {
        void OnInterstitialAdLoaded(IInterstitialAd interstitialAd, IInterstitialAdPlace place);
        void OnInterstitialAdLoadedFailed(IInterstitialAdPlace place);
    }
}