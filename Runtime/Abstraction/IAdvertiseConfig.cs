
using UnityEngine;

namespace GameWarriors.AdDomain.Abstraction
{
    public interface IAdvertiseConfig
    {
        public MonoBehaviour CoroutineHandler { get; }
        float WaitAfterNotExsit { get; }
        float WaitAfterNoInternet { get; }
        float DelayWhenIsRequesting { get; }

        string GetAdUnitId(EAdHandlerType handlerType, EUnitAdType unitType);
        GameObject AdUnitNativeBanner(EAdHandlerType type);

        IRewardedAdPlace DefaultVideoAdPlace { get; }
        IInterstitialAdPlace DefaultInterstitialPlace { get; }
    }
}