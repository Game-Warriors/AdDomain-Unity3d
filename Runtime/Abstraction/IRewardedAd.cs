namespace GameWarriors.AdDomain.Abstraction
{
    public interface IRewardedAd
    {
        bool IsAvailable { get; }
        EAdState Show();
    }
}