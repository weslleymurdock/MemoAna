using MemoAna.Game.Core;
using MemoAna.Game.EventArgs;

namespace MemoAna.Game.Abstract.Services;

public interface IGameService
{
    ObservableCollection<MemoryCard> CurrentCards { get; }
    TimeSpan RemainingTime { get; }
    bool IsGameActive { get; }
    int CurrentScore { get; }
    int TotalMoves { get; }

    event EventHandler<GameStatisticsEventArgs>? GameFinished; 
    event EventHandler<GameTickEventArgs>? TimerTick;
    
    Task FlipCardAsync(MemoryCard selectedCard); 
    Task StartGameAsync(int difficulty, string themeName);
    void ForceStopTimer();
}
