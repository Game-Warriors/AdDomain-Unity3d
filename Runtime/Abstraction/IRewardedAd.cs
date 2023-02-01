using System;

namespace GameWarriors.AdDomain.Abstraction
{
    public interface IRewardedAd : IDisposable
    {
        bool IsAvailable { get; }
        EAdState Show();
    }
}