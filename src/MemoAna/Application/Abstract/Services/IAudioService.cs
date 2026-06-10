namespace MemoAna.Application.Abstract.Services;

public interface IAudioService  
{
    Task PlayMainTitleAsync();
    Task PlayMainGameAsync();
    Task PlayWinAsync();
    Task PlayLoseAsync();
    Task StopAsync();
    bool IsPlaying { get; }
}
