namespace GameWarriors.AdDomain.Abstraction
{
    public interface IRewardedAdPlace
    {
        string Id { get; }
        void OnVideoLoaded();
        void OnVideoLoadFailed(EAdState adState, int code, string message);
        void OnVideoReward(bool hasReward);
        void OnVideoOpen(string madiationName, string response);
        void OnVideoShowFailed(EAdState state, int statusCode, string message);
        void OnVideoPaidData(string madiationName, string correncyCode, long amount, string precision);
    }
}