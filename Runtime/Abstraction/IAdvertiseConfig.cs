
using UnityEngine;

namespace GameWarriors.AdDomain.Abstraction
{
    public interface IAdvertiseConfig
    {
        public MonoBehaviour CoroutineHandler { get; }
        string GetAdUnitId(EAdHandlerType handlerType, EUnitAdType unitType);
        GameObject AdUnitNativeBanner(EAdHandlerType type);
    }
}