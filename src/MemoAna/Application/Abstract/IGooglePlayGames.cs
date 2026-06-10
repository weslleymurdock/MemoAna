namespace MemoAna.Application.Abstract
{
    public interface IGooglePlayGames
    {
        void Initialize(string accessToken);
        Task<Google.Apis.Requests.IDirectResponseSchema> UnlockAchievementAsync(string achievementId);
        Task<Google.Apis.Requests.IDirectResponseSchema> SubmitScoreAsync(string leaderboardId, long scoreValue);
    }
}