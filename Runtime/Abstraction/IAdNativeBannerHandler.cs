
namespace GameWarriors.AdDomain.Abstraction
{
    public interface IAdNativeBannerHandler 
    {
        bool HasBanner { get; }
        void LoadNativeBannerAd();
        void ShowNativeBannerAd();
        void HideNativeBannerAd();
    }
}
