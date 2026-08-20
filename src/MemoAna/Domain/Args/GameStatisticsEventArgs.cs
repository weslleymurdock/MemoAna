using MemoAna.Application.Dtos;
using MemoAna.Domain.Entities;
using MemoAna.Domain.Enums;

namespace MemoAna.Domain.Args;

public sealed record GameStatisticsEventArgs(string ThemeName, GameDifficulty Difficulty, DateTime PlayedAt, bool IsVictory, int RemainingSeconds, int TotalMoves, int SuccessfulMoves, int Mistakes, int FinalScore)
{
    internal GameStatisticsDto ToDto()
        => new(
            (this is GameStatisticsEventArgs gameStatisticsEntity ?
            gameStatisticsEntity.ThemeName :
            throw new ArgumentNullException(nameof(gameStatisticsEntity))),
            gameStatisticsEntity.Difficulty,
            DateTime.FromFileTimeUtc(gameStatisticsEntity.PlayedAt.ToFileTime()),
            gameStatisticsEntity.IsVictory,
            gameStatisticsEntity.RemainingSeconds,
            gameStatisticsEntity.TotalMoves,
            gameStatisticsEntity.SuccessfulMoves,
            gameStatisticsEntity.Mistakes,
            gameStatisticsEntity.FinalScore);
}

