using System;

namespace GameWarriors.AdDomain.Abstraction
{
    public interface IAdVideoHandler
    {
        void Setup(Action onInitializeDone);
        IRewardedAd LoadVideoAd(IRewardedAdPlace place, IRewardedEventListener listener);
    }
}
