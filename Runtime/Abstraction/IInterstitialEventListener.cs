
namespace GameWarriors.AdDomain.Abstraction
{
    public interface IInterstitialEventListener
    {
        void OnInterstitialAdLoaded(IInterstitialAd interstitialAd, IInterstitialAdPlace place);
        void OnInterstitialAdLoadedFailed(IInterstitialAd interstitialAd, IInterstitialAdPlace place);
    }
}