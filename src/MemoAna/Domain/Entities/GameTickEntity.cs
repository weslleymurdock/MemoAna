namespace MemoAna.Domain.Entities;

public record GameTickEntity(int RemainingSeconds)
{
    public string NextTime { get => TimeSpan.FromSeconds(RemainingSeconds).ToString(@"mm\:ss"); }
}
