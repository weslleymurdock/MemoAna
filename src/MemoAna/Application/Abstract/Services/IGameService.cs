using MemoAna.Application.Core;
using MemoAna.Domain.Args;
using MemoAna.Domain.Entities;
using MemoAna.Domain.Enums;
using System.Collections.ObjectModel;

namespace MemoAna.Application.Abstract.Services;

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
