
namespace GameWarriors.AdDomain.Abstraction
{
    public interface IInterstitialEventListener
    {
        void OnInterstitialAdLoaded(IInterstitialAd interstitialAd, IInterstitialAdPlace place);
        void OnInterstitialAdFailed(IInterstitialAd interstitialAd, IInterstitialAdPlace place);
        void OnInterstitialAdClosed(IInterstitialAd interstitialAd, IInterstitialAdPlace place);
    }
}