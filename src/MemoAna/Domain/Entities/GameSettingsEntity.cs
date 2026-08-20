using MemoAna.Domain.Models;

namespace MemoAna.Domain.Entities;

public class GameSettingsEntity : BaseEntity
{
    public GameSettingsEntity() : base(Guid.NewGuid().ToString()) { }

    public GameSettingsEntity(string id) : base(id) { }

    public GameOptions Options { get; set; } = new();
}