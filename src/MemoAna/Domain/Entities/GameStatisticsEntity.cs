using MemoAna.Application.Dtos;
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

    internal GameStatisticsDto ToDto()
        => new(
            (this is GameStatisticsEntity gameStatisticsEntity ?
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