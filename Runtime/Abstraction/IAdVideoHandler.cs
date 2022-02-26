using System;

namespace GameWarriors.AdDomain.Abstraction
{
    public interface IAdVideoHandler
    {
        bool IsVideoAvailable { get; }
        void Initialization(Action onInitializeDone,Action onVideoAvailable, Action<EVideoAdState> onVideoUnavailable);
        void LoadVideoAd();
        EVideoAdState ShowVideoAd(Action<bool, bool> onAdVideoDone);
    }
}
