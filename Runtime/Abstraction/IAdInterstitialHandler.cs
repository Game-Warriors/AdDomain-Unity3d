using System;

namespace GameWarriors.AdDomain.Abstraction
{
    public interface IAdInterstitialHandler
    {
        IInterstitialAd LoadInterstitialAd(IInterstitialAdPlace place, IInterstitialEventListener listener);
    }
}
