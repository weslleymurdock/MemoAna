using MemoAna.Application.Game.Core;
using MemoAna.Domain.Game.Enums;
using MemoAna.Domain.Game.EventArgs;
using System.Collections.ObjectModel;

namespace MemoAna.Application.Game.Abstract.Services;

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
    Task StartGameAsync(GameDifficulty difficulty, string themeName);
    void ForceStopTimer();
}
