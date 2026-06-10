using MemoAna.Domain.Entities;
using MemoAna.Domain.Enums;

namespace MemoAna.Application.Dtos;

public sealed record GameStatisticsDto(string ThemeName, GameDifficulty Difficulty, DateTime PlayedAt, bool IsVictory, int RemainingSeconds, int TotalMoves, int SuccessfulMoves, int Mistakes, int FinalScore)
{
    public static readonly GameStatisticsDto Default = new(string.Empty, (GameDifficulty)3, DateTime.Today, false, 0, 0, 0, 0, 0);
}
