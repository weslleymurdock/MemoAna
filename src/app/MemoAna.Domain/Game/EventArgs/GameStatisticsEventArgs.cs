using MemoAna.Domain.Game.Enums;

namespace MemoAna.Domain.Game.EventArgs;

public sealed record GameStatisticsEventArgs(string ThemeName, GameDifficulty Difficulty, DateTime PlayedAt, bool IsVictory, int RemainingSeconds, int TotalMoves, int SuccessfulMoves, int Mistakes, int FinalScore);
