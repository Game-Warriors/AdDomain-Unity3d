using GameWarriors.AdDomain.Abstraction;

namespace Managements.Handlers.Advertise
{
    public class AdMobeInterstitialHandler : IAdInterstitialHandler
    {
        private readonly string INTERSTITIAL_ID;

        [UnityEngine.Scripting.Preserve]
        public AdMobeInterstitialHandler(IAdvertiseConfig advertiseConfig)
        {
            INTERSTITIAL_ID = advertiseConfig.GetAdUnitId(EAdHandlerType.Admobe, EUnitAdType.InterstitalId);
        }

        public IInterstitialAd LoadInterstitialAd(IInterstitialAdPlace place, IInterstitialEventListener listener)
        {
#if ADMOB
            Interstitial interstitial = new Interstitial(INTERSTITIAL_ID, place, listener);
            interstitial.Load();
            return interstitial;
#else
            return null;
#endif
        }
    }
}
