using MemoAna.Application.Core;
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

    event EventHandler<GameStatisticsEntity>? GameFinished; 
    event EventHandler<GameTickEntity>? TimerTick;
    
    Task FlipCardAsync(MemoryCard selectedCard); 
    Task StartGameAsync(GameDifficulty difficulty, string themeName);
    void ForceStopTimer();
}
