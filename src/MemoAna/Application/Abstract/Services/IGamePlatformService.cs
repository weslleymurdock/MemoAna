namespace MemoAna.Application.Abstract.Services;

public interface IGamePlatformService
{
    void Initialize();
    Task<bool> SilentLoginAsync();
    void SendScoreToBoard(string boardId, long score);
    void UnlockAchievement(string achievementId);
}