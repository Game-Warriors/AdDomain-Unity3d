namespace GameWarriors.AdDomain.Abstraction
{
    public interface IInterstitialAd
    {
        bool IsAvailable { get; }
        EAdState Show();
    }
}