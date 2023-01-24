namespace GameWarriors.AdDomain.Abstraction
{
    public interface IAdInterstitialHandler
    {
        bool IsInterstitialAvailable { get; }
        void LoadInterstitialAd();
        void ShowInterstitialAd();
    }
}
