using MemoAna.Application.Dtos;
using MemoAna.Domain.Args;
using MemoAna.Domain.Enums;

namespace MemoAna.Domain.Entities;

public sealed class GameStatisticsEntity : BaseEntity
{
    public string ThemeName { get; set; } = string.Empty;
    public GameDifficulty Difficulty { get; set; }
    public DateTime PlayedAt { get; set; }
    public bool IsVictory { get; set; }

    public int RemainingSeconds { get; set; }
    public int TotalMoves { get; set; }
    public int SuccessfulMoves { get; set; }
    public int Mistakes { get; set; }
    public int FinalScore { get; set; }

    public GameStatisticsEntity(string id) : base(id) { }

    public GameStatisticsEntity() : base(Guid.NewGuid().ToString())
    {
        PlayedAt = DateTime.UtcNow;
    }

    internal GameStatisticsEventArgs ToEventArgs()
        => new((this is GameStatisticsEntity gs ? gs.ThemeName : throw new ArgumentNullException(nameof(gs))), gs.Difficulty, DateTime.FromFileTimeUtc(gs.PlayedAt.ToFileTime()), gs.IsVictory, gs.RemainingSeconds, gs.TotalMoves, gs.SuccessfulMoves, gs.Mistakes, gs.FinalScore);
}