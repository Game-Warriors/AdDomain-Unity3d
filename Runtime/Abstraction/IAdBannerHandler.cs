using System;

namespace GameWarriors.AdDomain.Abstraction
{
    public enum EBannerType { None, Top, Middle, Down }

    public interface IAdBannerHandler
    {
        public event Action<bool> OnLoadDone;
        public event Action OnAdOpen;

        public bool IsLoad { get; }

        void LoadBanner(EBannerType bannerType);

        void ShowBanner();

        void HideBanner();
    }
}