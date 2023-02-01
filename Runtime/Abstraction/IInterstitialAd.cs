using System;

namespace GameWarriors.AdDomain.Abstraction
{
    public interface IInterstitialAd : IDisposable
    {
        bool IsAvailable { get; }
        EAdState Show();
    }
}