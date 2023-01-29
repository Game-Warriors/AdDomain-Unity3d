namespace GameWarriors.AdDomain.Abstraction
{
    public interface IInterstitialAdPlace
    {
        string Id { get; }
        void OnInterstitialLoaded();
        void OnInterstitialOnAdLoadFailed(int statusCode, string message);
        void OnInterstitialOpen(string madiationName, string response);
        void OnInterstitialShowFailed(EAdState state, int statusCode, string message);
        void OnInterstitialPaidData(string madiationName, string correncyCode, long amount, string precision);
        void OnInterstitialAdClosed();
    }
}